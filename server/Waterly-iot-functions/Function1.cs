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
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Documents;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Cosmos;

namespace Waterly_iot_functions
{

    //inserts events from simulation to tables (by trigger)
    public static class InsertEvent
    {
        public static CosmosClient cosmosClient = new CosmosClient("AccountEndpoint=https://waterly-iot.documents.azure.com:443/;AccountKey=cC49BNfE7uQTuEVdSNeJAUZuzTjpzl5j0MLSsb8aHGL6jGh3JmubV2TAbgxW05vYtmMA8LqTitsbRPjUZY8YsA==;");

        public static DocumentClient docClient = new DocumentClient(new Uri("https://waterly-iot.documents.azure.com:443/"), "cC49BNfE7uQTuEVdSNeJAUZuzTjpzl5j0MLSsb8aHGL6jGh3JmubV2TAbgxW05vYtmMA8LqTitsbRPjUZY8YsA==");

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
            long last_update_timestamp = dataJson.timestamp;
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
            deviceItem.last_update_timestamp = last_update_timestamp;

            ResourceResponse<Document> response = await docClient.ReplaceDocumentAsync(
                UriFactory.CreateDocumentUri("waterly_db", "waterly_devices", deviceItem.id),
                deviceItem);

            var updated = response.Resource;

            Detector.detectionPipeline(dataJson, deviceItem.userId, log);

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
                SqlQuery = "SELECT top 20 * FROM c WHERE c.device_id = {device_id} order by c.timestamp desc", //todo: remember to change here
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


        [FunctionName("update_bill_paid")]
        public static async void Run6(
            [HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "bills/{bill_id}")] HttpRequest request, //todo: make sure http request is right
                ILogger log)


        {

            string bill_id = "{bill_id}";

            Container bills_container = cosmosClient.GetContainer("waterly_db", "bills_table");

            var option = new FeedOptions { EnableCrossPartitionQuery = true };

            BillItem bill_to_pay = docClient.CreateDocumentQuery<BillItem>(
                UriFactory.CreateDocumentCollectionUri("waterly_db", "bills_table"), option)
                .Where(bill_to_pay => bill_to_pay.bill_id.Equals(bill_id))
                .AsEnumerable()
                .First();

            bill_to_pay.status = true;

            ResourceResponse<Document> response = await docClient.ReplaceDocumentAsync(
                UriFactory.CreateDocumentUri("waterly_db", "bills_table", bill_to_pay.bill_id),
                bill_to_pay);

            var updated = response.Resource;
        }


        [FunctionName("get_bills_by_user_id")]
        public static IActionResult Run7(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "bills/{userId}")] HttpRequest request,
            [CosmosDB(
                databaseName: "waterly_db",
                collectionName: "bills_table",
                ConnectionStringSetting = "CosmosDBConnection",
                SqlQuery = "SELECT * FROM c WHERE c.user_id = {userId}")]
                IEnumerable<BillItem> bills,
                ILogger log)

        {

            List<BillItem> bills_list = new List<BillItem>();

            log.LogInformation("http request for bills of user id");
            foreach (BillItem bill in bills)
            {
                log.LogInformation($"bill id is : {bill.bill_id}");
                bills_list.Add(bill);
            }
            return new OkObjectResult(bills_list);
        }

        [FunctionName("get_alerts_by_user_id")]
        public static IActionResult Run8(
    [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "notifications?user_id=${userId}")] HttpRequest request,
    [CosmosDB(
                databaseName: "waterly_db",
                collectionName: "alerts_table",
                ConnectionStringSetting = "CosmosDBConnection",
                SqlQuery = "SELECT * FROM c WHERE c.user_id = {userId}")]
                IEnumerable<AlertItem> alerts,
        ILogger log)

        {

            List<AlertItem> alerts_list = new List<AlertItem>();

            log.LogInformation("http request for bills of user id");
            foreach (AlertItem alert in alerts)
            {
                log.LogInformation($"bill id is : {alert.alert_id}");
                alerts_list.Add(alert);
            }
            return new OkObjectResult(alerts_list);
        }

        [FunctionName("create_device")]
        public static async Task Run8(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "devices/{user_id}")] HttpRequest request, ILogger log) //route?
        {
            Container devices_container = cosmosClient.GetContainer("waterly_db", "waterly_devices");


            DeviceItem deviceJson = JsonConvert.DeserializeObject<DeviceItem>("check"); //check with eyal's client how it's sent

            await devices_container.CreateItemAsync(deviceJson);
        }

        [FunctionName("delete_device")]
        public static async Task Run9(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "devices/{device_id}")] HttpRequest request, ILogger log) //route?
        {
            Container devices_container = cosmosClient.GetContainer("waterly_db", "waterly_devices");

            string device_id = "{device_id}";

            //await devices_container.DeleteItemAsync(device_id); todo:delete
        }

        [FunctionName("update_alert")]
        public static async void Run10(
            [HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "notifications/{notification.id}")] HttpRequest request,
                ILogger log)

        {
            string alert_id = "{notification.id}";

            Container alerts_container = cosmosClient.GetContainer("waterly_db", "alerts_table");

            var option = new FeedOptions { EnableCrossPartitionQuery = true };

            AlertItem alert_to_update = docClient.CreateDocumentQuery<AlertItem>(
                UriFactory.CreateDocumentCollectionUri("waterly_db", "alerts_table"), option)
                .Where(alert_to_update => alert_to_update.alert_id.Equals(alert_id))
                .AsEnumerable()
                .First();

            alert_to_update.status = false; //todo: get from the request body the current status and change it.

            ResourceResponse<Document> response = await docClient.ReplaceDocumentAsync(
                UriFactory.CreateDocumentUri("waterly_db", "alerts_table", alert_to_update.alert_id),
                alert_to_update);

            var updated = response.Resource;
        }


        // Function is called every month on the 10th at 11 AM.
        [FunctionName("create_bills_for_all_users")]
        public static async void createBill(
            [TimerTrigger("0 0 10 10 * *")]TimerInfo myTimer,
            [CosmosDB(
                databaseName: "waterly_db",
                collectionName: "users_table",
                ConnectionStringSetting = "CosmosDBConnection",
                SqlQuery = "SELECT * FROM c")]
            IEnumerable<UserItem> users,
            ILogger log)
            
        {
            if (myTimer.IsPastDue)
            {
                log.LogInformation("Timer is running late!");
            }
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.UtcNow}");

            long sumConsumption = 0;
            Dictionary<UserItem, long> userConsumptionDict = new Dictionary<UserItem, long>();
            
            foreach (UserItem user in users)
            {
                long userConsumption = await BillGenerator.calculateUserConsumption(user, log, DateTime.Today);
                userConsumptionDict.Add(user, userConsumption);
                sumConsumption += userConsumption;
            }
            double avgConsumption = sumConsumption / users.Count();
            foreach (KeyValuePair<UserItem, long> keyValuePair in userConsumptionDict)
            {
                BillGenerator.generateNewBill(keyValuePair.Key, keyValuePair.Value, DateTime.Today, avgConsumption);
            }
        }
    }
}






/*    
    FETCH_DEVICE,,
    EDIT_DEVICE,
    */