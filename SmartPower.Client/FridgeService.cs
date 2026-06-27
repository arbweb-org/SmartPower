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
        try { return await _http.GetStringAsync("status") == "Connected"; }
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

    private async Task<string?> GetString(string path)
    {
        try { return await _http.GetStringAsync(path); }
        catch { return null; }
    }

    private async Task<bool> Send(string path)
    {
        try { return await _http.GetStringAsync(path) == "OK"; }
        catch { return false; }
    }

    // Temperatures
    public Task<double?> GetTemperature1() => GetDouble("get/T1");
    public Task<double?> GetTemperature2() => GetDouble("get/T2");

    // RMS → "Voltage|Current0|Current1"
    public Task<string?> GetRmsData() => GetString("get/W");

    // Parameters → pipe-delimited
    public Task<string?> GetParameters() => GetString("get/P");

    // Calibration → pipe-delimited
    public Task<string?> GetCalibration() => GetString("get/C");

    // Relay
    public Task<bool> SetRelay(int relay, bool on) =>
        Send($"get/{(on ? "ON" : "OFF")}/{relay}");

    // Parameter updates
    public Task<bool> SetTargetTemp(int value)      => Send($"get/D/{value}");
    public Task<bool> SetDefrostTemp(int value)     => Send($"get/E/{value}");
    public Task<bool> SetDifferential(int value)    => Send($"get/F/{value}");
    public Task<bool> SetDelayTime(int value)       => Send($"get/G/{value}");
    public Task<bool> SetCoolingDuration(int value) => Send($"get/H/{value}");
    public Task<bool> SetDefrostDuration(int value) => Send($"get/I/{value}");

    // Calibration updates
    public Task<bool> SetCurrentCal(float value) =>
        Send($"get/A/{value.ToString("F2", System.Globalization.CultureInfo.InvariantCulture)}");
    public Task<bool> SetVoltageCal(float value) =>
        Send($"get/V/{value.ToString("F2", System.Globalization.CultureInfo.InvariantCulture)}");

    // WiFi (handled by RC directly, not forwarded to Serial)
    public Task<bool> SetWifiSsid(string ssid)   => Send($"wifi-ssid/{ssid}");
    public Task<bool> SetWifiPassword(string pass) => Send($"wifi-password/{pass}");
}