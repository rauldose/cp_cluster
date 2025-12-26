# CyberPandino Cluster - .NET Port

This is a .NET port of the PandaOS Cluster project, originally built with Node.js/React/Electron.

## 🎯 Architecture

The .NET version consists of three main projects:

```
CyberPandinoCluster.sln
├── Server/          → ASP.NET Core Web API with SignalR (Backend)
├── Client/          → Blazor WebAssembly (Frontend)
└── Shared/          → Shared Models and DTOs
```

## 🛠️ Technology Stack

- **Server**: ASP.NET Core 10.0, SignalR, System.IO.Ports, System.Device.Gpio
- **Client**: Blazor WebAssembly 10.0, SignalR Client
- **Shared**: .NET Standard Library for shared models

## ⚙️ Prerequisites

- **.NET SDK 10.0** or later
- **Raspberry Pi 4B/5** (for production with hardware)
- **ELM327 USB adapter** (for OBD-II communication)
- **GPIO-connected optocouplers** (for vehicle warning lights)

For development on non-Raspberry Pi systems:
- The server will run in limited mode without hardware access
- The client can be developed independently

## 🚀 Getting Started

### 1. Clone and Build

```bash
git clone https://github.com/cyberpandino/cluster
cd cluster
dotnet build CyberPandinoCluster.sln
```

### 2. Run the Server

```bash
cd dotnet/Server
dotnet run
```

The server will start on `http://localhost:5086` by default.

**Note**: On non-Raspberry Pi systems, the server will run in limited mode without OBD-II and GPIO functionality.

### 3. Run the Client

```bash
cd dotnet/Client
dotnet run
```

The client will be available at `http://localhost:5290` (or the port shown in the console).

## 📡 SignalR Communication

The server and client communicate via SignalR over WebSockets:

- **Hub endpoint**: `/clusterhub`
- **Events**:
  - `obd-data`: OBD-II data (speed, RPM, temperature, etc.)
  - `gpio-warnings`: Vehicle warning lights status
  - `external-temperature`: External temperature sensor data
  - `fuel-level`: Fuel level sensor data

## 🔌 Hardware Configuration

### OBD-II Connection

The server connects to the ELM327 adapter via serial port:
- **Default port**: `/dev/ttyUSB0`
- **Baud rate**: 38400

Modify in `Services/OBDCommunicationService.cs` if needed.

### GPIO Pin Mapping

GPIO pins are configured in `Services/GPIOService.cs`:

| Warning Light | GPIO Pin (BCM) |
|--------------|----------------|
| Turn Signal | 17 |
| Alternator | 27 |
| Oil Pressure | 22 |
| Brake System | 23 |
| Injectors | 24 |
| Key On | 25 |
| High Beam | 5 |
| Low Beam | 6 |
| Hazard Lights | 12 |
| Fog Light | 13 |
| Coolant Temperature | 16 |
| Rear Defrost | 19 |
| Fuel Reserve | 20 |
| Ignition | 21 |

## 🎨 Client Development

The Blazor WebAssembly client provides a modern dashboard for the cluster.

### Key Components

- **Services/ClusterDataService.cs**: SignalR client service for real-time data
- **Pages/**: Blazor pages for the UI

### Customization

Edit the client pages in `dotnet/Client/Pages/` to customize the dashboard appearance and functionality.

## 🏗️ Deployment

### Raspberry Pi Deployment

1. Publish the server:
```bash
cd dotnet/Server
dotnet publish -c Release -r linux-arm64 --self-contained
```

2. Publish the client:
```bash
cd dotnet/Client
dotnet publish -c Release
```

3. Copy the published files to your Raspberry Pi
4. Run the server as a systemd service (see below)

### Systemd Service

Create `/etc/systemd/system/cluster-server.service`:

```ini
[Unit]
Description=CyberPandino Cluster Server
After=network.target

[Service]
WorkingDirectory=/home/pi/cluster/dotnet/Server/bin/Release/net10.0/linux-arm64/publish
ExecStart=/home/pi/cluster/dotnet/Server/bin/Release/net10.0/linux-arm64/publish/CyberPandinoCluster.Server
Restart=always
RestartSec=10
User=pi
Environment=ASPNETCORE_ENVIRONMENT=Production

[Install]
WantedBy=multi-user.target
```

Enable and start:
```bash
sudo systemctl enable cluster-server
sudo systemctl start cluster-server
```

## 🔍 Differences from Node.js Version

### Advantages of .NET Version

- ✅ **Better performance**: Native code compilation
- ✅ **Type safety**: Strong typing throughout
- ✅ **Modern async/await**: Built-in async patterns
- ✅ **Cross-platform**: Runs on Windows, Linux, macOS
- ✅ **Better debugging**: Excellent tooling support
- ✅ **Dependency injection**: Built-in DI container
- ✅ **SignalR**: Modern, efficient real-time communication

### Current Limitations

- ⚠️ **3D visualization**: Blazor doesn't have direct Three.js equivalent (can use Blazor.JSInterop)
- ⚠️ **Desktop app**: No direct Electron equivalent (can use MAUI or Avalonia)
- ⚠️ **Some sensors**: Temperature and fuel sensor services need completion

### What's Ported

- ✅ OBD-II communication via SerialPort
- ✅ GPIO service for warning lights
- ✅ SignalR real-time communication
- ✅ Server-side architecture
- ✅ Basic client structure

### What Needs Work

- 🔧 Temperature sensor service (DS18B20)
- 🔧 Fuel sensor service (ADS1115)
- 🔧 Ignition service with power management
- 🔧 Client UI dashboard
- 🔧 3D car model visualization
- 🔧 Desktop wrapper (MAUI/Avalonia)

## 📝 License

This project is licensed under **GNU General Public License v3.0 or later**.

```
PandaOS - .NET Port
Copyright (C) 2025  Cyberpandino

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License version 3.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU General Public License for more details.
```

## 👥 Authors

- **Original Project**: Matteo Errera, Roberto Zaccardi, Ludovico Verde
- **.NET Port**: [Contributor Name]

## 🤝 Contributing

Contributions are welcome! The .NET version still needs:
- Complete sensor implementations
- Client UI development
- Desktop wrapper
- Testing on Raspberry Pi hardware

See the original [CONTRIBUTING.md](../.github/CONTRIBUTING.md) for guidelines.

## 📞 Support

For issues specific to the .NET port, please open an issue on GitHub with the `dotnet-port` label.
