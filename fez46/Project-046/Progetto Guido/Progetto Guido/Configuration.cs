using System;
using Microsoft.SPOT;
using System.Collections;

namespace Progetto_Guido
{
    public class Configuration
    {
        private string device_id { get; set; }
        private string name { get; set; }
        private string group { get; set; }
        private string type { get; set; }
        private ArrayList sensorList {get; set;}
        private Hashtable sensors { get; set; }
        private string description { get; set; }
        private string location { get; set; }
        private string latitude { get; set; }
        private string longitude { get; set; }
        private bool intern;
        
        public Configuration(string id, string name, string group, string type, ArrayList sensors, string description, string location, string latitude, string longitude, bool intern) 
        {
            device_id = id;
            this.name = name;
            this.group = group;
            this.type = type;
            this.description = description;
            this.location = location;
            this.latitude = latitude;
            this.longitude = longitude;
            this.intern = intern;

            this.sensors = new Hashtable();
            this.sensorList = new ArrayList();
            this.sensors.Add("sensor",sensorList);
        }

        public Configuration()
        {
            
        }


       
    }
}
