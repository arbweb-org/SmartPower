using System.IO.Ports;

public sealed class SerialTransport : IDisposable
{
    public bool IsConnected {get; set;} = false;

    public SerialTransport()
    {
        TryConnect();
    }

    private SerialPort _port = new()
    {
        NewLine = "\n",
        ReadTimeout = 100,
        DtrEnable = false,
        RtsEnable = false
    };

    Boolean TryHandshake()
    {
        try
        {
            for (int i = 0; i < 40; i++)
            {
                _port.Write(new byte[] { (byte)'X' }, 0, 1);
                string response = _port.ReadLine().Trim();
                if (response == "OK")
                {
                    return true;
                }              
            }
        }
        catch { }

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

    public Boolean SendCommand(byte cmd)
    {
        try
        {
            _port.Write(new byte[] { cmd }, 0, 1);
            // Read until the NewLine string is encountered
            return _port.ReadLine().Trim() == "OK";
        }
        catch
        {
            return false;
        }
    }

    public string? QueryData(byte cmd)
    {
        try
        {
            _port.Write(new byte[] { cmd }, 0, 1);

            // Read until the NewLine string is encountered
            return _port.ReadLine().Trim();
        }
        catch
        {
            return null;
        }
    }

    public void Dispose()
    {
        TryDisconnect();
        _port?.Dispose();
    }
}