using System.IO.Ports;

public sealed class SerialTransport : IDisposable
{
    private SerialPort _port = new();
    private readonly object _lock = new();

    Boolean TryConnect(string portName)
    {
        TryDisconnect();

        try
        {
            _port = new SerialPort(portName, 9600)
            {
                NewLine = "\n",
                ReadTimeout = 1000,
                DtrEnable = false,
                RtsEnable = false
            };

            _port.Open();
            Thread.Sleep(3000);

            return true;
        }
        catch
        {
            return false;
        }
    }

    Boolean TryHandshake()
    {
        try
        {
            _port.Write(new byte[] { (byte)'X' }, 0, 1);
            string response = _port.ReadLine().Trim();
            if (response == "OK") { return true; }
        }
        catch { }

        TryDisconnect();
        return false;
    }

    void TryDisconnect()
    {
        try { _port.Close(); }
        catch { }
    }

    Boolean EnsureConnected()
    {
        if (_port.IsOpen)
        {
            if (TryHandshake())
            { return true; }
        }

        var portNames = SerialPort.GetPortNames().ToList();
        if (!portNames.Contains("/dev/ttyUSB0")) portNames.Add("/dev/ttyUSB0");

        foreach (var portName in portNames)
        {
            if (TryConnect(portName))
            {
                if (TryHandshake())
                {
                    return true;
                }
            }
        }

        return false;
    }

    public Boolean SendCommand(byte cmd)
    {
        lock (_lock)
        {
            if (!EnsureConnected()) { throw new InvalidOperationException(); }

            _port.Write(new byte[] { cmd }, 0, 1);

            // Read until the NewLine string is encountered
            return _port.ReadLine().Trim() == "OK";
        }
    }

    public string QueryData(byte cmd)
    {
        lock (_lock)
        {
            if (!EnsureConnected()) { throw new InvalidOperationException(); }

            _port.Write(new byte[] { cmd }, 0, 1);

            // Read until the NewLine string is encountered
            return _port.ReadLine().Trim();
        }
    }

    public void Dispose()
    {
        TryDisconnect();
        _port?.Dispose();
    }
}