using RJCP.IO.Ports;

public sealed class SerialTransport
{
    private readonly SerialPortStream _port;

    public SerialTransport(SerialPortStream port)
    {
        _port = port;
    }

    public string Send(byte cmd)
    {
        _port.Write(new byte[] { cmd }, 0, 1);
        return _port.ReadLine();
    }
}