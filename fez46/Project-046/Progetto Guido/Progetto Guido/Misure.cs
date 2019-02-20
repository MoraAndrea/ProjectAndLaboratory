using System;
using Microsoft.SPOT;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using Microsoft.SPOT.Net;
using System.Globalization;

namespace Progetto_Guido
{
    public class Misure
    {        
        public string iso_timestamp { get; set; }
        public string sensor { get; set; }
        public int sensor_id { get; set; }
        public float value { get; set;}
        public string status { get; set; }

        public Misure()
        {
            iso_timestamp = DateTime.Now.ToString("yyyy-MM-dd'T'HH:mm:ss'+02:00'");    //yyyy-MM-dd'T'HH:mm:ss.fff'Z'
        }  

        public void setSensorID(int ID){
            sensor_id=ID;
        }
    }
}
