using System;
using System.Collections;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Presentation;
using Microsoft.SPOT.Presentation.Controls;
using Microsoft.SPOT.Presentation.Media;
using Microsoft.SPOT.Presentation.Shapes;
using Microsoft.SPOT.Touch;
using Json.NETMF;
using Gadgeteer.Networking;
using GT = Gadgeteer;
using GTM = Gadgeteer.Modules;
using Microsoft.SPOT.Hardware;
using Gadgeteer.Modules.GHIElectronics;
using System.Text;
using Progetto_Guido;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using System.Net;
using GHI.Processor;
using GHI.Networking;
using System.IO;

namespace Progetto_Guido
{
    public partial class Program
    {
        WiFiConnection wifiC = null;
        SQL_Wrapper sqldb = null;
        InizializeDHT init = null;
        M2QTT_Wrapper client = null;
        //PERCHE' SUL MIO PC NON VA BROADCAST
        public static IPAddress broker = IPAddress.Parse("192.168.43.145");
        //public static IPAddress broker = null;
        public static int port=0;
        bool firstTime = true;
        string configuration = null;

        string SSID = "Luca";
        string PASSWORD = "ciaociao";

        // This method is run when the mainboard is powered up or reset.   
        void ProgramStarted()
        {
            Debug.Print("Program Started");
            #region ForEthernet
            //ethernetJ11D.UseThisNetworkInterface();
            //ethernetJ11D.UseStaticIP("192.168.137.1","255.255.255.0","192.168.137.46");

            //ethernetJ11D.NetworkUp += ethernetJ11D_NetworkUp;
            //ethernetJ11D.NetworkDown += ethernetJ11D_NetworkDown;
            #endregion

            /*Inizialized Database for Misure*/
            sqldb = new SQL_Wrapper(sdCard);
            sqldb.setup_database();

            /*Create Configuration*/
            //Configuration conf = new Configuration();
            StreamReader d = new StreamReader(sdCard.StorageDevice.RootDirectory + "\\configuration.json");
            configuration = d.ReadToEnd();

            /*Inizialized DHT11 sensor*/
            init = new InizializeDHT();

            /*Start Timer for read sensors*/
            GT.Timer t6sec = new GT.Timer(6000);
            t6sec.Tick += read_sensors;
            t6sec.Start();

            /*Start Timer for Backup DataBase*/
            //GT.Timer tBackup = new GT.Timer(1000 * 60 * 7); //7 minuti
            //tBackup.Tick += BackupDB;
            //tBackup.Start();

            /*Activate WiFi interface*/
            WiFiRS9110.NetworkParameters mynet = new WiFiRS9110.NetworkParameters();
            if (!wifiRS21.NetworkInterface.Opened) wifiRS21.NetworkInterface.Open();
            wifiRS21.UseThisNetworkInterface();

            #region AdHoc
            //mynet.Channel= 5;
            //mynet.Ssid="prova";
            //mynet.Key=" ";         
            //mynet.SecurityMode = WiFiRS9110.SecurityMode.Open;
            //mynet.NetworkType = WiFiRS9110.NetworkType.AdHoc;
            ////wifiRS21.NetworkInterface.EnableStaticIP("192.168.1.100", "255.255.0.0", "192.168.1.1");
            //wifiRS21.NetworkInterface.StartAdHocNetwork(mynet);



            //Thread.Sleep(2000);
            //HttpListener l = new HttpListener("http");

            //l.Start();
            //var contex = l.GetContext();
            //var respons = contex.Response;

            //const string r = "<html><body>Hello world</body></html>";
            //var buffer = System.Text.Encoding.UTF8.GetBytes(r);
            //respons.ContentLength64 = buffer.Length;
            //var output = respons.OutputStream;
            //output.Write(buffer, 0, buffer.Length);
            //output.Close();
            #endregion

            /*Connect to wifi*/
            wifiC = new WiFiConnection();
            wifiC.WifiConnected += wifiC_WifiConnected;
            wifiC.connectToWiFI(wifiRS21, SSID, PASSWORD);

            //setup();
        }

