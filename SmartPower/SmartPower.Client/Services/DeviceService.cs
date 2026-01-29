using SmartPower.Client.Models;
using System.Net.WebSockets;
using System.Runtime.InteropServices;
using System.Text;

namespace SmartPower.Client.Services
{
    public class DeviceService : IDisposable
    {
        private const string DEVICE_URL = "ws://192.168.4.1:81";
        private const int HEADER_BYTES = 8;      // 2 ints (cal1, cal2)
        private const int SAMPLE_BYTES = 24000;  // 2000 samples * 12 bytes
        private const int TOTAL_BYTES = HEADER_BYTES + SAMPLE_BYTES; // 24,008 bytes

        private ClientWebSocket _socket;
        private ClientWebSocket _calSocket;
        private CancellationTokenSource _cts = new();
        private bool _isDisposed;

        public event Action<Span<EspSample>, int, int>? OnDataReceived;
        public event Action<string>? OnStatusChanged;

        public int S1CalFactX10000 { get; private set; } = 10000;
        public int S2CalFactX10000 { get; private set; } = 10000;
        public string Status { get; private set; } = "Disconnected";

        public void Start()
        {
            _ = StartConnectionLoop();
        }

        private async Task StartConnectionLoop()
        {
            while (!_cts.IsCancellationRequested)
            {
                try
                {
                    if (await EnsureConnectedAsync())
                    {
                        byte[] receiveBuffer = new byte[TOTAL_BYTES];
                        var bufferSegment = new ArraySegment<byte>(receiveBuffer);

                        while (_socket?.State == WebSocketState.Open && !_cts.IsCancellationRequested)
                        {
                            var startTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

                            var sendBuffer = Encoding.UTF8.GetBytes("get");
                            await _socket.SendAsync(new ArraySegment<byte>(sendBuffer), WebSocketMessageType.Text, true, _cts.Token);

                            int totalRead = 0;
                            while (totalRead < TOTAL_BYTES && !_cts.IsCancellationRequested)
                            {
                                var result = await _socket.ReceiveAsync(bufferSegment.Slice(totalRead, TOTAL_BYTES - totalRead), _cts.Token);
                                if (result.MessageType == WebSocketMessageType.Close) break;
                                totalRead += result.Count;
                            }

                            if (totalRead == TOTAL_BYTES)
                            {
                                ProcessBinaryData(receiveBuffer);
                            }

                            var endTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                            var duration = (int)(endTime - startTime);

                            await Task.Delay(Math.Max(300 - duration, 0), _cts.Token);
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception)
                {
                    if (_cts.IsCancellationRequested) break;
                    UpdateStatus("Retrying...");
                    await CloseConnectionAsync();
                    try { await Task.Delay(300, _cts.Token); } catch { break; }
                }
            }
            UpdateStatus("Disconnected");
        }

        private async Task<bool> EnsureConnectedAsync()
        {
            if (_socket?.State == WebSocketState.Open) return true;

            await CloseConnectionAsync();

            try
            {
                UpdateStatus("Connecting...");
                _socket = new ClientWebSocket();
                _socket.Options.Proxy = null;
                _socket.Options.KeepAliveInterval = TimeSpan.FromSeconds(5);

                using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(_cts.Token, timeoutCts.Token);

                await _socket.ConnectAsync(new Uri(DEVICE_URL), linkedCts.Token);
                UpdateStatus("Connected");
                return true;
            }
            catch (Exception)
            {
                UpdateStatus("Connection Failed");
                await CloseConnectionAsync();
                return false;
            }
        }

        private async Task CloseConnectionAsync()
        {
            if (_socket != null)
            {
                try
                {
                    if (_socket.State == WebSocketState.Open || _socket.State == WebSocketState.CloseReceived)
                    {
                        using var closeCts = new CancellationTokenSource(TimeSpan.FromMilliseconds(500));
                        await _socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", closeCts.Token);
                    }
                }
                catch { }
                finally
                {
                    _socket.Dispose();
                    _socket = null;
                }
            }
        }

        private void UpdateStatus(string status)
        {
            if (_isDisposed) return;
            Status = status;
            OnStatusChanged?.Invoke(status);
        }

        private void ProcessBinaryData(byte[] data)
        {
            if (_isDisposed) return;
            var span = data.AsSpan();

            // Header
            var header = MemoryMarshal.Read<DataHeader>(span);
            S1CalFactX10000 = header.Cal1;
            S2CalFactX10000 = header.Cal2;

            // Samples
            var samplesSpan = span.Slice(HEADER_BYTES);
            var samples = MemoryMarshal.Cast<byte, EspSample>(samplesSpan);

            OnDataReceived?.Invoke(samples.Slice(0, 2000), S1CalFactX10000, S2CalFactX10000);
        }

        public async Task Calibrate(int targetRms, bool isChannel1, int currentRms)
        {
            if (currentRms == 0) currentRms = 1;

            double ratio = (double)targetRms / currentRms;
            int newCalFact = isChannel1
                ? (int)(S1CalFactX10000 * ratio)
                : (int)(S2CalFactX10000 * ratio);

            try
            {

                if (_calSocket == null || _calSocket.State != WebSocketState.None)
                {
                    _calSocket?.Dispose();
                    _calSocket = new ClientWebSocket();
                    _calSocket.Options.Proxy = null;
                }

                if (_calSocket.State == WebSocketState.None)
                {
                    await _calSocket.ConnectAsync(new Uri(DEVICE_URL), _cts.Token);
                }

                if (_calSocket.State == WebSocketState.Open)
                {
                    string cmd = $"cal|{(isChannel1 ? newCalFact : S1CalFactX10000)}|{(isChannel1 ? S2CalFactX10000 : newCalFact)}";
                    var sendBuffer = Encoding.UTF8.GetBytes(cmd);
                    await _calSocket.SendAsync(new ArraySegment<byte>(sendBuffer), WebSocketMessageType.Text, true, _cts.Token);
                    await _calSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Done", _cts.Token);
                    
                    // Update local factors
                    if (isChannel1) S1CalFactX10000 = newCalFact;
                    else S2CalFactX10000 = newCalFact;
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public void Dispose()
        {
            if (_isDisposed) return;
            _isDisposed = true;

            try
            {
                _cts.Cancel();
                // Sockets and CTS will be disposed after the loop exits or by GC
                // Disposing them here immediately while the loop is awaiting can cause ObjectDisposedException
                _socket?.Dispose();
                _calSocket?.Dispose();
                _cts.Dispose();
            }
            catch { }
        }
    }
}