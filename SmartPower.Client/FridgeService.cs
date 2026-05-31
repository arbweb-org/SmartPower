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
}