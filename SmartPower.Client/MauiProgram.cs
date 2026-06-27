using Microsoft.Extensions.Logging;

namespace SmartPower.Client;

public static class MauiProgram
{
        public static MauiApp CreateMauiApp()
        {
                var builder = MauiApp.CreateBuilder();
                builder.UseMauiApp<App>();

                builder.Services.AddMauiBlazorWebView();

#if DEBUG
                var baseUrl = "http://127.0.0.1:5000/";
                builder.Services.AddBlazorWebViewDeveloperTools();
                builder.Logging.AddDebug();
#else
                var baseUrl = "http://192.168.4.1:5000/";
#endif

                builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(baseUrl) });
                builder.Services.AddScoped<FridgeService>();

                return builder.Build();
        }
}