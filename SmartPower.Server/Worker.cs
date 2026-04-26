using MQTTnet;

namespace SmartPower.Server
{
    public class Worker : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var mqttFactory = new MqttClientFactory();
            using var client = mqttFactory.CreateMqttClient();
            var mqttClientOptions = new MqttClientOptionsBuilder().WithTcpServer("localhost").Build();

            // Setup message handling before connecting so that queued messages
            // are also handled properly. When there is no event handler attached all
            // received messages get lost.
            client.ApplicationMessageReceivedAsync += e =>
            {
                var t = e.ApplicationMessage.Topic;
                return Task.CompletedTask;
            };

            await client.ConnectAsync(mqttClientOptions, stoppingToken);
        }
    }
}