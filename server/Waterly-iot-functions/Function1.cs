using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Cosmos;
using System.IO;


namespace Waterly_iot_functions
{
    public static class Function1
    {
        [FunctionName("get_devices_by_user_id")] 
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



        [FunctionName("get_comsumption_of_user")] 
        public static async Task<IActionResult> getUserConsumption(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "consumption_per_month/userId={userId}&year={year}")] HttpRequest request,
        [CosmosDB(
                databaseName: "waterly_db",
                collectionName: "waterly_devices",
                ConnectionStringSetting = "CosmosDBConnection",
                SqlQuery = "SELECT * FROM c WHERE c.userId = {userId}")]
                IEnumerable<DeviceItem> devices,
                string year,
                ILogger log)

        {
            Container BillsContainer = Resources.bill_container;
            Container ConsumptionContainer = Resources.monthly_consumption_container;
            int cur_year = Int32.Parse(year);

            //for all year, dict for month number and dict userConsumptionPerMonthDict
            List<Dictionary<string, long>> consumptions_months_list = new List<Dictionary<string, long>>();

            for (int month_num = 1; month_num < 13; month_num++) 
            {
                //for each month, dict for device id and consumption sum
                Dictionary<string, long> userConsumptionPerMonthDict = new Dictionary<string, long>();

                userConsumptionPerMonthDict.Add("Month", month_num);

                //Add avg per month
                var sqlQueryText = $"SELECT TOP 1 * FROM c WHERE c.month = {month_num} AND c.year = {cur_year}";
                QueryDefinition bills_query = new QueryDefinition(sqlQueryText);
                FeedIterator<BillItem> bill_iterator = BillsContainer.GetItemQueryIterator<BillItem>(bills_query);


                Microsoft.Azure.Cosmos.FeedResponse<BillItem> currentResultSet;

                long avg = 0;
                
                while (bill_iterator.HasMoreResults)
                {
                    currentResultSet = await bill_iterator.ReadNextAsync();
                    if (currentResultSet.Count == 0)
                    {
                        break;
                    } else
                    {
                        BillItem first_bill = currentResultSet.First();
                        avg = (long)first_bill.avg;
                        avg /= 10;
                        break;
                    }

                }

                userConsumptionPerMonthDict.Add("Average", avg);
                
                foreach (DeviceItem device_item in devices)
                {
                    //calculate sum for month_num for device_item
                    string device_id = device_item.id;
                    sqlQueryText = $"SELECT * FROM c WHERE c.month = {month_num} AND c.year = {cur_year} AND c.device_id = '{device_id}'";
                    QueryDefinition consumption_query = new QueryDefinition(sqlQueryText);
                    FeedIterator<MonthlyDeviceConsumptionItem> consumption_iterator = ConsumptionContainer.GetItemQueryIterator<MonthlyDeviceConsumptionItem>(consumption_query);
                    long consumption_per_device_month = 0;
                    Microsoft.Azure.Cosmos.FeedResponse<MonthlyDeviceConsumptionItem> ConsumptionCurrentResultSet;


                    while (consumption_iterator.HasMoreResults)
                    {
                        ConsumptionCurrentResultSet = await consumption_iterator.ReadNextAsync();
                        if (ConsumptionCurrentResultSet.Count == 0)
                        {
                            break;
                        } else
                        {
                            MonthlyDeviceConsumptionItem first_sum_item = ConsumptionCurrentResultSet.First();
                            consumption_per_device_month = first_sum_item.consumption_sum / 1000000;
                            break;
                        }
                    }


                    //add to dict
                    userConsumptionPerMonthDict.Add(device_item.name, consumption_per_device_month);
                }

                consumptions_months_list.Add(userConsumptionPerMonthDict); 
            }

            return new OkObjectResult(consumptions_months_list);
        }


        [FunctionName("get_events_by_device_id")] 
        public static IActionResult getEventsByDeviceId(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "events/device_id={deviceId}")] HttpRequest request,
            [CosmosDB(
                databaseName: "waterly_db",
                collectionName: "water_table",
                SqlQuery = "SELECT top 20 * FROM c WHERE c.device_id = {deviceId} order by c.timestamp desc", //todo: remember to change here
                ConnectionStringSetting = "CosmosDBConnection")]
                IEnumerable<EventItem> events,
                ILogger log)


        {
            List<EventItem> eventsList = new List<EventItem>();
            log.LogInformation("http request for events of device id");
            foreach (EventItem event_item in events)
            {
                eventsList.Add(event_item);
            }
            return new OkObjectResult(eventsList);
        }


        [FunctionName("update_bill_paid")] 
        public static async Task<IActionResult> updateBillPaid(
            [HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "bills/{bill_id}")] HttpRequest request, string bill_id,
            ILogger log)
        {

            string req_body = await new StreamReader(request.Body).ReadToEndAsync();
            BillItem current_bill_data = JsonConvert.DeserializeObject<BillItem>(req_body);
            string user_id = current_bill_data.user_id;

            Container bills_container = Resources.bill_container;

            var option = new FeedOptions { EnableCrossPartitionQuery = true };

            BillItem bill_to_pay = Resources.docClient.CreateDocumentQuery<BillItem>(
                UriFactory.CreateDocumentCollectionUri("waterly_db", "bills_table"), option)
                .Where(bill_to_pay => bill_to_pay.id.Equals(bill_id))
                .AsEnumerable()
                .First();

            bill_to_pay.status = true; //true = paid

            ResourceResponse<Document> response = await Resources.docClient.ReplaceDocumentAsync(
                UriFactory.CreateDocumentUri("waterly_db", "bills_table", bill_to_pay.id),
                bill_to_pay);

            var updated = response.Resource;

            //send mail
            EmailSender.sendMailBillPaymentConfirmation(bill_to_pay, user_id);

            return new OkObjectResult(bill_to_pay);
         }


        [FunctionName("get_bills_by_user_id")] 
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

        [FunctionName("get_alerts_by_user_id")] 
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


        [FunctionName("get_water_quality_by_device_id")]
        public static IActionResult getWaterQualityByDeviceId(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "quality/device_id={device_id}")] HttpRequest request, 
            [CosmosDB(
                databaseName: "waterly_db",
                collectionName: "water_table",
                ConnectionStringSetting = "CosmosDBConnection",
                SqlQuery = "SELECT TOP 10 * FROM c WHERE c.device_id = {device_id} order by c.timestamp DESC")]
                IEnumerable<EventItem> events,
                ILogger log)

        {
            int numOfSamples = Resources.numOfSamples;
            float avgPh = 0;
            float avgPressure = 0;
            float avgSalinity = 0;

            log.LogInformation("http request for avg ph, pressure and salinity of device id");
            foreach (EventItem item in events)
            {
                avgPh += item.ph;
                avgPressure += item.pressure;
                avgSalinity += item.salinity;
            }
            Dictionary<string, float> qualityDict = new Dictionary<string, float> {
                {"ph", avgPh/numOfSamples}, {"pressure", avgPressure/numOfSamples}, {"salinity", avgSalinity/numOfSamples} };
            return new OkObjectResult(JsonConvert.SerializeObject(qualityDict));
        }

        
        [FunctionName("delete_device")] 
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

            string req_body = await new StreamReader(request.Body).ReadToEndAsync();
            AlertItem current_alert_data = JsonConvert.DeserializeObject<AlertItem>(req_body);
            Container alerts_container = Resources.alert_container;

            var option = new FeedOptions { EnableCrossPartitionQuery = true };

            AlertItem alert_to_update = Resources.docClient.CreateDocumentQuery<AlertItem>(
                UriFactory.CreateDocumentCollectionUri("waterly_db", "alerts_table"), option)
                .Where(alert_to_update => alert_to_update.id.Equals(alert_id))
                .AsEnumerable()
                .First();

            bool status = current_alert_data.status; 
            string feedback = current_alert_data.feedback; 

            if (alert_to_update.status != status)
            {
                alert_to_update.status = status;
            }

            if (alert_to_update.feedback != feedback)
            {
                alert_to_update.feedback = feedback;
            }


            ResourceResponse<Document> response = await Resources.docClient.ReplaceDocumentAsync(
                UriFactory.CreateDocumentUri("waterly_db", "alerts_table", alert_to_update.id),
                alert_to_update);

            var updated = response.Resource;

            return new OkObjectResult(alert_to_update);
        }

        // Function is called every month on the 10th at 9 AM.
        [FunctionName("create_bills_for_all_users")]
        public static async Task createBill(
            [TimerTrigger("0 0 9 10 * *")]TimerInfo myTimer,
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
                log.LogInformation($"Calculating user consumption: {user.full_name}");
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


    }
}
