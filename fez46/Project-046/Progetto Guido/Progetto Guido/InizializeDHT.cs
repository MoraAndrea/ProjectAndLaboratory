using System;
using Microsoft.SPOT;
using GT = Gadgeteer;
using System.Net.Sockets;
using Microsoft.SPOT.Hardware;
using GHI.SQLite;
using System.Collections;
using Json.NETMF;

namespace Progetto_Guido
{
    public  class InizializeDHT
    {
        GT.Socket socket;
        Cpu.Pin pin6in;
        Cpu.Pin pin7out;
        public Dht11Sensor dhtSensor{get; set;}

        public int numberP = 1;
        public int counterTm{get; set;}
        public int counterHm{get; set;}
        public int counterMm{get; set;}
        public int counterLs { get; set; }
        public HumidityMisure _last_hm{ get; set; }
        public MoistureMisure _last_mm{get; set;}
        public TemperatureMisure _last_tm{get; set;}
        public LightMisure _last_ls { get; set; }
        
        public InizializeDHT()
        {
            this.socket = GT.Socket.GetSocket(14, true, null, null);
            this.pin6in = socket.CpuPins[6];
            this.pin7out = socket.CpuPins[7];
            this.dhtSensor = new Dht11Sensor(pin6in, pin7out, Port.ResistorMode.PullUp);
            this.counterTm = 0;
            this.counterHm = 0;
            this.counterMm = 0;
            this.counterLs = 0;
            this._last_hm = new HumidityMisure(0, "FAIL");
            this._last_mm = new MoistureMisure(0, "FAIL");
            this._last_tm = new TemperatureMisure(0, "FAIL");
            this._last_ls = new LightMisure(0, "FAIL");
        }

        /// <summary>
        /// Read all sensors and validate readings, with specifices
        /// </summary>
        /// <param name="sqldb">Database to insert</param>
        /// <param name="moisture">A moinsture sensor</param>
        public void Read_and_check_values(SQL_Wrapper sqldb, Gadgeteer.Modules.GHIElectronics.Moisture moisture, Gadgeteer.Modules.GHIElectronics.LightSense lightSense)
        {    
            /*Read and Print data*/
            Utilità.ReadDHT(dhtSensor);
            
            if (dhtSensor.Temperature != _last_tm.value || counterTm == 15)
            {
                _last_tm.value = dhtSensor.Temperature;
                TemperatureMisure tm = new TemperatureMisure(dhtSensor.Temperature, "OK");
                //obj_send.addMisure(tm);
                sqldb.insert(tm,0);
                if (counterTm == 15) counterTm = 0;
            }
            else counterTm++;

            if ((dhtSensor.Humidity != _last_hm.value && dhtSensor.Humidity != 0) || counterHm == 15)
            {
                _last_hm.value = dhtSensor.Humidity;
                HumidityMisure hm = new HumidityMisure(dhtSensor.Humidity, "OK");
                //obj_send.addMisure(hm);
                sqldb.insert(hm,0);
                if (counterHm == 15) counterHm = 0;
            }
            else counterHm++;

            float moinstureData = moisture.ReadMoisture();
            Debug.Print("Moinsture      = " + moinstureData);
            if ((moinstureData != _last_mm.value && moinstureData != 0) || counterMm == 15)
            {
                _last_mm.value = moinstureData;
                MoistureMisure mm = new MoistureMisure(moinstureData, "OK");
                //obj_send.addMisure(mm);
                sqldb.insert(mm,0);
                if (counterMm == 15) counterMm = 0;
            }
            else counterMm++;

            Debug.Print(String.Empty);

            double lightDataIll = lightSense.GetIlluminance();
            double lightDataPro = lightSense.ReadProportion();
            double lightDataVolt = lightSense.ReadVoltage();
            Debug.Print("LightSenseIll  = " + lightDataIll);
            Debug.Print("LightSensePro  = " + lightDataPro);
            Debug.Print("LightSenseVolt = " + lightDataVolt);
            if (lightDataIll < (_last_ls.value * 0.85) || lightDataIll > (_last_ls.value + _last_ls.value*0.15) || counterLs == 15)
            {
                _last_ls.value = (float)lightDataIll;
                LightMisure ls = new LightMisure((float)lightDataIll, "OK");
                //obj_send.addMisure(ls);
                sqldb.insert(ls, 0);
                if (counterLs == 15) counterLs = 0;
            }
            else counterLs++;
        }

        /// <summary>
        /// Get Data from DB(last two minutes)
        /// </summary>
        /// <param name="sqldb">Database</param>
        /// <returns>Return packet to send</returns>
        public Hashtable retriveData(SQL_Wrapper sqldb)
        {
            Hashtable packet = new Hashtable();
            ResultSet data = sqldb.get_last2mindata(numberP);
            if (data.Data.Count != 0)
            {
                objson o = new objson("FEZ_46");
                foreach (ArrayList dato in data.Data)
                {
                    Misure m = new Misure();
                    m.sensor = (string)dato[0];
                    string id = dato[1].ToString();
                    m.sensor_id = Int32.Parse(id);
                    m.iso_timestamp = (string)dato[2];
                    m.value = (float)(double)dato[3];
                    m.status = (string)dato[4];
                    o.addMisure(m);
                }
                string json = JsonSerializer.SerializeObject(o.hastTable);
                packet.Add(numberP, json);
                numberP++;
            }
            else packet = null;
            //inviare questa stringa..
            return packet;
        }
    }
}
