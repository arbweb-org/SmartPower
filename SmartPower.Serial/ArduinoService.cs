public sealed class ArduinoService : IDisposable
{
    private SerialTransport _transport = new SerialTransport();
    public bool IsConnected => _transport.IsConnected;
    private const string _OK = "OK";
    private const string _ERROR = "Error";

    /// <summary>
    /// Performs handshake. Returns "OK" if device responds to 'X'.
    /// </summary>
    public string CheckConnection()
    {
        return _transport.SendRequest("X") == _OK ? _OK : _ERROR;
    }

    // --- Control Methods ---

    /// <summary>
    /// Sends '0'-'3' to turn relay ON, '4'-'7' to turn relay OFF.
    /// </summary>
    public string SetRelay(int relay, bool on)
    {
        if (!IsConnected || relay < 0 || relay > 3) return _ERROR;
        
        int commandValue = on ? relay : (relay + 4);
        return _transport.SendRequest(commandValue.ToString()) == _OK ? _OK : _ERROR;
    }

    // --- Data Retrieval Methods ---

    /// <summary>
    /// Requests temp from sensor 1 ('8') or 2 ('9').
    /// </summary>
    public string GetTemperature(int sensorId)
    {
        if (!IsConnected) return _ERROR;
        
        string cmd = sensorId == 1 ? "8" : "9";
        string? response = _transport.SendRequest(cmd);
        
        // Returns the temperature string directly if parseable, otherwise ERROR.
        return double.TryParse(response, out _) ? response! : _ERROR;
    }

    /// <summary>
    /// Returns "Voltage|Current0|Current1" from the multimeter.
    /// </summary>
    public string GetRmsData()
    {
        if (!IsConnected) return _ERROR;
        string? response = _transport.SendRequest("R");
        return string.IsNullOrEmpty(response) ? _ERROR : response;
    }

    /// <summary>
    /// Returns pipe-delimited string of all refrigerator parameters.
    /// </summary>
    public string GetParameters()
    {
        if (!IsConnected) return _ERROR;
        string? response = _transport.SendRequest("P");
        return string.IsNullOrEmpty(response) ? _ERROR : response;
    }

    /// <summary>
    /// Returns pipe-delimited string of current calibration values.
    /// </summary>
    public string GetCalibration()
    {
        if (!IsConnected) return _ERROR;
        string? response = _transport.SendRequest("C");
        return string.IsNullOrEmpty(response) ? _ERROR : response;
    }
    // --- Update Methods ---
    // Note: The 'N' suffix acts as a non-numeric terminator for Serial.parseInt().

    public string UpdateTargetTemp(int value) => SendUpdate($"D{value}N");
    public string UpdateDefrostTemp(int value) => SendUpdate($"E{value}N");
    public string UpdateDifferential(int value) => SendUpdate($"F{value}N");
    public string UpdateDelayTime(int value) => SendUpdate($"G{value}N");
    public string UpdateCoolingDuration(int value) => SendUpdate($"H{value}N");
    public string UpdateDefrostDuration(int value) => SendUpdate($"I{value}N");
    
    public string UpdateCurrentCal(float value) => SendUpdate($"A{value:F2}N");
    public string UpdateVoltageCal(float value) => SendUpdate($"V{value:F2}N");

    /// <summary>
    /// Helper to standardize the update response check.
    /// </summary>
    private string SendUpdate(string command)
    {
        if (!IsConnected) return _ERROR;
        return _transport.SendRequest(command) == _OK ? _OK : _ERROR;
    }

    public void Dispose()
    {
        _transport.Dispose();
    }
}