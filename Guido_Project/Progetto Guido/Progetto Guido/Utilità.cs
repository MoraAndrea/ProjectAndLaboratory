using System;
using System.Threading;
using Microsoft.SPOT;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using Microsoft.SPOT.Net;

namespace Progetto_Guido
{
    class Utilità
    {
        /// <summary>
        /// Provide Data from server NTP
        /// </summary>
        /// <returns>Return DateTime</returns>
        public static DateTime GetNetworkTime()
        {

            var ntpData = new byte[48];
            ntpData[0] = 0x1B; //LeapIndicator = 0 (no warning), VersionNum = 3 (IPv4 only), Mode = 3 (Client Mode)

            //var addresses = IPAddress.Parse("193.204.114.232");
            //var addresses = IPAddress.Parse("130.192.3.103");
            var addresses = IPAddress.Parse("193.204.114.232");
            var ipEndPoint = new IPEndPoint(addresses, 123);
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            socket.Connect(ipEndPoint);
            socket.Send(ntpData);
            socket.Receive(ntpData);
            socket.Close();

            ulong intPart = (ulong)ntpData[40] << 24 | (ulong)ntpData[41] << 16 | (ulong)ntpData[42] << 8 | (ulong)ntpData[43];
            ulong fractPart = (ulong)ntpData[44] << 24 | (ulong)ntpData[45] << 16 | (ulong)ntpData[46] << 8 | (ulong)ntpData[47];

            var milliseconds = (intPart * 1000) + ((fractPart * 1000) / 0x100000000L);
            DateTime networkDateTime = (new DateTime(1900, 1, 1)).AddMilliseconds((long)milliseconds);

            DateTime a = networkDateTime.ToLocalTime();

            //string time = a.Year + "-" + a.Month + "-" + a.Day + "T" + a.Hour + ":" + a.Minute + ":" + a.Second + "+00:00";
            //return networkDateTime.ToLocalTime().ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'+00:00'");
            return networkDateTime.ToLocalTime();
        }

        /// <summary>
        /// Read data from DHT and print
        /// </summary>
        /// <param name="dhtSensor">Sensor DHT</param>
        /// <param name="cnt">A counter</param>
        public static void ReadDHT(Dht11Sensor dhtSensor)
        {
            if (dhtSensor.Read())
            {
                //Debug.Print(cnt.ToString() + ":");
                Debug.Print("\n----------------------------------------");
                Debug.Print("Data:  "+DateTime.Now.ToString());
                PrintDHT11Mesure(dhtSensor);
            }
            else
            {
                /* first time always fail than it correct itself */
                Debug.Print("DHT sensor Read() failed");
                Thread.Sleep(2000);
            }
        }

        /// <summary>
        /// Print data read from sensor DHT
        /// </summary>
        /// <param name="dhtSensor">Sensor DHT used</param>
        private static void PrintDHT11Mesure(Dht11Sensor dhtSensor)
        {
            Debug.Print("Temp Celsius   = " + dhtSensor.Temperature.ToString("F1") + "°C");
            //Debug.Print("Temp Kelvin    = " + dhtSensor.TemperatureKelvin.ToString("F1") + "°K");
            //Debug.Print("Temp Farenhein = " + dhtSensor.TemperatureFarenheit.ToString("F1") + "°F");
            Debug.Print(String.Empty);

            Debug.Print("Humidity       = " + dhtSensor.Humidity.ToString("F1") + " %");
            Debug.Print(String.Empty);
        }
    }
}
