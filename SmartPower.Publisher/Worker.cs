namespace SmartPower.Publisher
{
    public class Worker : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                // Pull serial data from the serial port
                // Publish it to the MQTT broker

                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}