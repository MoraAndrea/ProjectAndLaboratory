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
using System.Net.Sockets;
using GHI.Processor;
using GHI.Networking;

namespace Progetto_Guido
{
    class brokerIP
    {
        Socket s;
        public event EventHandler IpObteined;
        public brokerIP()
        {
            s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            s.ReceiveTimeout = 6000;
            s.Bind(new IPEndPoint(IPAddress.Any, 8080));
        }

        public void startListening()
        {
            byte[] buffer = new byte[1024];
            while (true)
            {
                try
                {
                    s.Receive(buffer);
                }
                catch (Exception )
                {
                    Program.broker = null;
                }
                var sb = new StringBuilder(buffer.Length);
                foreach (var b in buffer)
                {
                    sb.Append((char)b);
                }
                var str = sb.ToString();
                string[] info = str.Split(':');
                if (info[0] == "Fez46")
                {
                    if (Program.broker == null) Program.broker = IPAddress.Any;
                    if (Program.broker.ToString() != info[1])
                    {
                        Program.broker = IPAddress.Parse(info[1]);
                        Program.port = Int32.Parse(info[2]);
                        ObteinedEvent(new EventArgs());
                        Debug.Print("Event BROKER RAISED");
                    }
                }
                if (Program.broker == IPAddress.Any) Program.broker = null;

                if (Program.broker == null)
                    Thread.Sleep(1000);
                else Thread.Sleep(1000 * 60);
            }
        }

        protected virtual void ObteinedEvent(EventArgs e)
        {
            if (IpObteined != null)
            {
                IpObteined(this, e);
            }
        }

    }
}
