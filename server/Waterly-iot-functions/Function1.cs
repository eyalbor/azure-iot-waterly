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
    public static class Function1
    {
 	[FunctionName("InsertEvent")] 
        public static async Task InsertEvent([EventHubTrigger("waterlyeventhub", Connection = "str")] EventData eventData, ILogger log)
        {

            log.LogInformation("C# event hub trigger function processed events.");
            string messageBody = Encoding.UTF8.GetString(eventData.Body.Array, eventData.Body.Offset, eventData.Body.Count);
            log.LogInformation($"Event message is : {messageBody}");
            EventItem dataJson = JsonConvert.DeserializeObject<EventItem>(messageBody);
            dataJson.id = Guid.NewGuid().ToString();
            await Resources.events_container.CreateItemAsync(dataJson);


            //now updates the devices table (the last water read)
            long last_water_read = dataJson.water_read;
            long last_update_timestamp = dataJson.timestamp;
            string device_id = dataJson.device_id;

            log.LogInformation("C# event hub trigger function update rows last_water_read.");
            var option = new FeedOptions { EnableCrossPartitionQuery = true };

            DeviceItem deviceItem = Resources.docClient.CreateDocumentQuery<DeviceItem>(
                UriFactory.CreateDocumentCollectionUri("waterly_db", "waterly_devices"), option)
                .Where(deviceItem => deviceItem.id.Equals(device_id))
                .AsEnumerable()
                .First();

            deviceItem.last_water_read = last_water_read;
            deviceItem.last_update_timestamp = last_update_timestamp;

            ResourceResponse<Document> response = await Resources.docClient.ReplaceDocumentAsync(
                UriFactory.CreateDocumentUri("waterly_db", "waterly_devices", deviceItem.id),
                deviceItem);

            var updated = response.Resource;

            //execute detector
            await Detector.executeDetectionLogic(dataJson, deviceItem.userId, log);

        }




        [FunctionName("get_devices_by_user_id")] //works
        public static IActionResult getDevicesOfUser(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "devices/userId={userId}")] HttpRequest request,
            [CosmosDB(
                databaseName: "waterly_db",
                collectionName: "waterly_devices",
                ConnectionStringSetting = "CosmosDBConnection",
                SqlQuery = "SELECT * FROM c WHERE c.userId = {userId} AND c.status = true")]
                IEnumerable<DeviceItem> devices,
                ILogger log)

        {

            List<DeviceItem> devicesList = new List<DeviceItem>();

            log.LogInformation("http request for devices of user id");
            foreach (DeviceItem device_item in devices)
            {
                log.LogInformation($"device id is : {device_item.id}");
                devicesList.Add(device_item);
            }
            return new OkObjectResult(devicesList);
        }



        [FunctionName("get_comsumption_of_user")] //todo: take the year out of the request - still doesn't work
        public static async Task<IActionResult> getUserConsumption(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "consumption_per_month/userId={userId}")] HttpRequest request,
        [CosmosDB(
                databaseName: "waterly_db",
                collectionName: "waterly_devices",
                ConnectionStringSetting = "CosmosDBConnection",
                SqlQuery = "SELECT * FROM c WHERE c.userId = {userId}")]
                IEnumerable<DeviceItem> devices,
                ILogger log)

        {
            Container BillsContainer = Resources.cosmosClient.GetContainer("waterly_db", "bill_table");
            Container ConsumptionContainer = Resources.cosmosClient.GetContainer("waterly_db", "consumption_device_month");


            //for all year, dict for month number and dict userConsumptionPerMonthDict
            Dictionary<int, Dictionary<string, long>> userConsumptionDict = new Dictionary<int, Dictionary<string, long>>();

            for (int month_num = 1; month_num < 13; month_num++)
            {
                //for each month, dict for device id and consumption sum
                Dictionary<string, long> userConsumptionPerMonthDict = new Dictionary<string, long>();

                //Add avg per month
                QueryDefinition bills_query = new QueryDefinition("SELECT top 1 avg FROM b WHERE b.month = @month_num").WithParameter("@month_num", month_num);
                FeedIterator<long> bill_iterator = BillsContainer.GetItemQueryIterator<long>(bills_query);


                Microsoft.Azure.Cosmos.FeedResponse<long> currentResultSet;

                long avg = 0;
                while (bill_iterator.HasMoreResults)
                {
                    currentResultSet = await bill_iterator.ReadNextAsync();
                    avg = currentResultSet.First();
                    break;
                } 
                userConsumptionPerMonthDict.Add("Average", avg);

                foreach (DeviceItem device_item in devices)
                {
                    //calculate sum for month_num for device_item
                    QueryDefinition consumption_query = new QueryDefinition("SELECT consumption_sum FROM c WHERE c.month = @month_num AND c.year = @year AND c.device_id = @device_id").WithParameter("@month_num", month_num).WithParameter("@year", DateTime.Today.Year).WithParameter("@device_id", device_item.id);
                    FeedIterator<long> consumption_iterator = ConsumptionContainer.GetItemQueryIterator<long>(consumption_query);
                    long consumption_per_device_month = 0;
                     
                    while (consumption_iterator.HasMoreResults)
                    {
                        currentResultSet = await consumption_iterator.ReadNextAsync();
                        consumption_per_device_month = currentResultSet.First();
                        break;
                    }


                    //add to dict
                    userConsumptionPerMonthDict.Add(device_item.id, consumption_per_device_month);
                }

                userConsumptionDict.Add(month_num, userConsumptionPerMonthDict); 
            }

            return new OkObjectResult(userConsumptionDict);
        }

            



        [FunctionName("get_events_by_device_id")] //todo- doesn't work
        public static IActionResult getEventsByDeviceId(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "events/device_id={deviceId}")] HttpRequest request,
            [CosmosDB(
                databaseName: "waterly_db",
                collectionName: "water_table",
                SqlQuery = "SELECT top 20 * FROM c WHERE c.id = {deviceId} order by c.timestamp desc", //todo: remember to change here
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


        [FunctionName("update_bill_paid")] //works
        public static async Task<IActionResult> updateBillPaid(
            [HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "bills/{bill_id}")] HttpRequest request, string bill_id,//todo: make sure http request is right
                ILogger log)

        {
       
            Container bills_container = Resources.cosmosClient.GetContainer("waterly_db", "bills_table");

            var option = new FeedOptions { EnableCrossPartitionQuery = true };

            BillItem bill_to_pay = Resources.docClient.CreateDocumentQuery<BillItem>(
                UriFactory.CreateDocumentCollectionUri("waterly_db", "bills_table"), option)
                .Where(bill_to_pay => bill_to_pay.id.Equals(bill_id))
                .AsEnumerable()
                .First();

            bill_to_pay.status = true; //todo: make sure which is paid and which is unpaid

            ResourceResponse<Document> response = await Resources.docClient.ReplaceDocumentAsync(
                UriFactory.CreateDocumentUri("waterly_db", "bills_table", bill_to_pay.id),
                bill_to_pay);

            var updated = response.Resource;

            return new OkObjectResult(bill_to_pay);
        }


        [FunctionName("get_bills_by_user_id")] //works
        public static IActionResult getBillsByUserId(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "bills/userId={userId}")] HttpRequest request,
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
                log.LogInformation($"bill id is : {bill.id}");
                bills_list.Add(bill);
            }
            return new OkObjectResult(bills_list);
        }

        [FunctionName("get_alerts_by_user_id")] //works
        public static IActionResult getAlertsByUserId(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "notifications/user_id={userId}")] HttpRequest request,
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
                log.LogInformation($"bill id is : {alert.id}");
                alerts_list.Add(alert);
            }
            return new OkObjectResult(alerts_list);
        }


        [FunctionName("get_ph_by_device_id")]
        public static IActionResult getPhByUserId(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "ph/{device_id}")] HttpRequest request, //todo: eyal - we changed the route
            [CosmosDB(
                databaseName: "waterly_db",
                collectionName: "water_table",
                ConnectionStringSetting = "CosmosDBConnection",
                SqlQuery = "SELECT TOP 10 * FROM c WHERE c.id = '{device_id}' order by c.timestamp DESC")]
                IEnumerable<EventItem> events,
                ILogger log)

        {

            float avgPh = 0;

            log.LogInformation("http request for avg ph of device id");
            foreach (EventItem item in events)
            {
                avgPh += item.ph;
            }
            return new OkObjectResult(avgPh / 10);
        }


        [FunctionName("get_pressure_by_device_id")]
        public static IActionResult getPressureByUserId(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "pressure/{device_id}")] HttpRequest request, //todo: eyal - we changed the route
            [CosmosDB(
                        databaseName: "waterly_db",
                        collectionName: "water_table",
                        ConnectionStringSetting = "CosmosDBConnection",
                        SqlQuery = "SELECT TOP 10 * FROM c WHERE c.id = '{device_id}' order by c.timestamp DESC")]
                        IEnumerable<EventItem> events,
                ILogger log)

        {

            float avgPressure = 0;

            log.LogInformation("http request for avg pressure of device id");
            foreach (EventItem item in events)
            {
                avgPressure += item.pressure;
            }
            return new OkObjectResult(avgPressure / 10);
        }



        [FunctionName("create_device")] //todo: need to add device registration
        public static async Task<IActionResult> createDevice(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "devices/{user_id}")] HttpRequest request, ILogger log) //route?
        {
            Container devices_container = Resources.cosmosClient.GetContainer("waterly_db", "waterly_devices");


            DeviceItem deviceJson = JsonConvert.DeserializeObject<DeviceItem>("check"); //check with eyal's client how it's sent

            await devices_container.CreateItemAsync(deviceJson);

            return new OkObjectResult(deviceJson);
        }

        
        [FunctionName("delete_device")] //works
        public static async Task<IActionResult> delete_device(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "devices/{device_id}")] HttpRequest request, string device_id, ILogger log) //
        {

            var option = new FeedOptions { EnableCrossPartitionQuery = true };

            DeviceItem device_to_delete = Resources.docClient.CreateDocumentQuery<DeviceItem>(
                UriFactory.CreateDocumentCollectionUri("waterly_db", "waterly_devices"), option)
                .Where(device_to_delete => device_to_delete.id.Equals(device_id))
                .AsEnumerable()
                .First();

            //swipe boolean status
            device_to_delete.status = false;


            ResourceResponse<Document> response = await Resources.docClient.ReplaceDocumentAsync(
                UriFactory.CreateDocumentUri("waterly_db", "waterly_devices", device_to_delete.id),
                device_to_delete);

            var updated = response.Resource;

            return new OkObjectResult(device_to_delete);
        }
        

        [FunctionName("update_alert")]
        public static async Task<IActionResult> updateAlert(
            [HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "notifications/{notificationId}")] HttpRequest request, string notificationId,
        ILogger log)

        {
            string alert_id = notificationId;

            Container alerts_container = Resources.cosmosClient.GetContainer("waterly_db", "alerts_table");

            var option = new FeedOptions { EnableCrossPartitionQuery = true };

            AlertItem alert_to_update = Resources.docClient.CreateDocumentQuery<AlertItem>(
                UriFactory.CreateDocumentCollectionUri("waterly_db", "alerts_table"), option)
                .Where(alert_to_update => alert_to_update.id.Equals(alert_id))
                .AsEnumerable()
                .First();

            //swipe boolean status
           if (alert_to_update.status == true)
            {
                alert_to_update.status = false;
            } else
            {
                alert_to_update.status = true;
            }
            
            ResourceResponse<Document> response = await Resources.docClient.ReplaceDocumentAsync(
                UriFactory.CreateDocumentUri("waterly_db", "alerts_table", alert_to_update.id),
                alert_to_update);

            var updated = response.Resource;

            return new OkObjectResult(alert_to_update);
        }

        //todo: add function update alert feedback

        // Function is called every month on the 10th at 9 AM.
        [FunctionName("create_bills_for_all_users")]
        public static async Task createBill(
            [TimerTrigger("0 * * * * *")]TimerInfo myTimer,
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
                await BillGenerator.generateNewBill(keyValuePair.Key, keyValuePair.Value, DateTime.Today, avgConsumption);
            }
        }

        /*
        [FunctionName("edit_device_name")]
        public static async Task<IActionResult> EditDeviceName(
    [HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "devices/{id}")] HttpRequest request, string id,
ILogger log)

        {
            string device_id = id;

            Container devices_container = Resources.cosmosClient.GetContainer("waterly_db", "waterly_devices");

            var option = new FeedOptions { EnableCrossPartitionQuery = true };

            DeviceItem device_to_update = Resources.docClient.CreateDocumentQuery<DeviceItem>(
                UriFactory.CreateDocumentCollectionUri("waterly_db", "waterly_devices"), option)
                .Where(device_to_update => device_to_update.id.Equals(device_id))
                .AsEnumerable()
                .First();

            //device_to_update.name = request;


            ResourceResponse<Document> response = await Resources.docClient.ReplaceDocumentAsync(
                UriFactory.CreateDocumentUri("waterly_db", "waterly_devices", device_to_update.id),
                device_to_update);

            var updated = response.Resource;

            return new OkObjectResult(device_to_update);
        }
        */
    }
}






/*    
    FETCH_DEVICE,
    */
