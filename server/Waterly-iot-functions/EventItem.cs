using System;
using System.Collections.Generic;
using System.Text;

namespace Waterly_iot_functions
{
    public class Address
    {

        public string city { get; set; }
        public string street { get; set; }

        public int building_num { get; set; }
        public int apt_num { get; set; }
    }

    public class EventItem
    {

        public string device_id { get; set; }

        public long water_read { get; set; }
        public long timestamp { get; set; } 

        public string id { get; set; }

        public float ph { get; set; }
        public float pressure { get; set; }
         
    }

    public class DeviceItem
    {
        public string device_id { get; set; }

        public string userId { get; set; }

        public string name { get; set; }

        public Address address { get; set; }

        public long last_water_read { get; set; }

        public long last_update_timestamp { get; set; }


        public string id { get; set; }

        public float lat { get; set; }

        public float lng { get; set; }


    }

    public class UserItem
    {

        public string id { get; set; }
        public string full_name { get; set; }
        public long created_at { get; set; }
        public string email { get; set; }
    }

    public class BillItem
    {
        public string id { get; set; }
        public string user_id { get; set; }
        public int month { get; set; }
        public int year { get; set; }
        public float water_expenses { get; set; }

        public float fixed_expenses { get; set; }
        public long total_flow { get; set; }

        public bool status { get; set; }

        public double avg { get; set; }
    }

    public class AlertItem
    {
        public string id { get; set; }

        public string device_id { get; set; }

        public string user_id { get; set; }

        public string type { get; set; }

        public long created_at { get; set; }

        public string message { get; set; }

        public bool status { get; set; }

        public bool feedback { get; set; }
    }

    public class MonthlyDeviceConsumptionItem
    {
        public string id { get; set; }
        public string device_id { get; set; }
        public int month { get; set; }
        public int year { get; set; }
        public long consumption_sum { get; set; }
        public long last_water_read { get; set; }
    }
}




