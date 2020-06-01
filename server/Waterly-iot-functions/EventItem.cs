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

        public float water_read { get; set; }
        public float timestamp { get; set; }


    }

    public class DeviceItem
    {
        public string device_id { get; set; }

        public string userId { get; set; }

        public string name { get; set; }
        public Address address { get; set; }

        public float last_water_read { get; set; }


    }
}





