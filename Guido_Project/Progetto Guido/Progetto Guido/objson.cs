using System;
using Microsoft.SPOT;
using System.Collections;

namespace Progetto_Guido
{
    public class objson
    {
        private string device_id { get; set; }
        //private int group { get; set; }
        //private int iso_timestamp { get; set; }
        //private int version;
        private ArrayList measurements {get; set;}
        public Hashtable hastTable { get; set; }

        public objson(string id) 
        {
            hastTable = new Hashtable();
            hastTable.Add("device_id", id);
            hastTable.Add("group", "FEZ_46");
            hastTable.Add("iso_timestamp", DateTime.Now.ToString("yyyy-MM-dd'T'HH:mm:ss'+02:00'"));   //con il tostring dico il formato che voglio
            hastTable.Add("version", 2);
            measurements = new ArrayList();
            hastTable.Add("measurements", measurements);
            device_id = id;   
        }

        public void addMisure(Misure m)
        {
            measurements.Add(m); 
        }
    }
}
