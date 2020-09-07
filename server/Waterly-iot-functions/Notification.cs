using System;
using System.Collections.Generic;
using System.Text;

namespace Waterly_iot_functions
{
    public class Notification
    {
        public string id { get; set; }
        public string user_id { get; set; }
        public int timestamp { get; set; }
        public string type { get; set; }
        public string message { get; set; }
        public string device_id { get; set; }
        public bool status { get; set; }
        public bool feedback { get; set; }
    }
}
