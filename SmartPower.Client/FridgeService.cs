namespace SmartPower.Client;

public class FridgeService
{
    private readonly HttpClient _http;

    public FridgeService(HttpClient http)
    {
        _http = http;
    }

    public async Task<bool> GetStatus()
    {
        try
        {
            return await _http.GetStringAsync("status") == "Connected";
        }
        catch { return false; }
    }

    private async Task<double?> GetDouble(string path)
    {
        try
        {
            var raw = await _http.GetStringAsync(path);
            return double.TryParse(raw, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out var val) ? val : null;
        }
        catch { return null; }
    }

    private async Task<bool> Set(string path)
    {
        try
        {
            var res = await _http.GetStringAsync(path);
            return res == "OK";
        }
        catch { return false; }
    }

    // Temperatures
    public Task<double?> GetTemperature1() => GetDouble("get/T1");
    public Task<double?> GetTemperature2() => GetDouble("get/T2");

    // RMS Current
    public Task<double?> GetCurrent() => GetDouble("get/R1");

    // Relay states (0 or 1)
    public Task<double?> GetCompressorState() => GetDouble("get/compressor");
    public Task<double?> GetFanState()         => GetDouble("get/fan");
    public Task<double?> GetDefrostState()     => GetDouble("get/defrost");
    public Task<double?> GetLightState()       => GetDouble("get/light");

    // Set temperature target
    public Task<bool> SetTargetTemperature(double temp) => Set($"set/target/{temp.ToString(System.Globalization.CultureInfo.InvariantCulture)}");

    // WiFi config
    public Task<bool> SetWifiSsid(string ssid)         => Set($"wifi-ssid/{ssid}");
    public Task<bool> SetWifiPassword(string password)  => Set($"wifi-password/{password}");
}