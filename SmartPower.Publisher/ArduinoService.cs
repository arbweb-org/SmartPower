public sealed class ArduinoService
{
    private SerialTransport _transport = new SerialTransport();

    public Boolean TurnRelayOnOff(int relayIndex, bool OnOff)
    {
        int onOff = OnOff ? 0 : 4;
        byte command = (byte)(onOff + relayIndex + ((byte)'0'));
        return _transport.SendCommand(command);
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