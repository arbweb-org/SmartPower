using System.IO.Ports;
using System.Text;

public sealed class SerialTransport : IDisposable
{
    public bool IsConnected {get; set;} = false;
    private readonly object _serialLock = new object();
    private SerialPort _port = new()
    {
        NewLine = "\n",
        ReadTimeout = 100,
        DtrEnable = false,
        RtsEnable = false
    };

    public SerialTransport()
    {
        TryConnect();
    }

    Boolean TryHandshake()
    {
        for (int i = 0; i < 40; i++)
        {
            try
            {
                _port.Write(new byte[] { (byte)'X' }, 0, 1);
                string response = _port.ReadLine().Trim();
                if (response == "OK")
                {
                    return true;
                }
            }
            catch { }
        }

        return false;
    }

    Boolean TestPort(string portName)
    {
        try
        {
            if (_port.IsOpen) 
            {
                _port.Close();
            }
            _port.PortName = portName;

            _port.Open();
            _port.DiscardInBuffer();

            if(TryHandshake())
            {
                return true;
            }
        }
        catch { }

        TryDisconnect();
        return false;
    }

    Boolean TryDisconnect()
    {
        try
        {
            if (_port.IsOpen) 
            { 
                _port.Close();
            }
            
            return true;
        }
        catch { }
        return false;
    }

    Boolean TryConnect()
    {
        if (_port.IsOpen) 
        {
            return true; 
        }

        IsConnected = false;
        
        if(!TryDisconnect())
        {
            return false; 
        }

        var portNames = SerialPort.GetPortNames().ToList();
        if (!portNames.Contains("/dev/ttyUSB0")) portNames.Add("/dev/ttyUSB0");

        foreach (var portName in portNames)
        {
            if (TestPort(portName))
            {
                IsConnected = true;
                return true;
            }

            TryDisconnect();
        }

        return false;
    }

    public string? SendRequest(String cmd)
    {
        lock (_serialLock)
        {
            try
            {
                byte[] cmdBytes = Encoding.UTF8.GetBytes(cmd);
                _port.Write(cmdBytes, 0, cmdBytes.Length);
                // Read until the NewLine string is encountered
                return _port.ReadLine().Trim();
            }
            catch
            {
                IsConnected = false;
                return null;
            }
        }
    }

    public void Dispose()
    {
        TryDisconnect();
        _port?.Dispose();
    }
}