        void wifiC_WifiConnected(object sender, EventArgs e)
        {
           //QUA CI VANNO LE COSE QUANDO E CONNESSO
            Debug.Print("Evento WiFi Connesso Ricevuto");
            #region Check connection
            /*Check connection*/
            //HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://www.google.it");
            //HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            //if (response.StatusDescription == " OK")
            //{
            //    Debug.Print("Connect on internet");
            //}
            #endregion
            
            /*Set Data*/
            RealTimeClock.SetDateTime(Utilità.GetNetworkTime());

            
            /*Start Timer for syncronize time*/
            Utility.SetLocalTime(Utilità.GetNetworkTime());
            GT.Timer syncClock = new GT.Timer(1000 * 60 * 30);
            if (!syncClock.IsRunning)
            {
                syncClock.Tick += sincronizzazione_clock;
                syncClock.Start();
            }
            
            //solo prova perchè su mio pc non va broadcast!!!!!!!!!!!!!!!!!!!!
            client = new M2QTT_Wrapper("192.168.43.145", sqldb);
            /*Start Timer for send data*/
            GT.Timer t2min = new GT.Timer(2000 * 60);
            if (!t2min.IsRunning)
            {
                t2min.Tick += send_data;
                t2min.Start();
            }
            
            //FINO A QUA, DA TOGLIERE
            
            /*Find Broker IP*/
            //brokerIP broker = new brokerIP();
            //broker.IpObteined += b_IpObteined;
            //Thread scanIP = new Thread(broker.startListening);
            //scanIP.Start();
        }
        
        void b_IpObteined(object sender, EventArgs e)
        {
            Debug.Print("Evento IP Broker Ottenuto Ricevuto");
            /*Inizialize Client MQTT*/
            //client.Disconnect();
            client = new M2QTT_Wrapper(broker.ToString(), sqldb);
                        
            /*Start Timer for send data*/
            GT.Timer t2min = new GT.Timer(500 * 60);
            t2min.Tick += send_data;            
            t2min.Start();
        }

        /// <summary>
        /// Synchronize time
        /// </summary>
        /// <param name="timer">Timer</param>
        private void sincronizzazione_clock(GT.Timer timer)
        {
            Utility.SetLocalTime(Utilità.GetNetworkTime());
            Debug.Print("SYNC");
            //RealTimeClock.SetDateTime(DateTime.Now);
        }

        /// <summary>
        /// Send Database data
        /// </summary>
        /// <param name="timer">Timer</param>
        private void send_data(GT.Timer timer)
        {
            if (firstTime == true)
            {
                client.sendToBroker(configuration, -1 , true);
                firstTime = false;
            }
            if (DateTime.Now.Year != 2011)
            {
                if (wifiRS21.IsNetworkConnected && broker != null)
                {
                    Hashtable packet = new Hashtable();
                    if ((packet = init.retriveData(sqldb)) != null)
                    {
                        foreach (DictionaryEntry de in packet)
                        {
                            int num = (int)de.Key;
                            string json = (string)de.Value;
                            Debug.Print("sending....!");
                            client.sendToBroker(json, num, false);
                            Debug.Print("DATA SENDED!");
                        }
                    }
                }
                else
                {
                    Debug.Print("RECONNECTION");
                   // wifiC = new WiFiConnection();
                   // wifiC.WifiConnected += wifiC_WifiConnected;
                    wifiC.connectToWiFI(wifiRS21, SSID, PASSWORD);
                }
            }
        }

        /// <summary>
        /// Create a copy of database for security
        /// </summary>
        private void BackupDB(GT.Timer timer)
        {
            try
            {
                // Will overwrite if the destination file already exists.
                File.Copy(Path.Combine(sdCard.StorageDevice.RootDirectory, "Database.dat"), Path.Combine(sdCard.StorageDevice.RootDirectory, "DatabaseBackup.dat"), true);
            }
            catch (IOException copyError)
            {
                Debug.Print(copyError.Message);
            }
        }

