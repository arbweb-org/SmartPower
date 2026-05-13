using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using MQTTnet;

namespace SmartPower.Publisher
{
    public class Worker : BackgroundService
    {
        private readonly ArduinoService _arduino;
        private readonly Refrigerator _fridge = new Refrigerator();
        private IMqttClient _client;

        public Worker(ArduinoService arduino)
        {
            _arduino = arduino;
        }

        void OnSerialError()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                // Process.Start("sudo", "reboot");
            }
        }

        // Runs every about second
        async Task ControlArduino(CancellationToken stoppingToken)
        {
            float? temp1 = _arduino.GetTemp1();
            float? temp2 = _arduino.GetTemp2();
            float? rms1 = _arduino.GetRms1();
            float? rms2 = _arduino.GetRms2();

            if (temp1 is null || temp2 is null || rms1 is null || rms2 is null) { OnSerialError(); return; }
        }

        async Task Publish(CancellationToken stoppingToken)
        {
                var payload = Encoding.UTF8.GetBytes("hello from worker");

                var message = new MqttApplicationMessageBuilder()
                    .WithTopic("smartpower/status")
                    .WithPayload(payload)
                    .Build();

                await _client.PublishAsync(message, stoppingToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var factory = new MqttClientFactory();
            _client = factory.CreateMqttClient();

            var options = new MqttClientOptionsBuilder()
            .WithTcpServer("192.168.1.148", 1883) // MQTT broker IP
            .Build();

            try
            {
                await _client.ConnectAsync(options, stoppingToken);
            }
            catch
            { return; }

            while (!stoppingToken.IsCancellationRequested)
            {
                // await ControlArduino(stoppingToken);
                await Publish(stoppingToken);
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}