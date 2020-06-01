using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Description;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.WebJobs.Extensions.CosmosDB;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.EventHubs;
using Microsoft.Extensions.Logging;
using Bugsnag.Payload;
using Exception = System.Exception;
using Microsoft.Azure.Amqp.Framing;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Documents;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Cosmos;

namespace Waterly_iot_functions
{
    public static class InsertEvent
    {
        static CosmosClient cosmosClient = new CosmosClient("AccountEndpoint=https://waterly-iot.documents.azure.com:443/;AccountKey=cC49BNfE7uQTuEVdSNeJAUZuzTjpzl5j0MLSsb8aHGL6jGh3JmubV2TAbgxW05vYtmMA8LqTitsbRPjUZY8YsA==;");


        [FunctionName("InsertEvent")]
        public static void Run([EventHubTrigger("waterlyeventhub", Connection = "str")] EventData eventData,
            [CosmosDB(
                databaseName: "waterly_db",
                collectionName: "water_table",
                ConnectionStringSetting = "CosmosDBConnection",
                PartitionKey = "111")] out dynamic item,
    ILogger log)
        {

            log.LogInformation("C# event hub trigger function processed events.");



            string messageBody = Encoding.UTF8.GetString(eventData.Body.Array, eventData.Body.Offset, eventData.Body.Count);

            log.LogInformation($"Event message is : {messageBody}");


            EventItem dataJson = JsonConvert.DeserializeObject<EventItem>(messageBody);


            item = dataJson;


        }


        [FunctionName("update_last_water_read_for_device")]
        public static async Task Run5([EventHubTrigger("waterlyeventhub", Connection = "str")] EventData eventData,
        ILogger log)
        {

            log.LogInformation("C# event hub trigger function update rows last_water_read.");

            string messageBody = Encoding.UTF8.GetString(eventData.Body.Array, eventData.Body.Offset, eventData.Body.Count);
            EventItem dataJson = JsonConvert.DeserializeObject<EventItem>(messageBody);
            float last_water_read = dataJson.water_read;
            string device_id = dataJson.device_id;

            Container devices_container = cosmosClient.GetContainer("waterly_db", "waterly_devices");

            Microsoft.Azure.Cosmos.PartitionKey partitionKey = new Microsoft.Azure.Cosmos.PartitionKey("100034411967853931768");

            ItemResponse <DeviceItem> deviceResponse = await devices_container.ReadItemAsync<DeviceItem>(device_id, partitionKey); /*new Microsoft.Azure.Cosmos.PartitionKey("null")*/
            var itemBody = deviceResponse.Resource;

            itemBody.last_water_read = last_water_read;


            // replace the item with the updated content
            deviceResponse = await devices_container.ReplaceItemAsync<DeviceItem>(itemBody, itemBody.device_id);
       
        }
        

            [FunctionName("get_devices_by_user_id")]
        public static IActionResult Run2(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "devices/{userId}")] HttpRequest request,
            [CosmosDB(
                databaseName: "waterly_db",
                collectionName: "waterly_devices",
                ConnectionStringSetting = "CosmosDBConnection",
                SqlQuery = "SELECT * FROM c WHERE c.userId = {userId}")]
                IEnumerable<DeviceItem> devices,
                ILogger log)
            
        {

            List<DeviceItem> devicesList = new List<DeviceItem>();

            log.LogInformation("http request for devices of user id");
            foreach (DeviceItem device_item in devices)
            {
                log.LogInformation($"device id is : {device_item.device_id}");                
                devicesList.Add(device_item); 
            }
            return new OkObjectResult(devicesList);
        }


        [FunctionName("get_events_by_device_id")]
        public static IActionResult Run3(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "events/{device_id}")] HttpRequest request,
            [CosmosDB(
                databaseName: "waterly_db",
                collectionName: "water_table",
                SqlQuery = "SELECT top 20 * FROM c WHERE c.device_id = {device_id}",
                ConnectionStringSetting = "CosmosDBConnection")]
                IEnumerable<EventItem> events,
                ILogger log)


        {

            List<EventItem> eventsList = new List<EventItem>();
            log.LogInformation("http request for events of device id");
            foreach (EventItem event_item in events)
            {
                log.LogInformation($"water_read is : {event_item.water_read}");
                eventsList.Add(event_item);
            }
            return new OkObjectResult(eventsList);
        }


        [FunctionName("get_last_event_by_device_id")]
        public static IActionResult Run4(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "devices/last_event/{device_id}")] HttpRequest request,
            [CosmosDB(
                databaseName: "waterly_db",
                collectionName: "water_table",
                SqlQuery = "SELECT top 1 * FROM c WHERE c.device_id = {device_id} order by c.timestamp desc",
                ConnectionStringSetting = "CosmosDBConnection")]
                IEnumerable<EventItem> events,
                ILogger log)


        {
            List<EventItem> eventsList = new List<EventItem>();

            log.LogInformation("http request for last event of device id");
            foreach (EventItem event_item in events)
            {
                log.LogInformation($"last water_read is : {event_item.water_read}");
                eventsList.Add(event_item);

            }
            return new OkObjectResult(eventsList);
        }

    }



    
}



/*

[FunctionName("DocByIdFromQueryString")]
public static IActionResult Run(
          [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)]
                HttpRequest req,
          [CosmosDB(
                databaseName: "ToDoItems",
                collectionName: "Items",
                ConnectionStringSetting = "CosmosDBConnection",
                Id = "{Query.id}",
                PartitionKey = "{Query.partitionKey}")] ToDoItem toDoItem,
          ILogger log)
{
    log.LogInformation("C# HTTP trigger function processed a request.");

    if (toDoItem == null)
    {
        log.LogInformation($"ToDo item not found");
    }
    else
    {
        log.LogInformation($"Found ToDo item, Description={toDoItem.Description}");
    }
    return new OkResult();
}

*/

/*
 [FunctionName("InsertEvent_new")]
        public static void Run(
    [EventHubTrigger("waterlyeventhub",
            Connection = "Endpoint=sb://waterlynamespace.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=I9pJ+D7i+RA9FZ6ta64BhnxtwHBzUvIxk0IqeiUXvBo=")] string eventHubString,
    [CosmosDB(
    databaseName: "waterly_db",
    collectionName: "water_table", 
    Id = "device_id",
    ConnectionStringSetting = "AccountEndpoint=https://waterly-iot.documents.azure.com:443/;AccountKey=cC49BNfE7uQTuEVdSNeJAUZuzTjpzl5j0MLSsb8aHGL6jGh3JmubV2TAbgxW05vYtmMA8LqTitsbRPjUZY8YsA==;" )] out dynamic dbItem,
ILogger log)

        {
            log.LogInformation("C# HTTP trigger function processed an event post request.");

            eventItem dataJson = JsonConvert.DeserializeObject<EventItem>(eventHubString);

            //validate json validity
            //todo

            //todo: add timestamp to json
            dataJson.timestamp = DateTime.Now;

            dbItem = dataJson;

        }*/
