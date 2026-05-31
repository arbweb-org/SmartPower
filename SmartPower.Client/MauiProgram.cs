using Microsoft.Extensions.Logging;

namespace SmartPower.Client;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder.UseMauiApp<App>();

		builder.Services.AddMauiBlazorWebView();
		builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("http://192.168.4.1:5000/") });
		builder.Services.AddScoped<FridgeService>();

#if DEBUG
		builder.Services.AddBlazorWebViewDeveloperTools();
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}