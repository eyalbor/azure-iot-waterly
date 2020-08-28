using System;
using System.Linq;
using System.Net;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;

namespace Waterly_iot_functions
{
    class Detector
    {

        public static Container events_container = InsertEvent.cosmosClient.GetContainer("waterly_db", "water_table");
        public static Container alert_container = InsertEvent.cosmosClient.GetContainer("waterly_db", "alerts_table");
        public static String LEAKAGE = "Leakage";
        public static String ABNORMAL_PH_LEVEL = "Abnormal PH level";
        public static String ABNORMAL_PRESSURE_LEVEL = "Abnormal pressure level";
        private static ILogger logger;


        public static void detectionPipeline(EventItem eventItem, string userId)
        {
            detectPHLevel(eventItem, userId);
            detectPressureLevel(eventItem, userId);
            detectLeakage(eventItem, userId);
        }

        public static async void detectPHLevel(EventItem eventItem, string userId)
        {
            int numOfSamples = 5;
            var sqlQueryText = $"SELECT TOP {numOfSamples} * FROM c WHERE c.device_id = {eventItem.device_id}";
            float avgPH = 0;

            logger.LogInformation(sqlQueryText);
            QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
            FeedIterator<EventItem> queryResultSetIterator = events_container.GetItemQueryIterator<EventItem>(queryDefinition);
            FeedResponse<EventItem> currentResultSet;

            while (queryResultSetIterator.HasMoreResults)
            {
                currentResultSet = await queryResultSetIterator.ReadNextAsync();
                foreach (EventItem item in currentResultSet)
                {
                    avgPH += item.ph;
                }
            }

            avgPH = avgPH / numOfSamples;
            if (avgPH != 0)
            {
                if (avgPH > 5 || avgPH < 3)
                {
                    //There is abnormal PH level
                    logger.LogInformation(ABNORMAL_PH_LEVEL + $"PH: {avgPH}");
                    supressDetection(eventItem, ABNORMAL_PH_LEVEL, userId);
                }
            }
        }


        public static async void detectPressureLevel(EventItem eventItem, string userId)
        {
            int numOfSamples = 5;
            var sqlQueryText = $"SELECT TOP {numOfSamples} * FROM c WHERE c.device_id = {eventItem.device_id}";
            float avgPressue = 0;

            logger.LogInformation(sqlQueryText);
            QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
            FeedIterator<EventItem> queryResultSetIterator = events_container.GetItemQueryIterator<EventItem>(queryDefinition);
            FeedResponse<EventItem> currentResultSet;

            while (queryResultSetIterator.HasMoreResults)
            {
                currentResultSet = await queryResultSetIterator.ReadNextAsync();
                foreach (EventItem item in currentResultSet)
                {
                    avgPressue += item.pressure;
                }
            }

            avgPressue = avgPressue / numOfSamples;

            if (avgPressue != 0)
            {
                if (avgPressue > 5 || avgPressue < 3)
                {
                    //There is a leak abnormal pressure level
                    logger.LogInformation(ABNORMAL_PRESSURE_LEVEL + $"Pressure: {avgPressue}");
                    supressDetection(eventItem, ABNORMAL_PRESSURE_LEVEL, userId);
                }
            }

        }


