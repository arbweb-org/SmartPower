namespace SmartPower.Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // run as Windows Service when on Windows
            builder.Host.UseWindowsService();

            // listen on all interfaces
            builder.WebHost.UseUrls("http://0.0.0.0:5000");

            // services
            builder.Services.AddSingleton<WebServer>();

            var app = builder.Build();

            // HTTP API
            app.MapGet("/temp1", (WebServer svc) => svc.GetTemp1());
            app.MapGet("/temp2", (WebServer svc) => svc.GetTemp2());
            app.MapGet("/rms1", (WebServer svc) => svc.GetRms1());
            app.MapGet("/rms2", (WebServer svc) => svc.GetRms2());

            app.MapGet("/relay/{i}/on", (int i, WebServer svc) =>
            {
                if (svc.TurnRelayOnOff(i, true))
                {
                    return Results.Ok();
                }
                else
                {
                    return Results.StatusCode(500);
                }
            });

            app.MapGet("/relay/{i}/off", (int i, WebServer svc) =>
            {
                if (svc.TurnRelayOnOff(i, false))
                {
                    return Results.Ok();
                }
                else
                {
                    return Results.StatusCode(500);
                }
            });

            app.Run();
        }
    }
}