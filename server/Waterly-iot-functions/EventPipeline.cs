using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

namespace Waterly_iot_functions
{
    class EventPipeline
    {
        //inserts events from simulation to tables (by trigger)
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

            // Correcting out of orderness
            if (deviceItem.last_update_timestamp < last_update_timestamp)
            {
                deviceItem.last_water_read = last_water_read;
                deviceItem.last_update_timestamp = last_update_timestamp;
            }


            ResourceResponse<Document> response = await Resources.docClient.ReplaceDocumentAsync(
                UriFactory.CreateDocumentUri("waterly_db", "waterly_devices", deviceItem.id),
                deviceItem);

            var updated = response.Resource;

            //execute detector
            await Detector.executeDetectionLogic(dataJson, deviceItem.userId, log);

        }

    }
}
