using Microsoft.AspNetCore.Components;
using System.Net.WebSockets;
using System.Runtime.InteropServices;
using System.Text;

namespace SmartPower.Client.Components.Pages;

public partial class Home : ComponentBase, IDisposable
{
    private const string DEVICE_URL = "ws://192.168.100.33:81";
    private const int TOTAL_BYTES = 30000; // 2500 samples * 12 bytes

    private const int WINDOW_MS = 1000; // 1 Second window
    private List<EspSample> _masterBuffer = new();

    protected string _pointsS1 = "";
    protected string _pointsS2 = "";
    protected string _status = "Disconnected";

    private ClientWebSocket _socket;
    private CancellationTokenSource _cts = new();

    protected override void OnInitialized() => _ = StartConnectionLoop();

    private async Task StartConnectionLoop()
    {
        while (!_cts.IsCancellationRequested)
        {
            try
            {
                _socket = new ClientWebSocket();
                _socket.Options.Proxy = null;

                await _socket.ConnectAsync(new Uri(DEVICE_URL), _cts.Token);
                _status = "Connected";
                await InvokeAsync(StateHasChanged);

                byte[] receiveBuffer = new byte[TOTAL_BYTES];
                var bufferSegment = new ArraySegment<byte>(receiveBuffer);

                while (_socket.State == WebSocketState.Open && !_cts.IsCancellationRequested)
                {
                    // 1. WRITE: Request data (Send "get" as bytes)
                    var sendBuffer = Encoding.UTF8.GetBytes("get\0");
                    await _socket.SendAsync(new ArraySegment<byte>(sendBuffer), WebSocketMessageType.Text, true, _cts.Token);

                    // 2. READ: Fill the buffer until we have exactly 30,000 bytes
                    int totalRead = 0;

                    while (totalRead < TOTAL_BYTES)
                    {
                        var result = await _socket.ReceiveAsync(bufferSegment.Slice(totalRead, TOTAL_BYTES - totalRead), _cts.Token);

                        if (result.MessageType == WebSocketMessageType.Close) break;

                        totalRead += result.Count;
                    }

                    if (totalRead == TOTAL_BYTES)
                    {
                        ProcessBinaryData(receiveBuffer);
                        await InvokeAsync(StateHasChanged);
                    }

                    await Task.Delay(300, _cts.Token);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Stream Error: {ex.Message}");
                _status = "Retrying...";
                await InvokeAsync(StateHasChanged);
                await Task.Delay(3000);
            }
        }
    }

    private void ProcessBinaryData(byte[] data)
    {
        GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
        try
        {
            IntPtr ptr = handle.AddrOfPinnedObject();
            var incomingBatch = new List<EspSample>();

            for (int i = 0; i < 2500; i++)
            {
                incomingBatch.Add(Marshal.PtrToStructure<EspSample>(IntPtr.Add(ptr, i * 12)));
            }

            UpdateMasterBuffer(incomingBatch);
        }
        finally { handle.Free(); }
    }

    private void UpdateMasterBuffer(List<EspSample> newSamples)
    {
        // 1. Add only unique samples (Purge duplicates based on Time)
        var existingTimes = _masterBuffer.Select(x => x.Time).ToHashSet();
        foreach (var s in newSamples)
        {
            if (!existingTimes.Contains(s.Time))
            {
                _masterBuffer.Add(s);
            }
        }

        // 2. Sort by Time
        _masterBuffer = _masterBuffer.OrderBy(x => x.Time).ToList();

        // 3. Purge samples older than 1 second
        if (_masterBuffer.Count > 0)
        {
            uint newestTime = _masterBuffer.Last().Time;
            // Note: If ESP32 Time is in microseconds, 1s = 1,000,000 units
            // Assuming your Time unit is microseconds (200us interval)
            uint threshold = newestTime - 1000000;
            _masterBuffer.RemoveAll(x => x.Time < threshold);
        }

        // 4. Update the Polyline (Snapshot the 1s window)
        GeneratePolyline();
    }

    private void GeneratePolyline()
    {
        var sb1 = new StringBuilder();
        var sb2 = new StringBuilder();

        // We scale the X-axis based on the count in the buffer (0 to 5000 approx)
        for (int i = 0; i < _masterBuffer.Count; i++)
        {
            sb1.Append($"{i},{(4096 - _masterBuffer[i].S1)} ");
            sb2.Append($"{i},{(4096 - _masterBuffer[i].S2)} ");
        }

        _pointsS1 = sb1.ToString();
        _pointsS2 = sb2.ToString();
    }

    public void Dispose()
    {
        _cts.Cancel();
        _socket?.Dispose();
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct EspSample
    {
        public uint Time;
        public int S1;
        public int S2;
    }
}