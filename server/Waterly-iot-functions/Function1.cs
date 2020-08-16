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

        static DocumentClient docClient = new DocumentClient(new Uri("https://waterly-iot.documents.azure.com:443/"), "cC49BNfE7uQTuEVdSNeJAUZuzTjpzl5j0MLSsb8aHGL6jGh3JmubV2TAbgxW05vYtmMA8LqTitsbRPjUZY8YsA==");
        
        [FunctionName("InsertEvent")]
        public static async Task Run([EventHubTrigger("waterlyeventhub", Connection = "str")] EventData eventData, ILogger log)
        {

            log.LogInformation("C# event hub trigger function processed events.");

            Container events_container = cosmosClient.GetContainer("waterly_db", "water_table");


            string messageBody = Encoding.UTF8.GetString(eventData.Body.Array, eventData.Body.Offset, eventData.Body.Count);

            log.LogInformation($"Event message is : {messageBody}");

            EventItem dataJson = JsonConvert.DeserializeObject<EventItem>(messageBody);

            await events_container.CreateItemAsync(dataJson);




            //now updates the devices table (the last water read)
            float last_water_read = dataJson.water_read;
            string device_id = dataJson.device_id;

            log.LogInformation("C# event hub trigger function update rows last_water_read.");

            Container devices_container = cosmosClient.GetContainer("waterly_db", "waterly_devices");

            var option = new FeedOptions { EnableCrossPartitionQuery = true };

            DeviceItem deviceItem = docClient.CreateDocumentQuery<DeviceItem>(
                UriFactory.CreateDocumentCollectionUri("waterly_db", "waterly_devices"), option)
                .Where(deviceItem => deviceItem.device_id.Equals(device_id))
                .AsEnumerable()
                .First();

            deviceItem.last_water_read = last_water_read;

            ResourceResponse<Document> response = await docClient.ReplaceDocumentAsync(
                UriFactory.CreateDocumentUri("waterly_db", "waterly_devices", deviceItem.id),
                deviceItem);

            var updated = response.Resource;

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
                SqlQuery = "SELECT top 20 * FROM c WHERE c.device_id = {device_id} order by c.timestamp desc",
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

    }
    
}






