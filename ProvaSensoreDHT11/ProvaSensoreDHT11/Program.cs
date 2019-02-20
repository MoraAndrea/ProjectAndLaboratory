using System;
using System.Collections;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Presentation;
using Microsoft.SPOT.Presentation.Controls;
using Microsoft.SPOT.Presentation.Media;
using Microsoft.SPOT.Presentation.Shapes;
using Microsoft.SPOT.Touch;

using Gadgeteer.Networking;
using GT = Gadgeteer;
using GTM = Gadgeteer.Modules;
using Microsoft.SPOT.Hardware;
using Gadgeteer.Modules.GHIElectronics;

namespace ProvaSensoreDHT11
{
    public partial class Program
    {
        // This method is run when the mainboard is powered up or reset.   
        void ProgramStarted()
        {
            // Use Debug.Print to show messages in Visual Studio's "Output" window during debugging.
            Debug.Print("Program Started");
            setup();
        }

        public static void setup()
        {
            GT.Socket socket = GT.Socket.GetSocket(14, true, null, null);
            Cpu.Pin pin3in = socket.CpuPins[4];
            Cpu.Pin pin8out = socket.CpuPins[5];

            /*   D2     D3    */
            Dht11Sensor dhtSensor = new Dht11Sensor(pin3in, pin8out, Port.ResistorMode.PullUp);

            int cnt = 0; /* just a simple counter */
            Debug.Print("----------------------------------------");

            /* loopa e ngjarjeve dhe funksionet */
            while (true)
            {
                cnt++;

                if (dhtSensor.Read())
                {
                    Debug.Print(cnt.ToString() + ":");
                    Debug.Print("----------------------------------------");

                    Debug.Print("Temp Celsius   = " + dhtSensor.Temperature.ToString("F1") + "°C");
                    Debug.Print("Temp Kelvin    = " + dhtSensor.TemperatureKelvin.ToString("F1") + "°K");
                    Debug.Print("Temp Farenhein = " + dhtSensor.TemperatureFarenheit.ToString("F1") + "°F");
                    Debug.Print(String.Empty);

                    Debug.Print("Humidity       = " + dhtSensor.Humidity.ToString("F1") + " %");
                    Debug.Print(String.Empty);

                    Thread.Sleep(2000);
                    
                }
                else
                {
                    /* first time always fail than it correct itself */
                    Debug.Print("DHT sensor Read() failed");
                    Thread.Sleep(2000);

                }
            }

        }


    }
}
