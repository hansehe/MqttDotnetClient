using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using MQTTnet;
using MQTTnet.Client.Options;
using MQTTnet.Extensions.ManagedClient;
using MQTTnet.Protocol;

/// Useful links:
///     MqttNet source code: https://github.com/chkr1011/MQTTnet
///     Mqtt details: https://mosquitto.org/man/mqtt-7.html
///     Mqtt qos security details: http://www.steves-internet-guide.com/understanding-mqtt-qos-levels-part-1/
///
/// Note! RabbitMq does not suppert QoS 2 (Exactly once), and will downgrade the security to QoS 1 (At least once).
///     See: https://www.rabbitmq.com/mqtt.html

namespace MqttDotnetClient
{
    internal class Program
    {
        private const string MqttBrokerHostname = "localhost";
        private const int MqttBrokerPort = 8883;
        private const string CertificatePath = "client_certificate.p12";
        private const string CertificatePassword = "password";
        private const string Username = "amqp";
        private const string Password = "amqp";
        
        private static readonly string ClientId = $"Client_{Guid.NewGuid().ToString()}";
        private const string Topic = "MyTopic";
        private const string Message = "Hello World";
        
        public static void Main(string[] args)
        {
            var options = CreateManagedMqttClientOptions();
            var mqttClient = CreateManagedMqttClient();
            SetupMqttClient(mqttClient, options).Wait();
            
            Console.ReadLine();

            mqttClient.StopAsync().Wait(TimeSpan.FromSeconds(5));
        }

        private static ManagedMqttClientOptions CreateManagedMqttClientOptions()
        {
            var options = new ManagedMqttClientOptionsBuilder()
                .WithAutoReconnectDelay(TimeSpan.FromSeconds(5))
                .WithClientOptions(new MqttClientOptionsBuilder()
                    .WithClientId(ClientId)
                    .WithTcpServer(MqttBrokerHostname, MqttBrokerPort)
                    .WithCredentials(Username, Password)
                    .WithTls(UpdateTlsParameters)
                    .Build())
                .Build();
            return options;
        }

        private static IManagedMqttClient CreateManagedMqttClient()
        {
            var mqttClient = new MqttFactory().CreateManagedMqttClient();
            return mqttClient;
        }

        private static async Task SetupMqttClient(IManagedMqttClient mqttClient, ManagedMqttClientOptions options)
        {
            
            mqttClient.UseApplicationMessageReceivedHandler(async e =>
            {
                Console.WriteLine("### RECEIVED APPLICATION MESSAGE ###");
                Console.WriteLine($"+ Topic = {e.ApplicationMessage.Topic}");
                Console.WriteLine($"+ Payload = {Encoding.UTF8.GetString(e.ApplicationMessage.Payload)}");
                Console.WriteLine($"+ QoS = {e.ApplicationMessage.QualityOfServiceLevel}");
                Console.WriteLine($"+ Retain = {e.ApplicationMessage.Retain}");
                Console.WriteLine();

                await mqttClient.PublishAsync(CreateMessage());
            });
            
            mqttClient.UseConnectedHandler(async e =>
            {
                Console.WriteLine("### CONNECTED WITH SERVER ###");

                await mqttClient.SubscribeAsync(CreateTopicFilter());
            
                await mqttClient.PublishAsync(CreateMessage());

                Console.WriteLine("### SUBSCRIBED ###");
            });

            mqttClient.ConnectingFailedHandler = new ConnectingFailedHandlerDelegate(e =>
            {
                Console.WriteLine(e.Exception.GetBaseException().Message);
            });
            
            await mqttClient.StartAsync(options);
        }

        private static void UpdateTlsParameters(MqttClientOptionsBuilderTlsParameters tlsParameters)
        {
            var certBytes = File.ReadAllBytes(CertificatePath);
            var cert = new X509Certificate2(certBytes, CertificatePassword, X509KeyStorageFlags.Exportable);
            
            tlsParameters.UseTls = true;
            tlsParameters.AllowUntrustedCertificates = true;
            tlsParameters.Certificates = new[] {cert.Export(X509ContentType.Pfx)};
        }
        
        private static TopicFilter CreateTopicFilter()
        {
            var topicFilter = new TopicFilterBuilder()
                .WithTopic(Topic)
                .WithAtLeastOnceQoS()
                .Build();
            
            return topicFilter;
        }

        private static MqttApplicationMessage CreateMessage()
        {
            var message = new MqttApplicationMessageBuilder()
                .WithTopic(Topic)
                .WithPayload(Message)
                .WithAtLeastOnceQoS()
                .WithRetainFlag()
                .Build();
            
            return message;
        }
    }
}