        public static async void detectLeakage(EventItem eventItem, string userId)
        {

            double lastDayConsumptionPerHour;
            double lastWeekConsumptionPerHour;
            float waterRead24HourAgo = 0;
            float waterRead7DaysAgo = 0;
            long waterReadTimestamp24HoursAgo = 0;
            long waterReadTimestamp7DaysAgo = 0;

            var sqlQueryText = $"SELECT TOP 1 * FROM c WHERE c.device_id = {eventItem.device_id} AND " +
                $"c.timestamp > {(eventItem.timestamp - TimeSpan.FromDays(1).TotalMilliseconds)} " +
                "order by c.timestamp";

            logger.LogInformation(sqlQueryText);
            QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
            FeedIterator<EventItem> queryResultSetIterator = events_container.GetItemQueryIterator<EventItem>(queryDefinition);
            FeedResponse<EventItem> currentResultSet;

            while (queryResultSetIterator.HasMoreResults)
            {
                currentResultSet = await queryResultSetIterator.ReadNextAsync();
                EventItem eventItem24HoursAgo = currentResultSet.FirstOrDefault<EventItem>();
                waterRead24HourAgo = eventItem24HoursAgo.water_read;
                waterReadTimestamp24HoursAgo = eventItem24HoursAgo.timestamp;
            }

            sqlQueryText = $"SELECT TOP 1 * FROM c WHERE c.device_id = {eventItem.device_id} AND " +
                $"c.timestamp > {(eventItem.timestamp - TimeSpan.FromDays(7).TotalMilliseconds)} " +
                $"order by c.timestamp";

            logger.LogInformation(sqlQueryText);
            queryDefinition = new QueryDefinition(sqlQueryText);
            queryResultSetIterator = events_container.GetItemQueryIterator<EventItem>(queryDefinition);

            while (queryResultSetIterator.HasMoreResults)
            {
                currentResultSet = await queryResultSetIterator.ReadNextAsync();
                EventItem eventItem7DaysAgo = currentResultSet.FirstOrDefault<EventItem>();
                waterRead7DaysAgo = eventItem7DaysAgo.water_read;
                waterReadTimestamp7DaysAgo = eventItem7DaysAgo.timestamp;
            }

            if (waterRead24HourAgo != 0 && waterRead7DaysAgo != 0)
            {
                double hoursDiff = TimeSpan.FromMilliseconds(eventItem.timestamp - waterReadTimestamp24HoursAgo).TotalHours;
                lastDayConsumptionPerHour = (eventItem.water_read - waterReadTimestamp24HoursAgo) / hoursDiff;
                hoursDiff = TimeSpan.FromMilliseconds(eventItem.timestamp - waterReadTimestamp7DaysAgo).TotalHours;
                lastWeekConsumptionPerHour = (eventItem.water_read - waterRead7DaysAgo) / hoursDiff;

                if (lastDayConsumptionPerHour / lastWeekConsumptionPerHour > 1.5)
                {
                    //There is a leak
                    logger.LogInformation(LEAKAGE + $"waterRead24hours: {waterRead24HourAgo}, waterRead7DaysAgo: {waterRead7DaysAgo}");
                    supressDetection(eventItem, LEAKAGE, userId);
                }
            }
        }


        // no more than one alert per week
        public static void supressDetection(EventItem eventItem, string type, string userId)
        {
            var now = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();

            //we don't want to raise alert on old events
            if (eventItem.timestamp < now - TimeSpan.FromDays(7).TotalMilliseconds)
            {
                return;
            }

            var sqlQueryText = $"SELECT TOP 1 * FROM c WHERE c.device_id = {eventItem.device_id} AND " +
                $"c.timestamp >  {(now - TimeSpan.FromDays(7).TotalMilliseconds)} AND " +
                $"c.type = {type} order by c.timestamp";

            logger.LogInformation(sqlQueryText);
            QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
            FeedIterator<AlertItem> queryResultSetIterator = alert_container.GetItemQueryIterator<AlertItem>(queryDefinition);

            if (!queryResultSetIterator.HasMoreResults)
            {
                createAlert(eventItem, type, userId);
            }
        }


        public static async void createAlert(EventItem eventItem, string type, string userId)
        {

            AlertItem alert = new AlertItem
            {
                device_id = eventItem.device_id,
                timestamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds(),
                type = type,
                user_id = userId,
                status = false,
                message = "Contact with technician"
            };

            // Create an item in the container representing the Andersen family. Note we provide the value of the partition key for this item, which is "Andersen"
            ItemResponse<AlertItem> alertResponse = await alert_container.CreateItemAsync<AlertItem>(alert, new PartitionKey(alert.device_id));

            // Note that after creating the item, we can access the body of the item with the Resource property off the ItemResponse.
            Console.WriteLine("Created item in database with id: {0}\n", alertResponse.Resource.id);
        }
    }
}
