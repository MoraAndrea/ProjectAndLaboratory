using System;
using System.Text;
using System.Security.Cryptography.X509Certificates;
using System.Windows;
using System.Timers;
using MQTTnet;
using MQTTnet.Server;
using MQTTnet.Client;
using System.Net.Sockets;
using System.Net;

namespace Broker_Mqtt
{
    /// <summary>
    /// Logica di interazione per MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public delegate void visualize(MqttApplicationMessageReceivedEventArgs e);
        System.Timers.Timer t = new System.Timers.Timer(3000);
        
        UdpClient me = new UdpClient(new IPEndPoint(IPAddress.Parse(Utilities.GetLocalIp().ToString()), 45445));
        IMqttServer mqttServer;
        AWS a;
        static public IMqttClient client;
        public MainWindow()
        {
            InitializeComponent();
            t.Elapsed += advertisment; 
        }

        //Send his ip every 3 sec
        private void advertisment(object sender, ElapsedEventArgs e)
        {
            IPAddress myIP = Utilities.GetLocalIp();
            //string myIP = Utilities.GetLocalIp();
            string advS = "Fez46:"+ myIP + ":8080";
            byte[] advB = Encoding.ASCII.GetBytes(advS);
            //IPEndPoint towards = new IPEndPoint(Utilities.findBroadCast(IPAddress.Parse(myIP), Utilities.getSubMask(IPAddress.Parse(myIP))), 8080);
            IPEndPoint towards = new IPEndPoint(Utilities.GetBroadcastAddress(myIP,Utilities.GetNetmask()), 45445);
            try
            {
                me.Send(advB, advB.Length, towards);
            }
            catch (Exception p){
                MessageBox.Show(p.Message);
            }
        }

        private async void btn_startBroker_ClickAsync(object sender, RoutedEventArgs e)
        {
            t.Start();
            mqttServer = new MqttFactory().CreateMqttServer();
            await mqttServer.StartAsync(new MqttServerOptions());
            lstv_log.Items.Add("Started broker");
            mqttServer.ApplicationMessageReceived += Message_consumer;

            a = new AWS(this);
            a.fPublish += Failed_pAsync;
            a.Connect(); //problema: se non si è ancora connesso non fa la publish
        }

        private async void Failed_pAsync(object sender, EventArgs e)
        {
           // lstv_log.Items.Add("unlucky");
            var message = new MqttApplicationMessageBuilder()
                .WithTopic("test/errors")
                .WithPayload("Impossible send data")
                .Build();
           await mqttServer.PublishAsync(message);
        }

        private async void Message_consumer(object sender, MqttApplicationMessageReceivedEventArgs e)
        {
            visualize v = Visualize;
            // lstv_log.Items.Add(e.ApplicationMessage.Topic);
            Dispatcher.Invoke(v, e);
            if (e.ApplicationMessage.Topic == "test/errors")
            {
                return;
            }
            string[] splittedMessage = e.ApplicationMessage.Topic.Split('/');
            string control = splittedMessage[0] + "/" + splittedMessage[1];
            if (splittedMessage[1] != "send/configuration" && control!= "reply/topic")
            {
                int numPack = Int32.Parse(splittedMessage[2]);
            }
            if (control == "send/topic")
            {
                //File.WriteAllText("Here.json", Encoding.UTF8.GetString(e.ApplicationMessage.Payload));
                var message = new MqttApplicationMessageBuilder()
                                .WithTopic("reply/topic")
                                .WithPayload(splittedMessage[2])       //.WithPayload(splittedMessage[2] + ":ACK")
                                .Build();

                //-->Send to Amazon
                // a.pubblish("aws/misure", Encoding.UTF8.GetString(e.ApplicationMessage.Payload)); //FEZ46/measurements
                try
                {
                    a.pubblish("FEZ46/measurements", Encoding.UTF8.GetString(e.ApplicationMessage.Payload));
                }
                catch (Exception r)
                {
                    return;
                }
                //Dispatcher.Invoke(v,e);                  
                await mqttServer.PublishAsync(message);
            }
            if (control == "send/configuration")
            {
                //a.pubblish("aws/configuration", Encoding.UTF8.GetString(e.ApplicationMessage.Payload)); //FEZ46/configuration
                a.pubblish("FEZ46/configuration", Encoding.UTF8.GetString(e.ApplicationMessage.Payload));
            }
        }

        public void Visualize(MqttApplicationMessageReceivedEventArgs e)
        {          
            lstv_log.Items.Add("        ----------------------      ");
            lstv_log.Items.Add("+ Topic = "+e.ApplicationMessage.Topic);
            lstv_log.Items.Add("+ Payload = "+Encoding.UTF8.GetString(e.ApplicationMessage.Payload));
            // lstv_log.Items.Add($"+ QoS = {e.ApplicationMessage.QualityOfServiceLevel}");
            //lstv_log.Items.Add($"+ Retain = {e.ApplicationMessage.Retain}");
            lstv_log.Items.Add("        ----------------------      ");
        }

        private async void btn_publish_Click(object sender, RoutedEventArgs e)
        {
            var message = new MqttApplicationMessageBuilder()
                            .WithTopic("replay/topic")
                            .WithPayload("Hello World")
                            .Build();
            await mqttServer.PublishAsync(message);
        }

        private async void btn_stop_Click(object sender, RoutedEventArgs e)
        {
            await mqttServer.StopAsync();
            lstv_log.Items.Add("Broker stopped.");
            t.Stop();
        }
    }
}
