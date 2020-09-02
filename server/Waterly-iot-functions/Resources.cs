using System;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Cosmos;


namespace Waterly_iot_functions
{
    public static class Resources
    {
        public static CosmosClient cosmosClient = new CosmosClient("AccountEndpoint=https://waterly-iot.documents.azure.com:443/;AccountKey=cC49BNfE7uQTuEVdSNeJAUZuzTjpzl5j0MLSsb8aHGL6jGh3JmubV2TAbgxW05vYtmMA8LqTitsbRPjUZY8YsA==;");
        public static DocumentClient docClient = new DocumentClient(new Uri("https://waterly-iot.documents.azure.com:443/"), "cC49BNfE7uQTuEVdSNeJAUZuzTjpzl5j0MLSsb8aHGL6jGh3JmubV2TAbgxW05vYtmMA8LqTitsbRPjUZY8YsA==");
        public static Container events_container = cosmosClient.GetContainer("waterly_db", "water_table");
        public static Container devices_container = cosmosClient.GetContainer("waterly_db", "devices_table");
        public static Container bill_container = cosmosClient.GetContainer("waterly_db", "bill_table");
        public static Container monthly_consumption_container = cosmosClient.GetContainer("waterly_db", "consumption_device_month");
        public static Container alert_container = cosmosClient.GetContainer("waterly_db", "alerts_table");
        public static Container users_container = cosmosClient.GetContainer("waterly_db", "users_table");
    }
}
