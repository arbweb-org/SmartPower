namespace SmartPower.Client;

public class FridgeService
{
    private readonly HttpClient _http;

    public FridgeService(HttpClient http)
    {
        _http = http;
    }

    public async Task<Boolean> GetStatus()
    {
        return await _http.GetStringAsync("status") == "Connected";
    }

    public async Task<double?> GetTemperature1()
    {
        try
        {
            var raw = await _http.GetStringAsync("get/T1");
            return double.TryParse(raw, out var val) ? val : null;
        }
        catch { return null; }
    }
}