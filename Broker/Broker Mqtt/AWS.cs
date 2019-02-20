using System;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using MQTTnet;
using MQTTnet.Client;

namespace Broker_Mqtt
{
    class AWS
    {
        private const string IoTEndPoint = "a38elfj72z1pzy.iot.us-east-1.amazonaws.com";
        //private const string IoTEndPoint = "a2lzki3obz1dhu.iot.eu-west-1.amazonaws.com"; //idilio

        private const int BrokerPort = 8883;
        public delegate void visualize(String e);
        //public IMqttClient client { get; set; }
        visualize v;
        MainWindow w;
        public IMqttClient client { get; set; }
        public event EventHandler fPublish;
        public AWS(MainWindow w)
        {
            this.w = w;
            v = Visualize;
            client = null;
        }
        public async Task Connect()
        {
            var certificate = new X509Certificate("C:\\Users\\Andre\\Desktop\\ESAMI\\Broker_Anto\\Resource\\certificate.pfx", "Guido2018", X509KeyStorageFlags.Exportable);
            //var certificate = new X509Certificate("C:\\Users\\Andre\\Desktop\\ESAMI\\Broker_Anto\\Resource\\esame.pfx", "Guido2018", X509KeyStorageFlags.Exportable);   ///idilio

            var caRoot = X509Certificate2.CreateFromSignedFile("C:\\Users\\Andre\\Desktop\\ESAMI\\Broker_Anto\\Resource\\CAroot.crt");
            MqttClientOptionsBuilder optionsBuilder = new MqttClientOptionsBuilder();
            var fact = new MqttFactory();
            client = fact.CreateMqttClient();
            client.ApplicationMessageReceived += Client_ApplicationMessageReceived;
            client.Connected += Client_Connected;

            optionsBuilder
                .WithClientId("pc-broker")
                .WithTcpServer(IoTEndPoint, 8883)
                .WithTls(true, true, true, certificate.Export(X509ContentType.Pfx), caRoot.Export(X509ContentType.Cert))
                .WithCleanSession()
                .WithProtocolVersion(MQTTnet.Serializer.MqttProtocolVersion.V311)
                .WithKeepAlivePeriod(new TimeSpan(0, 0, 5));

            try
            {
                await client.ConnectAsync(optionsBuilder.Build());
            }
            catch (Exception e)
            {
                w.Dispatcher.Invoke(v, "impossibile connect " + e.Message.ToString());
            }

            try
            {
                await client.SubscribeAsync("aws/misure");
            }
            catch (Exception e)
            {
                w.Dispatcher.Invoke(v, "impossibile subscribe " + e.Message.ToString());
            }
            while (!client.IsConnected) { }
        }

        public async void pubblish(string topic, string payload)
        {
            try
            {
                if (!client.IsConnected)
                    await this.Connect();
                await client.PublishAsync(topic, payload, MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce);
            }
            catch (Exception e)
            {
                w.Dispatcher.Invoke(v, "impossibile publish " + e.Message.ToString());
                //dirlo alla schedina con evento su main
                failed_to_publish(new EventArgs());
                throw new Exception("impossibile publish " + e.Message.ToString()); //rilancia eccezione
            }

        }

        private void Client_Connected(object sender, MQTTnet.Client.MqttClientConnectedEventArgs e)
        {
            w.Dispatcher.Invoke(v, "connected to aws successfully!");
        }

        private void Client_ApplicationMessageReceived(object sender, MqttApplicationMessageReceivedEventArgs e)
        {
            //MessageBox.Show(e.ApplicationMessage.Payload.ToString());
        }

        private void Visualize(String e)
        {
            w.lstv_log.Items.Add(e);
        }
        protected virtual void failed_to_publish(EventArgs e)
        {
            if (fPublish != null)
            {
                fPublish(this, e);
            }
        }
    }
}
