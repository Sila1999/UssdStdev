using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Protocol;
using MQTTnet.Server;


namespace ussd
{
    internal class MqttClientManager
    {
        private readonly IMqttClient _mqttClient;
        private readonly MqttClientOptions _options;
        private MqttClientConnectResult _connectResult;

        public MqttClientManager(string broker, int port, string clientId, string username, string password)
        {
            //Create a MQTT client factory
            var factory = new MqttFactory();
            // Create a MQTT client instance
            this._mqttClient = factory.CreateMqttClient();
            // Create MQTT client options
            this._options = new MqttClientOptionsBuilder()
                .WithTcpServer(broker, port) // MQTT broker address and port
                .WithCredentials(username, password) // Set username and password
                .WithClientId(clientId)
                .WithCleanSession()
                .Build();
            // Connect to MQTT broker
           // ConnectAsync().Wait();
            //mqttClient.ConnectAsync(options);
        }

        public async Task ConnectAsync()
        {
            // Connect to MQTT broker
            try
            {
                this._connectResult = await _mqttClient.ConnectAsync(_options, CancellationToken.None);
                Console.WriteLine("Connecté au serveur MQTT.");
            }
            catch (Exception ex) 
            {
                Console.WriteLine(  ex.ToString()); 
            }
        }
        
        public async Task DisconnectAsync()
        {
            // Connect to MQTT broker
            try
            {
                await _mqttClient.DisconnectAsync();
                Console.WriteLine("Connecté au serveur MQTT.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public async Task PublishAsync(string topic, string payload)
        {
            try
            {
                var message = new MqttApplicationMessageBuilder()
                    .WithTopic(topic)
                    .WithPayload(payload)
                    .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                    .WithRetainFlag()
                    .Build();
                await _mqttClient.PublishAsync(message, CancellationToken.None);
                Console.WriteLine("Message envoyé au serveur MQTT.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}