        /// <summary>
        /// Read all sensors and inserts into the Database
        /// </summary>
        /// <param name="timer">Timer</param>
        private void read_sensors(GT.Timer timer)
        {
            if (DateTime.Now.Year != 2011)
            {
                try
                {
                    Debug.Print("Lettura in corso...");
                    init.Read_and_check_values(sqldb, moisture, lightSense);
                }
                catch (Exception e) { Debug.Print(e.Message); };
            }
            else Debug.Print("Data non sincronizzata...");
        }

        #region Events Inutili

        private void wifiRS21_NetworkUp(GTM.Module.NetworkModule sender, GTM.Module.NetworkModule.NetworkState state)
        {
            Debug.Print("WiFi Network UP");
            Debug.Print("LinkConnected is: " + wifiRS21.NetworkInterface.LinkConnected.ToString());
            Debug.Print("NetworkAvailable is: " + wifiRS21.NetworkInterface.NetworkAvailable.ToString());
        }

        private void wifiRS21_NetworkDown(GTM.Module.NetworkModule sender, GTM.Module.NetworkModule.NetworkState state)
        {
            Debug.Print("wifiRS21 Network Down. Always down before it is up..");
        }
        #endregion

        #region Old Implementation
        //public void setup()
        //{


        //    /* loopa e ngjarjeve dhe funksionet */
        //    while (true)
        //    {
        //        objson obj_send = new objson("FEZ_46");
        //cnt = 0;
        //while (cnt <= 15)
        //{
        //    cnt++;

        //ReadDHT(init.dhtSensor, cnt);
        //if (dhtSensor.Temperature != _last_tm.value || counterTm == 15)
        //{
        //    _last_tm.value = dhtSensor.Temperature;
        //    TemperatureMisure tm = new TemperatureMisure(dhtSensor.Temperature);
        //    //obj_send.addMisure(tm);
        //    sqldb.insert(tm);
        //    if (counterTm == 15) counterTm = 0; else counterTm++;
        //}
        //if (dhtSensor.Humidity != _last_hm.value || counterHm == 15)
        //{
        //    _last_hm.value = dhtSensor.Humidity;
        //    HumidityMisure hm = new HumidityMisure(dhtSensor.Humidity);
        //    //obj_send.addMisure(hm);
        //    sqldb.insert(hm);
        //    if (counterHm == 15) counterHm = 0; else counterHm++;
        //}
        //float a = moisture.ReadMoisture();
        //if ( a != _last_mm.value || counterMm == 15)
        //{
        //    _last_mm.value = a;
        //    MoistureMisure mm = new MoistureMisure(a);
        //    //obj_send.addMisure(mm);
        //    sqldb.insert(mm);
        //    if (counterMm == 15) counterMm = 0; else counterMm++;
        //}




        //objson_support os = new objson_support(obj_send);
        //String jsonM = JsonSerializer.SerializeObject(obj_send.hastTable);
        //byte[] jsonByte = Encoding.UTF8.GetBytes(jsonM);

        //if (ethernetJ11D.IsNetworkUp == true)
        //{
        //    //network is up


        //    M2QTT_Wrapper Client = new M2QTT_Wrapper();
        //    Client.init();

        //}
        //else
        //{
        //    //salvare dati
        //    sdCard.StorageDevice.WriteFile("MisureLog" + cnt.ToString() + ".json", jsonByte);
        //    sdCard.StorageDevice.Volume.FlushAll();



        //    }
        //}

        //void ethernetJ11D_NetworkDown(GTM.Module.NetworkModule sender, GTM.Module.NetworkModule.NetworkState state)
        //{ 
        //    Debug.Print("Network is down!"); 
        //}

        //void ethernetJ11D_NetworkUp(GTM.Module.NetworkModule sender, GTM.Module.NetworkModule.NetworkState state)
        //{
        //    Debug.Print("Network is up!");
        //    Debug.Print("My IP is: " + ethernetJ11D.NetworkSettings.IPAddress);
        //}
        #endregion
        
    }
}


  


