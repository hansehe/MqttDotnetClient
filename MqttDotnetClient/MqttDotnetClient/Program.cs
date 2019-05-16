using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using MQTTnet;
using MQTTnet.Client.Options;
using MQTTnet.Extensions.ManagedClient;

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
            var mqttClient = new MqttFactory().CreateManagedMqttClient();
            SetupMqttClient(mqttClient).Wait();
            
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

        private static async Task SetupMqttClient(IManagedMqttClient mqttClient)
        {
            var options = CreateManagedMqttClientOptions();
            mqttClient.UseApplicationMessageReceivedHandler(async e =>
            {
                Console.WriteLine("### RECEIVED APPLICATION MESSAGE ###");
                Console.WriteLine($"+ Topic = {e.ApplicationMessage.Topic}");
                Console.WriteLine($"+ Payload = {Encoding.UTF8.GetString(e.ApplicationMessage.Payload)}");
                Console.WriteLine($"+ QoS = {e.ApplicationMessage.QualityOfServiceLevel}");
                Console.WriteLine($"+ Retained = {e.ApplicationMessage.Retain}");
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