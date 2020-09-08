using System;
using System.Collections.Generic;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Linq;
using Bugsnag.Payload;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.Net.Http;



namespace Waterly_iot_functions
{
    public static class BillGenerator
    {
        public static int FIXED_EXPENSES = 20;
        private static ILogger logger;

        public static async Task<long> calculateUserConsumption(UserItem user, ILogger log, DateTime today)
        {
            logger = log;   
            List<DeviceItem> userDevices = await getUserDevices(user);
            long totalConsumption = 0;
            foreach (DeviceItem device in userDevices)
            {
                totalConsumption += await calculateDeviceConsumption(device.id, today, user.id);
            }
            return totalConsumption;

        }

        public static async Task<List<DeviceItem>> getUserDevices(UserItem user)
        {
            // query devices

            var sqlQueryText = $"SELECT * FROM c WHERE c.userId = '{user.id}'";

            QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
            FeedIterator<DeviceItem> queryResultSetIterator = Resources.devices_container.GetItemQueryIterator<DeviceItem>(queryDefinition);
            FeedResponse<DeviceItem> currentResultSet;

            List<DeviceItem> deviceList = new List<DeviceItem>();

            while (queryResultSetIterator.HasMoreResults)
            {
                currentResultSet = await queryResultSetIterator.ReadNextAsync();
                foreach (DeviceItem item in currentResultSet)
                {
                    deviceList.Add(item);
                }
            }

            return deviceList;
        }

        public static async Task<long> calculateDeviceConsumption(string device_id, DateTime today, string user_id)
        {
            long deviceConsumption = 0;
            long waterReadfirstEventInTheMonth = 0;
            long waterReadfirstEventInTheLastMonth = 0;

            logger.LogInformation($"Calculating device consumption: {device_id}");

            DateTime startOfMonth = new DateTime(today.Year, today.Month, 1);
            var sqlQueryText = $"SELECT TOP 1 * FROM c WHERE c.device_id = '{device_id}' AND " +
            $"c.timestamp > {((DateTimeOffset)startOfMonth).ToUnixTimeSeconds()} " +
            "order by c.timestamp";

            // query first water read of this month

            QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
            FeedIterator<EventItem> queryResultSetIterator = Resources.events_container.GetItemQueryIterator<EventItem>(queryDefinition);
            FeedResponse<EventItem> currentResultSet;

            while (queryResultSetIterator.HasMoreResults)
            {
                currentResultSet = await queryResultSetIterator.ReadNextAsync();
                EventItem firstEventItemInTheMonth = currentResultSet.FirstOrDefault<EventItem>();
                waterReadfirstEventInTheMonth = firstEventItemInTheMonth.water_read;
            }

            DateTime startOfLastMonth = startOfMonth.AddMonths(-1);
            sqlQueryText = $"SELECT TOP 1 * FROM c WHERE c.device_id = '{device_id}' AND " +
            $"c.timestamp > {((DateTimeOffset)startOfLastMonth).ToUnixTimeSeconds()} " +
            "order by c.timestamp";

            // query first water read of last month

            queryDefinition = new QueryDefinition(sqlQueryText);
            queryResultSetIterator = Resources.events_container.GetItemQueryIterator<EventItem>(queryDefinition);
         
            while (queryResultSetIterator.HasMoreResults)
            {
                currentResultSet = await queryResultSetIterator.ReadNextAsync();
                EventItem firstEventItemInTheLastMonth = currentResultSet.FirstOrDefault<EventItem>();
                waterReadfirstEventInTheLastMonth = firstEventItemInTheLastMonth.water_read;
            }

            if (waterReadfirstEventInTheLastMonth < waterReadfirstEventInTheMonth)
            {
                deviceConsumption = waterReadfirstEventInTheMonth - waterReadfirstEventInTheLastMonth;
                await createMonthlyDeviceConsumptionItem(device_id, today.AddMonths(-1), deviceConsumption,
                     waterReadfirstEventInTheMonth, waterReadfirstEventInTheLastMonth, user_id);
            }
            return deviceConsumption;

        }

        public static async Task createMonthlyDeviceConsumptionItem(string device_id, DateTime period, long consuption,
            long waterReadfirstEventInTheMonth, long waterReadfirstEventInTheLastMonth, string user_id)
        {
            MonthlyDeviceConsumptionItem monthlyDeviceConsumptionItem = new MonthlyDeviceConsumptionItem()
            {
                id = $"{device_id}-{period.Month}-{period.Year}",
                consumption_sum = consuption,
                month = period.Month,
                year = period.Year,
                device_id = device_id,
                user_id = user_id,
                first_water_read = waterReadfirstEventInTheLastMonth,
                last_water_read = waterReadfirstEventInTheMonth
            };

            // Create an item in the container representing the bill.
            ItemResponse<MonthlyDeviceConsumptionItem> monthlyConsumptionResponse = 
                await Resources.monthly_consumption_container.UpsertItemAsync<MonthlyDeviceConsumptionItem>(monthlyDeviceConsumptionItem);

            // Note that after creating the item, we can access the body of the item with the Resource property off the ItemResponse.
            Console.WriteLine("Created item in database with id: {0}\n", monthlyConsumptionResponse.Resource.id);

        }

        public static async Task generateNewBill(UserItem user, long consumption, DateTime today, double avarage)
        {

            DateTime billPeriod = today.AddMonths(-1);

            BillItem bill = new BillItem
            {
                id = $"{user.id}-{billPeriod.Month}-{billPeriod.Year}",
                user_id = user.id,
                avg = 10 * (float)Math.Floor(avarage / 1000000),
                status = false,
                month = billPeriod.Month,
                year = billPeriod.Year,
                total_flow = consumption,
                fixed_expenses = 20,
                water_expenses = 10 * (float)Math.Floor((double)consumption / 1000000)
            };

            // Create an item in the container representing the bill.
            ItemResponse<BillItem> billResponse = await Resources.bill_container.UpsertItemAsync<BillItem>(bill);

            // Note that after creating the item, we can access the body of the item with the Resource property off the ItemResponse.
            Console.WriteLine("Created item in database with id: {0}\n", billResponse.Resource.id);
        }


        [FunctionName("pay_bill")]
        public static async Task<IActionResult> payBill(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "bill")] HttpRequest req,
            ILogger log)
        {
            try
            {
                log.LogInformation("pay the bill for user ");

                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var data = JsonConvert.DeserializeObject<WaterlyBillReq>(requestBody);

                //send the email

                // requires using System.Net.Http;
                var client = new HttpClient();
                log.LogInformation(requestBody);

                HttpResponseMessage result = await client.PostAsync(
                    // requires using System.Configuration;
                    "https://prod-26.eastus.logic.azure.com:443/workflows/06a66aa325a84a29b64f788ff1537d50/triggers/manual/paths/invoke?api-version=2016-10-01&sp=%2Ftriggers%2Fmanual%2Frun&sv=1.0&sig=-f_3sTNheCjl7dSq3dZzCuqkYChEXDcweiK92DVv_KU",
                    new StringContent(requestBody, System.Text.Encoding.UTF8, "application/json"));

                var statusCode = result.StatusCode.ToString();

                if (statusCode != "200") {
                    return new BadRequestObjectResult(statusCode);
                }
                else {
                    return new OkObjectResult(statusCode);
                }
            }
            catch (System.Exception e)
            {
                return new BadRequestObjectResult(e.Message);
            }

        }

    }
}
