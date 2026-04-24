# SmartPower Architecture Overview

The **SmartPower** project is an IoT-based power management and monitoring system. The solution is structured into distinct layers, separating hardware control, background processing, API exposure, and user interface. 

## Project Structure

The repository consists of four primary projects:

1. **SmartPower.IO** (Hardware Firmware)
2. **SmartPower.Publisher** (Hardware Communication & Worker Service)
3. **SmartPower.Server** (HTTP API Server)
4. **SmartPower.Test** (.NET MAUI Blazor Hybrid Application)

---

### 1. SmartPower.IO (Hardware Layer)
This is an Arduino sketch (`SmartPower.IO.ino`) that directly interfaces with the physical hardware components.

- **Hardware Monitored/Controlled:**
  - **4 Relays** (Pins 3, 2, 5, 4).
  - **2 Temperature Sensors** (Dallas Temperature on 1-Wire pins 7 and 8).
  - **2 Analog Inputs** (A1, A3) for True RMS voltage/current calculations.
- **Communication Protocol:** 
  It uses a simple 9600 baud Serial protocol where single-character commands trigger actions:
  - `'X'`: Handshake (Returns "OK")
  - `'0'` to `'3'`: Turn Relay 0-3 **ON** (Returns "OK")
  - `'4'` to `'7'`: Turn Relay 0-3 **OFF** (Returns "OK")
  - `'8'`, `'9'`: Request Temperature 1 & 2 respectively
  - `'A'`, `'B'`: Request RMS values from Analog Pin 1 & 2 respectively

### 2. SmartPower.Publisher (Service & Communication Layer)
A .NET Worker Service responsible for maintaining a reliable connection to the Arduino and abstracting the serial protocol.

- **`SerialTransport.cs`**: 
  Manages the serial port connection using `RJCP.IO.Ports`. It features auto-discovery (looping through available COM ports), automatic connection recovery, thread-safe command dispatch (`SendCommand`, `QueryData`), and an `'X'` handshake validation.
- **`ArduinoService.cs`**: 
  A high-level wrapper around `SerialTransport`. It exposes clean C# methods (e.g., `TurnRelayOnOff`, `GetTemp1`, `GetRms1`) that abstract away the byte-level serial commands.
- **`Worker.cs`**: 
  A `BackgroundService` that runs continuously. It currently serves as a placeholder to routinely pull serial data from the Arduino and publish it to an MQTT broker.

### 3. SmartPower.Server (API Layer)
A lightweight .NET Web API application designed to expose the hardware's capabilities over the network.

- **Features**: Listens on port 5000 (`http://0.0.0.0:5000`) and can be run as a Windows Service.
- **Endpoints**:
  - `/temp1`, `/temp2`
  - `/rms1`, `/rms2`
  - `/relay/{i}/on`, `/relay/{i}/off`
- **Current State**: The underlying `WebServer.cs` service currently contains mocked implementations returning placeholder values (`0` or `true`). It acts as the foundational HTTP bridge, awaiting integration with `ArduinoService` or an MQTT client.

### 4. SmartPower.Test (UI & Testing Layer)
A cross-platform .NET MAUI application utilizing Blazor Hybrid (`BlazorWebView`) to provide a modern web-based user interface.

- **Purpose**: Serves as a frontend interface to test relay toggles and view telemetry data (temperatures and RMS values).
- **`PiService.cs`**: 
  Acts as the client-side data provider for the Blazor UI. Similar to the Web API, it currently contains mock methods. Future development will likely wire this service to make HTTP requests to `SmartPower.Server` or subscribe to MQTT topics.

## System Workflow

1. The **Arduino (SmartPower.IO)** physically controls the relays and reads sensor data.
2. The **Publisher Service** maintains a continuous serial connection to the Arduino, fetching data and sending commands securely via `ArduinoService.cs`.
3. The **Server** exposes an HTTP interface to allow external applications to request data or trigger relays remotely.
4. The **Test App (MAUI Blazor)** provides a graphical interface for end-users to interact with the entire system seamlessly.

> [!NOTE]
> The `Server` and `Test` projects currently rely on mock implementations (`WebServer.cs` and `PiService.cs`). The next logical integration step is bridging these services with the functional `ArduinoService.cs` or an MQTT broker to achieve end-to-end communication.
