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
        public static String LEAKAGE = "Possible leakage";
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
            int numOfSamples = Resources.numOfSamples;
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
                if (avgPH > 8 || avgPH < 6)
                {
                    //There is abnormal PH level
                    string evidence = $"PH: {avgPH}";
                    logger.LogInformation(ABNORMAL_PH_LEVEL + " , " + evidence);
                    await suppressDetection(eventItem, ABNORMAL_PH_LEVEL, userId, evidence);
                }
            }
        }


        public static async Task detectPressureLevel(EventItem eventItem, string userId)
        {
            int numOfSamples = Resources.numOfSamples;
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
                    //There is abnormal pressure level
                    string evidence = $"Pressure: {avgPressure}";
                    logger.LogInformation(ABNORMAL_PRESSURE_LEVEL + " , " + evidence);
                    await suppressDetection(eventItem, ABNORMAL_PRESSURE_LEVEL, userId, evidence);
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
                    string evidence = $"waterRead24hours: {waterRead24HourAgo}, waterRead7DaysAgo: {waterRead7DaysAgo}";
                    logger.LogInformation(LEAKAGE + " , " + evidence);
                    await suppressDetection(eventItem, LEAKAGE, userId, evidence);
                }
            }
        }


        // no more than one alert per week
        public static async Task suppressDetection(EventItem eventItem, string type, string userId, string evidence)
        {
            var now = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();

            //we don't want to raise alert on old events
            if (eventItem.timestamp < now - TimeSpan.FromDays(7).TotalSeconds)
            {
                return;
            }

            var sqlQueryText = $"SELECT TOP 1 * FROM c WHERE c.device_id = '{eventItem.device_id}' AND " +
                $"c.created_at >  {(now - TimeSpan.FromDays(1).TotalSeconds)} AND " +
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


        public static async Task createAlert(EventItem eventItem, string type, string userId, string evidence)
        {
            logger.LogInformation("Creating alert...");

            var sqlQueryText = $"SELECT * FROM c WHERE c.id = '{eventItem.device_id}'";
            string device_name = null;

            QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
            FeedIterator<DeviceItem> queryResultSetIterator = Resources.devices_container.GetItemQueryIterator<DeviceItem>(queryDefinition);
            FeedResponse<DeviceItem> currentResultSet;
            while (queryResultSetIterator.HasMoreResults)
            {
                currentResultSet = await queryResultSetIterator.ReadNextAsync();
                DeviceItem device = currentResultSet.FirstOrDefault<DeviceItem>();
                device_name = device.name;
            }

            AlertItem alert = new AlertItem
            {
                id = Guid.NewGuid().ToString(),
                device_id = eventItem.device_id,
                created_at = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds(),
                type = type,
                user_id = userId,
                status = true,
                evidence = evidence,
                device_name = device_name,
                message = "Please contact with your local service center"
            };

            // Create an item in the container representing alert.
            ItemResponse<AlertItem> alertResponse = await Resources.alert_container.CreateItemAsync<AlertItem>(alert);

            // Note that after creating the item, we can access the body of the item with the Resource property off the ItemResponse.
            Console.WriteLine("Created alert in database with id: {0}\n", alertResponse.Resource.id);

            EmailSender.sendMailNewAlert(alert, userId);

        }
    
    }
}
