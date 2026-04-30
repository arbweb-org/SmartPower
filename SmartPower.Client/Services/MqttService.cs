using MQTTnet;
using System.Text;

namespace SmartPower.Client.Services;

public class MqttService
{
    private IMqttClient _client;

    public event Action<string, string>? MessageReceived;

    public async Task ConnectAsync()
    {
        var factory = new MqttClientFactory();
        _client = factory.CreateMqttClient();

        _client.ApplicationMessageReceivedAsync += e =>
        {
            var topic = e.ApplicationMessage.Topic;
            var payload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);

            MessageReceived?.Invoke(topic, payload);

            return Task.CompletedTask;
        };

        var options = new MqttClientOptionsBuilder()
            .WithTcpServer("localhost", 1883)
            .Build();

        await _client.ConnectAsync(options);
    }

    public async Task SubscribeAsync(string topic)
    {
        await _client.SubscribeAsync(topic);
    }

    public async Task PublishAsync(string topic, string payload)
    {
        var message = new MqttApplicationMessageBuilder()
            .WithTopic(topic)
            .WithPayload(payload)
            .Build();

        await _client.PublishAsync(message);
    }
}