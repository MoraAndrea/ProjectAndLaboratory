using System;
using Microsoft.SPOT;
using GHI.Networking;
using Gadgeteer.Modules.GHIElectronics;
using System.Text;
using System.Net;
using System.IO;

namespace Progetto_Guido
{
    public class WiFiConnection
    {
        public  event EventHandler WifiConnected;
        public  void connectToWiFI(WiFiRS21 wifiRS21, string ssid, string password)
        {
            string pass=password;
            
           // public event EventHandler WiFiConnected;
            //WiFiRS9110.NetworkParameters net = new WiFiRS9110.NetworkParameters();
            //net.SecurityMode = WiFiRS9110.SecurityMode.Wep;
            //net.Key = "tamarindo3579";
            //net.Key = "74616D6172696E646F33353739"; 
            //net.Ssid = "ProjecAndLab";
            //net.NetworkType = WiFiRS9110.NetworkType.AccessPoint;
            //net.Channel = 6;

            //WiFiRS9110.NetworkParameters[] c = wifiRS21.NetworkInterface.Scan();
            try
            {
                if (wifiRS21 == null || !(wifiRS21.IsNetworkConnected && wifiRS21.IsNetworkUp))
                {
                    Debug.Print("Try to connect");

                    if (!wifiRS21.NetworkInterface.Opened)
                    {
                        wifiRS21.NetworkInterface.Open();
                    }

                    if (!wifiRS21.NetworkInterface.IsDhcpEnabled)
                    {
                        wifiRS21.NetworkInterface.EnableDhcp();
                    }

                    WiFiRS9110.NetworkParameters[] nets = wifiRS21.NetworkInterface.Scan(ssid);

                    if (nets.Length > 0)
                    {
                        if (nets[0].SecurityMode == GHI.Networking.WiFiRS9110.SecurityMode.Wep)
                            pass = HexToString(password);
                        if (nets[0].SecurityMode == GHI.Networking.WiFiRS9110.SecurityMode.Open)
                            pass = "";

                        nets[0].Key = pass;
                        wifiRS21.NetworkInterface.Join(nets[0]);
                        while (wifiRS21.NetworkInterface.IPAddress=="0.0.0.0")
                        {
                            //Debug.Print(wifiRS21.NetworkInterface.IPAddress);
                        }
                        Debug.Print(wifiRS21.NetworkInterface.IPAddress);
                        Debug.Print("Connected");
                        wifi_connected(new EventArgs());
                        
                    }
                    else
                    {
                        Debug.Print("Not found network");
                    }
                }
            }
            catch (Exception e)
            {
                Debug.Print(e.Message);
            }
        }

        // wifiRS21.NetworkInterface.Join("ProjecAndLab","tamarindo3579");

        //Microsoft.SPOT.Net.NetworkInformation.NetworkInterface networkInterface =
        //        Microsoft.SPOT.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces()[0];

        // Debug.Print("IP address: " + networkInterface.IPAddress.ToString()); 
        //     Debug.Print("SubnetMask: " + networkInterface.SubnetMask.ToString());
        //     Debug.Print("GatewayAddress: " + wifiRS21.NetworkInterface.GatewayAddress.ToString()); 
        //      Debug.Print("NetworkInterfaceType: " + wifiRS21.NetworkInterface.NetworkInterfaceType.ToString());



        private string HexToString(string str)
        {
            var sb = new StringBuilder();

            var bytes = Encoding.UTF8.GetBytes(str);
            foreach (var t in bytes)
            {
                sb.Append(t.ToString("X2"));
            }

            return sb.ToString(); // returns: HEX from String
        }

        protected  virtual void wifi_connected(EventArgs e)
        {

            if (WifiConnected != null)
            {
                WifiConnected(this, e);
            }
        }

    }

}
