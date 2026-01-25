using Microsoft.AspNetCore.Components;
using SmartPower.Client.Models;
using System.Net.WebSockets;
using System.Runtime.InteropServices;
using System.Text;

namespace SmartPower.Client.Components.Pages;

public partial class Home : ComponentBase, IDisposable
{
    private const string DEVICE_URL = "ws://192.168.100.33:81";
    private const int HEADER_BYTES = 8;      // 2 floats (cal1, cal2)
    private const int SAMPLE_BYTES = 24000;  // 2000 samples * 12 bytes
    private const int TOTAL_BYTES = HEADER_BYTES + SAMPLE_BYTES; // 24,008 bytes

    private const int WINDOW_MS = 1000; // 1 Second window
    private uint lastMicros = 0;
    private CircularBuffer _masterBuffer = new(5000); // Store last 5000 samples (1 seconds at 50Hz)
    private CircularBuffer _rms = new(25); // Store last 25 RMS values (1 second, 2 cycles each)

    protected string _pointsCrn = "";
    protected string _pointsRms = "";
    protected string _status = "Disconnected";

    // Calibration factors (received from ESP32)
    protected float _calFactor1 = 1.0f;
    protected float _calFactor2 = 1.0f;

    private ClientWebSocket _socket;
    private CancellationTokenSource _cts = new();

    protected override void OnInitialized() => _ = StartConnectionLoop();

    private async Task StartConnectionLoop()
    {
        // Retrying connection loop
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

                // 300ms interval request loop
                while (_socket.State == WebSocketState.Open && !_cts.IsCancellationRequested)
                {
                    // 1. WRITE: Request data (Send "get" as bytes)
                    var sendBuffer = Encoding.UTF8.GetBytes("get");
                    await _socket.SendAsync(new ArraySegment<byte>(sendBuffer), WebSocketMessageType.Text, true, _cts.Token);

                    // 2. READ: Fill the buffer until we have exactly 30,008 bytes
                    int totalRead = 0;

                    while (totalRead < TOTAL_BYTES)
                    {
                        var result = await _socket.ReceiveAsync(bufferSegment.Slice(totalRead, TOTAL_BYTES - totalRead), _cts.Token);

                        if (result.MessageType == WebSocketMessageType.Close) break;

                        totalRead += result.Count;
                    }

                    if (totalRead == TOTAL_BYTES)
                    {
                        // Set calibration factors and append samples
                        ProcessBinaryData(receiveBuffer);

                        // Draw curves
                        DrawCurves();
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
                await Task.Delay(300);
            }
        }
    }

    private void ProcessBinaryData(byte[] data)
    {
        var span = data.AsSpan();

        // Header
        var header = MemoryMarshal.Read<DataHeader>(span);
        _calFactor1 = header.Cal1;
        _calFactor2 = header.Cal2;

        // Samples
        var samplesSpan = span.Slice(HEADER_BYTES);
        var samples = MemoryMarshal.Cast<byte, EspSample>(samplesSpan);

        // Append samples to master buffer
        PrepareSamples(samples.Slice(0, 2000));
    }
    
    private void PrepareSamples(Span<EspSample> samples)
    {
        // 1. Sort by time with wrap-around handling
        int pivot = -1;

        // Find wrap point
        for (int i = 0; i < samples.Length - 1; i++)
        {
            if (samples[i].Time > samples[i + 1].Time)
            {
                pivot = i + 1;
                break;
            }
        }

        Span<EspSample> sortedSamples = samples;

        // No wrap → already ordered
        if (pivot == -1)
        {
            sortedSamples = samples;
        }
        // Wrap detected
        else
        {
            EspSample[] result = new EspSample[samples.Length];
            Span<EspSample> combined = result;

            var oldSpan = samples.Slice(pivot);
            var newSpan = samples.Slice(0, pivot);

            oldSpan.CopyTo(combined);
            newSpan.CopyTo(combined.Slice(oldSpan.Length));

            sortedSamples = combined;
        }

        // 2. Remove repeat samples based on time
        int index = -1;
        for (int i = 0; i < sortedSamples.Length; i++)
        {
            if (sortedSamples[i].Time > lastMicros)
            {
                index = i;
                break;
            }
        }

        if (index == -1)
        {
            return; // All samples are repeats
        }
        Span<EspSample> uniqueSamples = sortedSamples.Slice(index);

        // 3. Get max sub-span with count = integer multiple of 200
        int validCount = uniqueSamples.Length - (uniqueSamples.Length % 200);
        if (validCount <= 200)
        {
            return; // Not enough unique samples
        }

        var maxCycles = uniqueSamples.Slice(0, validCount);
        lastMicros = maxCycles[^1].Time;

        ProcessSamples(maxCycles);
    }

    private void ProcessSamples(Span<EspSample> samples)
    {
        // Process in chunks of 200 samples (two cycles at 50Hz)
        for (int i = 0; i < samples.Length; i += 200)
        {
            var chunk = samples.Slice(i, 200);
            ProcessChunck(chunk);
        }
    }

    private void ProcessChunck(Span<EspSample> chunk)
    {
        // Extract samples for both channels
        double[] samplesS1 = new double[200];
        double[] samplesS2 = new double[200];

        double sumS1 = 0;
        double sumS2 = 0;

        // Apply calibration factors
        for (int i = 0; i < 200; i++)
        {
            samplesS1[i] = chunk[i].S1 * _calFactor1;
            samplesS2[i] = chunk[i].S2 * _calFactor2;

            sumS1 += samplesS1[i];
            sumS2 += samplesS2[i];
        }

        // Calculate mean for both channels
        double meanS1 = sumS1 / 200;
        double meanS2 = sumS2 / 200;

        double sumSqS1 = 0;
        double sumSqS2 = 0;

        // Remove offset
        for (int i = 0; i < 200; i++)
        {
            samplesS1[i] = samplesS1[i] - meanS1;
            samplesS2[i] = samplesS2[i] - meanS2;

            sumSqS1 += samplesS1[i] * samplesS1[i];
            sumSqS2 += samplesS2[i] * samplesS2[i];
        }

        double rmsS1 = Math.Sqrt(sumSqS1 / 200);
        double rmsS2 = Math.Sqrt(sumSqS2 / 200);

        if (rmsS1 < 1000)
        {
            // Use S1
            SaveChunck(samplesS1, rmsS1);
        }
        else
        {
            // Use S2
            SaveChunck(samplesS2, rmsS2);
        }
    }

    private void SaveChunck(double[] readings, double rms)
    {
        foreach (var reading in readings)
        {
            _masterBuffer.Enqueue((int)reading);
        }

        _rms.Enqueue((int)rms);
    }

    private void DrawCurves()
    {
        var crn = new StringBuilder();
        var rms = new StringBuilder();
        float scaleY = 240f / 60000f; // 240 pixels for 60,000mA (-30A to 30A)

        for (int i = 0; i < _masterBuffer.Count; i++)
        {
            double x = i;
            double y = 120 - (_masterBuffer[i] * scaleY);

            crn.Append($"{x},{y} ");
        }

        for (int i = 0; i < _rms.Count; i++)
        {
            double x = i * 200;
            double y = 120 - (_rms[i] * scaleY);
            rms.Append($"{x},{y} ");
        }

        _pointsCrn = crn.ToString();
        _pointsRms = rms.ToString();
    }

    public void Dispose()
    {
        _cts.Cancel();
        _socket?.Dispose();
    }
}