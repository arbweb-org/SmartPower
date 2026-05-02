public sealed class ArduinoService
{
    private SerialTransport _transport = new SerialTransport();

    public string Send(byte cmd)
    {
        return _transport.QueryData(cmd); // send + read line
    }

    public string GetStatus()
    {
        return Send("GET");
    }

    public bool SetParam(string key, float value)
    {
        var res = Send($"SET {key} {value}");
        return res == "OK";
    }

    public float? GetTemp1()
    {
        string? response = _transport.QueryData((byte)'8');
        return float.TryParse(response, out float result) ? result : null;
    }

    public float? GetTemp2()
    {
        string? response = _transport.QueryData((byte)'9');
        return float.TryParse(response, out float result) ? result : null;
    }

    public float? GetRms1()
    {
        string? response = _transport.QueryData((byte)'B');
        return float.TryParse(response, out float result) ? result : null;
    }

    public float? GetRms2()
    {
        string? response = _transport.QueryData((byte)'A');
        return float.TryParse(response, out float result) ? result : null;
    }
}