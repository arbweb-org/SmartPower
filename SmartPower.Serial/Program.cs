namespace SmartPower.Serial
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);
            builder.Services.AddSingleton<ArduinoService>();
            builder.Services.AddSystemd();        // Handles Linux systemd signals
            builder.Services.AddHostedService<Worker>();

            var host = builder.Build();
            host.Run();
        }
    }
}