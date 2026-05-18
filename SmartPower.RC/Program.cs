namespace SmartPower.RC;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);
        
        builder.Services.AddSystemd();
        builder.Services.AddHostedService<Worker>();

        var host = builder.Build();
        host.Run();
    }
}