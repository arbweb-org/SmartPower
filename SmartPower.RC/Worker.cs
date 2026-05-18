using InTheHand.Net.Sockets;
using System.Text;

namespace SmartPower.RC;

public class Worker(ILogger<Worker> logger) : BackgroundService
{
    private static readonly Guid serviceId = new Guid("00001101-0000-1000-8000-00805f9b34fb");
    BluetoothListener listener = new BluetoothListener(serviceId)
    {
        ServiceName = "Net10BluetoothHelloWorld"
    };

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        listener.Start();

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                BluetoothClient client = await listener.AcceptBluetoothClientAsync();
                await HandleClientSessionAsync(client, stoppingToken);
            }
        }
        catch { }
        finally
        {
            DisposeListener();
        }
    }

    private async Task HandleClientSessionAsync(BluetoothClient client, CancellationToken token)
    {
        using var stream = client.GetStream();
        byte[] buffer = new byte[1024];
        int count = stream.Read(buffer, 0, buffer.Length);
        string text = Encoding.UTF8.GetString(buffer, 0, count);
        string response = text == "hello" ? "world" : "unknown";

        byte[] responseBytes = Encoding.UTF8.GetBytes(response);
        stream.Write(responseBytes, 0, responseBytes.Length);
    }

    private void DisposeListener()
    {
        if (listener != null)
        {
            listener.Stop();
            listener = null;
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        DisposeListener();
        await base.StopAsync(cancellationToken);
    }
}