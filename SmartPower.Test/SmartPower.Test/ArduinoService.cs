public sealed class ArduinoService
{
    private readonly SerialTransport _transport;

    public ArduinoService(SerialTransport transport)
    {
        _transport = transport;
    }

    public float GetTemp1()
    {
        return float.Parse(_transport.Send((byte)'8'));
    }

    public float GetRmsA1()
    {
        return float.Parse(_transport.Send((byte)'A'));
    }
}