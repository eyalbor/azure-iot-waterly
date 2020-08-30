using System;
using System.Collections.Generic;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Linq;



namespace Waterly_iot_functions
{
    class BillGenerator
    {

        public static Container events_container = InsertEvent.cosmosClient.GetContainer("waterly_db", "water_table");
        public static Container devices_container = InsertEvent.cosmosClient.GetContainer("waterly_db", "devices_table");
        public static Container bill_container = InsertEvent.cosmosClient.GetContainer("waterly_db", "bill_table");
        public static int FIXED_EXPENSES = 20;
        private static ILogger logger;


        public static async Task<long> calculateUserConsumption(UserItem user, ILogger log, DateTime today)
        {
            logger = log;
            List<DeviceItem> userDevices = await getUserDevices(user);
            long totalConsumption = 0;
            foreach (DeviceItem device in userDevices)
            {
                totalConsumption += await calculateDeviceConsumption(device.device_id, today);
            }
            return totalConsumption;

        }

        public static async Task<List<DeviceItem>> getUserDevices(UserItem user)
        {
            // query devices

            var sqlQueryText = $"SELECT * FROM c WHERE c.userId = {user.id}";

            logger.LogInformation(sqlQueryText);
            QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
            FeedIterator<DeviceItem> queryResultSetIterator = devices_container.GetItemQueryIterator<DeviceItem>(queryDefinition);
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

        public static async Task<long> calculateDeviceConsumption(string device_id, DateTime today)
        {
            long deviceConsumption = 0;
            long waterReadfirstEventInTheMonth = 0;
            long waterReadfirstEventInTheLastMonth = 0;

            DateTime startOfMonth = new DateTime(today.Year, today.Month, 1);
            var sqlQueryText = $"SELECT TOP 1 * FROM c WHERE c.device_id = {device_id} AND " +
            $"c.timestamp > {((DateTimeOffset)startOfMonth).ToUnixTimeMilliseconds()} " +
            "order by c.timestamp DESC";

            // query first water read of this month

            logger.LogInformation(sqlQueryText);
            QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
            FeedIterator<EventItem> queryResultSetIterator = events_container.GetItemQueryIterator<EventItem>(queryDefinition);
            FeedResponse<EventItem> currentResultSet;

            while (queryResultSetIterator.HasMoreResults)
            {
                currentResultSet = await queryResultSetIterator.ReadNextAsync();
                EventItem firstEventItemInTheMonth = currentResultSet.FirstOrDefault<EventItem>();
                waterReadfirstEventInTheMonth = firstEventItemInTheMonth.water_read;
            }

            DateTime startOfLastMonth = startOfMonth.AddMonths(-1);
            sqlQueryText = $"SELECT TOP 1 * FROM c WHERE c.device_id = {device_id} AND " +
            $"c.timestamp > {((DateTimeOffset)startOfMonth).ToUnixTimeMilliseconds()} " +
            "order by c.timestamp DESC";

            // query first water read of last month

            logger.LogInformation(sqlQueryText);
            queryDefinition = new QueryDefinition(sqlQueryText);
            queryResultSetIterator = events_container.GetItemQueryIterator<EventItem>(queryDefinition);
         
            while (queryResultSetIterator.HasMoreResults)
            {
                currentResultSet = await queryResultSetIterator.ReadNextAsync();
                EventItem firstEventItemInTheLastMonth = currentResultSet.FirstOrDefault<EventItem>();
                waterReadfirstEventInTheLastMonth = firstEventItemInTheLastMonth.water_read;
            }

            if (waterReadfirstEventInTheLastMonth < waterReadfirstEventInTheMonth)
            {
                deviceConsumption = waterReadfirstEventInTheMonth - waterReadfirstEventInTheLastMonth;
            }

            return deviceConsumption;

        }

        public static async void generateNewBill(UserItem user, long consumption, DateTime today, double avarage)
        {

            DateTime billPeriod = today.AddMonths(-1);

            BillItem bill = new BillItem
            {
                user_id = user.id,
                avg = avarage,
                status = false,
                month = billPeriod.Month,
                year = billPeriod.Year,
                total_flow = consumption,
                fixed_expenses = 20,
                water_expenses = 10 * (float)Math.Floor((double)consumption / 1000000)
            };

            // Create an item in the container representing the bill.
            ItemResponse<BillItem> billResponse = await bill_container.CreateItemAsync<BillItem>(bill);

            // Note that after creating the item, we can access the body of the item with the Resource property off the ItemResponse.
            Console.WriteLine("Created item in database with id: {0}\n", billResponse.Resource.bill_id);
        }


    }
}
