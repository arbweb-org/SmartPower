# SmartPower

Real-time power monitoring and waveform visualization for IoT using ESP32 and .NET MAUI Blazor.

## Overview

SmartPower is an IoT solution for real-time electrical power monitoring. It consists of two main components:

1. **ESP32 Firmware** (`smart_power/`) - Arduino-based firmware that samples analog sensors at high frequency and streams data via WebSocket.
2. **MAUI Blazor Client** (`SmartPower/`) - Cross-platform desktop/mobile app for real-time visualization of power waveforms.

## Architecture

```
┌─────────────────┐    WebSocket (Port 81)    ┌──────────────────────┐
│   ESP32 Board   │ ─────────────────────────▶│  MAUI Blazor Client  │
│  (smart_power)  │      Binary Data          │  (SmartPower.Client) │
└─────────────────┘                           └──────────────────────┘
        │
        │ Analog Read
        ▼
┌─────────────────┐
│  Power Sensors  │
│  (GPIO 34 & 35) │
└─────────────────┘
```

## ESP32 Firmware

### Features

- **High-Frequency Sampling**: 200 microseconds per sample (5 kHz sampling rate)
- **Dual-Channel Input**: Reads from two analog sensors (GPIO 34 & 35)
- **Circular Buffer**: Stores 2,500 samples in memory (~0.5 seconds of data)
- **WebSocket Server**: Streams binary data on demand via port 81
- **Static IP**: Configured for `192.168.100.33`
- **Calibration Storage**: Two persistent calibration factors stored in flash memory

### Files

| File | Description |
|------|-------------|
| `smart_power.ino` | Main entry point, initializes sensors and WiFi |
| `smartCommon.h` | Shared data structures and buffer definitions |
| `smartSensor.h` | Timer interrupt-based ADC sampling |
| `smartServer.h` | WiFi and WebSocket server configuration |
| `smartCalibration.h` | Calibration factor storage using ESP32 Preferences |

### Data Format

Each sample is a 12-byte packed struct:
- `time` (4 bytes): Microsecond timestamp
- `s1` (4 bytes): Sensor 1 reading (0-4095)
- `s2` (4 bytes): Sensor 2 reading (0-4095)

Total transmission per request: **30,000 bytes** (2,500 samples)

## MAUI Blazor Client

### Features

- **Real-Time Visualization**: SVG-based waveform rendering with live updates
- **Cross-Platform**: Runs on Windows, Android, iOS, and macOS
- **Auto-Reconnect**: Automatically reconnects to the ESP32 on connection loss
- **1-Second Rolling Window**: Displays the most recent 1 second of data

### Technology Stack

- .NET MAUI with Blazor Hybrid
- WebSocket client for binary data streaming
- SVG polyline rendering for waveforms

### Platforms

| Platform | Min Version |
|----------|-------------|
| Android | 24.0 |
| iOS | 15.0 |
| macOS Catalyst | 15.0 |
| Windows | 10.0.17763.0 |

## Getting Started

### ESP32 Setup

1. Open `smart_power/smart_power.ino` in Arduino IDE
2. Update WiFi credentials in `smartServer.h`:
   ```cpp
   const char* ssid = "YOUR_SSID";
   const char* password = "YOUR_PASSWORD";
   ```
3. Adjust static IP if needed:
   ```cpp
   IPAddress local_IP(192, 168, 100, 33);
   ```
4. Upload to your ESP32 board

### Client Setup

1. Open `SmartPower/SmartPower.slnx` in Visual Studio
2. Build and run the `SmartPower.Client` project
3. Ensure your device is on the same network as the ESP32

## Network Protocol

The client communicates with the ESP32 using WebSocket text commands:

| Command | Response | Description |
|---------|----------|-------------|
| `get` | 30,000 bytes binary | Returns 2,500 samples of sensor data |
| `getcal` | `cal:X.XXXXXX,Y.YYYYYY` | Returns current calibration factors |
| `setcal:X,Y` | `ok` or `err` | Sets and persists calibration factors |

### Data Streaming Flow

1. **Client → ESP32**: Send `"get"` text message
2. **ESP32 → Client**: Respond with 30,000 bytes of binary sample data
3. Client processes data and repeats after 300ms delay

## License

MIT License
