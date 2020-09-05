using System;
using System.Linq;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.WebJobs;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Waterly_iot_functions
{
    public static class Detector
    {

        public static Container events_container = Resources.cosmosClient.GetContainer("waterly_db", "water_table");
        public static Container alert_container = Resources.cosmosClient.GetContainer("waterly_db", "alerts_table");
        public static String LEAKAGE = "Leakage";
        public static String ABNORMAL_PH_LEVEL = "Abnormal PH level";
        public static String ABNORMAL_PRESSURE_LEVEL = "Abnormal pressure level";
        private static ILogger logger;

        [FunctionName("execute_detection_logic")]
        public static async Task executeDetectionLogic(EventItem eventItem, String userId, ILogger log)
        {
            Detector.logger = log;
            logger.LogInformation("Executing detection pipeline...");
            
            Task ph = detectPHLevel(eventItem, userId);
            Task pressure = detectPressureLevel(eventItem, userId);
            Task leakage = detectLeakage(eventItem, userId);

            await ph;
            await pressure;
            await leakage;
        }

        public static async Task detectPHLevel(EventItem eventItem, string userId)
        {
            float avgPH = 0;
            int numOfSamples = 10;
            var sqlQueryText = $"SELECT TOP {numOfSamples} * FROM c WHERE c.device_id = '{eventItem.device_id}' " +
                    "order by c.timestamp DESC";

            logger.LogInformation("Observing PH level...");
            QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
            FeedIterator<EventItem> queryResultSetIterator = Resources.events_container.GetItemQueryIterator<EventItem>(queryDefinition);
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
                    logger.LogInformation(ABNORMAL_PH_LEVEL + $" PH: {avgPH}");
                    await suppressDetection(eventItem, ABNORMAL_PH_LEVEL, userId);
                }
            }
        }


        public static async Task detectPressureLevel(EventItem eventItem, string userId)
        {
            int numOfSamples = 10;
            var sqlQueryText = $"SELECT TOP {numOfSamples} * FROM c WHERE c.device_id = '{eventItem.device_id}'" +
                    "order by c.timestamp DESC";
            float avgPressure = 0;

            logger.LogInformation("Observing pressure level...");
            QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
            FeedIterator<EventItem> queryResultSetIterator = Resources.events_container.GetItemQueryIterator<EventItem>(queryDefinition);
            FeedResponse<EventItem> currentResultSet;

            while (queryResultSetIterator.HasMoreResults)
            {
                currentResultSet = await queryResultSetIterator.ReadNextAsync();
                foreach (EventItem item in currentResultSet)
                {
                    avgPressure += item.pressure;
                }
            }

            avgPressure = avgPressure / numOfSamples;

            if (avgPressure != 0)
            {
                if (avgPressure > 5 || avgPressure < 3)
                {
                    //There is a leak abnormal pressure level
                    logger.LogInformation(ABNORMAL_PRESSURE_LEVEL + $" Pressure: {avgPressure}");
                    await suppressDetection(eventItem, ABNORMAL_PRESSURE_LEVEL, userId);
                }
            }
        }


        public static async Task detectLeakage(EventItem eventItem, string userId)
        {

            double lastDayConsumptionPerHour;
            double lastWeekConsumptionPerHour;
            long waterRead24HourAgo = 0;
            long waterRead7DaysAgo = 0;
            long waterReadTimestamp24HoursAgo = 0;
            long waterReadTimestamp7DaysAgo = 0;
            
            logger.LogInformation("Observing avg water consumption...");

            var sqlQueryText = $"SELECT TOP 1 * FROM c WHERE c.device_id = '{eventItem.device_id}' AND " +
                $"c.timestamp > {(eventItem.timestamp - TimeSpan.FromDays(1).TotalSeconds)} " +
                "order by c.timestamp";

            QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
            FeedIterator<EventItem> queryResultSetIterator = Resources.events_container.GetItemQueryIterator<EventItem>(queryDefinition);
            FeedResponse<EventItem> currentResultSet;

            while (queryResultSetIterator.HasMoreResults)
            {
                currentResultSet = await queryResultSetIterator.ReadNextAsync();
                EventItem eventItem24HoursAgo = currentResultSet.FirstOrDefault<EventItem>();
                waterRead24HourAgo = eventItem24HoursAgo.water_read;
                waterReadTimestamp24HoursAgo = eventItem24HoursAgo.timestamp;
            }

            sqlQueryText = $"SELECT TOP 1 * FROM c WHERE c.device_id = '{eventItem.device_id}' AND " +
                $"c.timestamp > {(eventItem.timestamp - TimeSpan.FromDays(7).TotalSeconds)} " +
                $"order by c.timestamp";

            queryDefinition = new QueryDefinition(sqlQueryText);
            queryResultSetIterator = Resources.events_container.GetItemQueryIterator<EventItem>(queryDefinition);

            while (queryResultSetIterator.HasMoreResults)
            {
                currentResultSet = await queryResultSetIterator.ReadNextAsync();
                EventItem eventItem7DaysAgo = currentResultSet.FirstOrDefault<EventItem>();
                waterRead7DaysAgo = eventItem7DaysAgo.water_read;
                waterReadTimestamp7DaysAgo = eventItem7DaysAgo.timestamp;
            }

            if (waterRead24HourAgo != 0 && waterRead7DaysAgo != 0)
            {
                double hoursDiff = TimeSpan.FromSeconds(eventItem.timestamp - waterReadTimestamp24HoursAgo).TotalHours;
                lastDayConsumptionPerHour = (eventItem.water_read - waterRead24HourAgo) / hoursDiff;
                hoursDiff = TimeSpan.FromSeconds(eventItem.timestamp - waterReadTimestamp7DaysAgo).TotalHours;
                lastWeekConsumptionPerHour = (eventItem.water_read - waterRead7DaysAgo) / hoursDiff;

                if (lastDayConsumptionPerHour / lastWeekConsumptionPerHour > 1.5)
                {
                    //There is a leak
                    logger.LogInformation(LEAKAGE + $" waterRead24hours: {waterRead24HourAgo}, waterRead7DaysAgo: {waterRead7DaysAgo}");
                    await suppressDetection(eventItem, LEAKAGE, userId);
                }
            }
        }


        // no more than one alert per week
        public static async Task suppressDetection(EventItem eventItem, string type, string userId, Dictionary<string,int> evidence)
        {
            var now = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();

            //we don't want to raise alert on old events
            if (eventItem.timestamp < now - TimeSpan.FromDays(7).TotalSeconds)
            {
                return;
            }

            var sqlQueryText = $"SELECT TOP 1 * FROM c WHERE c.device_id = '{eventItem.device_id}' AND " +
                $"c.created_at >  {(now - TimeSpan.FromDays(7).TotalSeconds)} AND " +
                $"c.type = '{type}' order by c.created_at";

            logger.LogInformation("Checking older alerts...");
            
            QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
            FeedIterator<AlertItem> queryResultSetIterator = Resources.alert_container.GetItemQueryIterator<AlertItem>(queryDefinition);

            if (queryResultSetIterator.HasMoreResults)
            {
                FeedResponse<AlertItem> currentResultSet = await queryResultSetIterator.ReadNextAsync();
                AlertItem alert = currentResultSet.FirstOrDefault<AlertItem>();
                if (!Object.Equals(null, alert)){
                    logger.LogInformation("Detection was suppressed");
                    return;
                }                
            }
            await createAlert(eventItem, type, userId, evidence);
        }


        public static async Task createAlert(EventItem eventItem, string type, string userId)
        {
            logger.LogInformation("Creating alert...");

            AlertItem alert = new AlertItem
            {
                id = Guid.NewGuid().ToString(),
                device_id = eventItem.device_id,
                created_at = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds(),
                type = type,
                user_id = userId,
                status = false,
                message = "Contact with technician"
            };

            // Create an item in the container representing alert.
            ItemResponse<AlertItem> alertResponse = await Resources.alert_container.CreateItemAsync<AlertItem>(alert);

            // Note that after creating the item, we can access the body of the item with the Resource property off the ItemResponse.
            Console.WriteLine("Created alert in database with id: {0}\n", alertResponse.Resource.id);
        }
    
    }
}
