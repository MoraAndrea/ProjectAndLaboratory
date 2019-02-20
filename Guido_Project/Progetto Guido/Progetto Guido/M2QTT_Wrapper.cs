using System;
using Microsoft.SPOT;
using uPLibrary.Networking.M2Mqtt;
using System.Text;
using uPLibrary.Networking.M2Mqtt.Messages;
using System.Threading;

namespace Progetto_Guido
{
    public class M2QTT_Wrapper
    {
        MqttClient client;
        string clientId;
        SQL_Wrapper sqldb;
        string ip;

        public M2QTT_Wrapper(string ip, SQL_Wrapper sqldb)
        {
            this.sqldb = sqldb;
            this.ip = ip;
        }

        public void sendToBroker(string jsonByte, int num, bool conf)
        {
            string topic = null;
            string topicSend = null; ;
            string strValue = null; ;
            string BrokerAddress = ip;

            client = new MqttClient(BrokerAddress);

            // register a callback-function (we have to implement, see below) which is called by the library when a message was received
            client.MqttMsgPublishReceived += client_MqttMsgPublishReceived;

            // use a unique id as client id, each time we start the application
            clientId = Guid.NewGuid().ToString();
            if (clientId != null)
            {
                client.Connect(clientId);
                if (conf == true)
                {
                    topic = "reply/configuration";
                    topicSend = "send/configuration/" + num;
                    strValue = jsonByte;
                }
                else
                {
                    topic = "reply/topic";
                    topicSend = "send/topic/" + num;
                    strValue = jsonByte;
                }

                // subscribe to the topic with QoS 1
                client.Subscribe(new string[] { topic }, new byte[] { MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE });

                // publish a message on "test/topic" topic with QoS 1 
                client.Publish(topicSend, Encoding.UTF8.GetBytes(strValue), MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE, false);
                Thread.Sleep(100);
            }
        }

        private void SubscribeOnTopic(object sender, RoutedEventArgs e)
        {
            // whole topic
            string topic = "reply/topic";

            // subscribe to the topic with QoS 2
            client.Subscribe(new string[] { topic }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });
        }

        public void Disconnect()
        {
            client.Disconnect();

        }

        private void PublishOnTopic(object sender, RoutedEventArgs e)
        {
            // whole topic
            string topicSend = "send/topic/" + 1;

            // publish a message with QoS 1
            string strValue = "prova";

            // publish a message on "/home/temperature" topic with QoS 1 
            client.Publish(topicSend, Encoding.UTF8.GetBytes(strValue), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, false);
        }

        // this code runs when a message was received
        void client_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
        {
            Encoding utf8 = Encoding.UTF8;
            if (e.Topic != "test/errors")
            {
                char[] ReceivedMessageChar = utf8.GetChars(e.Message, 0, e.Message.Length);

                var sb = new StringBuilder(e.Message.Length);
                foreach (var b in e.Message)
                {
                    sb.Append((char)b);
                }
                var str = sb.ToString();
                Int32 numPack = Int32.Parse(str);

                //cancello nel database i pacchetti con numero receivedMessage
                sqldb.DeleteMisuresRecived(numPack);
            }
        }
    }
}
