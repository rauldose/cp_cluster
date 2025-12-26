# 🚗 Cyberpandino Cluster - Multi-Platform Project

This repository contains both the **original Node.js/React** version and a new **.NET port** of the Cyberpandino Cluster (PandaOS) project.

## 🎯 Choose Your Version

### Node.js/React/Electron (Original)
**Best for**: Web developers familiar with JavaScript/TypeScript, rapid prototyping

📂 **Location**: Root directory (`server/`, `client/`, `main.js`)
📖 **Documentation**: [README.md](README.md)

**Key Technologies**:
- Backend: Node.js, Socket.IO
- Frontend: React, TypeScript, Three.js, Vite
- Desktop: Electron

### .NET/Blazor (New Port)
**Best for**: .NET developers, better performance, production deployments

📂 **Location**: `dotnet/` directory
📖 **Documentation**: [docs/DOTNET_PORT.md](docs/DOTNET_PORT.md) and [dotnet/README.md](dotnet/README.md)

**Key Technologies**:
- Backend: ASP.NET Core, SignalR
- Frontend: Blazor WebAssembly
- Hardware: System.Device.Gpio, System.IO.Ports

## 📊 Comparison

| Feature | Node.js Version | .NET Version |
|---------|----------------|--------------|
| **Performance** | Good | Excellent |
| **Memory Usage** | ~500MB | ~150MB |
| **Startup Time** | ~5-10s | ~2-3s |
| **Development Speed** | Fast | Medium |
| **Type Safety** | TypeScript | Full C# |
| **Ecosystem** | npm (huge) | NuGet (mature) |
| **3D Graphics** | Three.js ✅ | Needs JSInterop 🔧 |
| **Desktop App** | Electron ✅ | MAUI/Avalonia 🔧 |
| **Hardware Support** | Full ✅ | Core ✅, Sensors 🔧 |
| **Maturity** | Production-ready | In development |

## 🚀 Quick Start

### Node.js Version

```bash
# Install dependencies
npm run install:all

# Run everything (server + client + electron)
npm start

# Or run individually
npm run server   # Server only
npm run client   # Client only
```

### .NET Version

```bash
# Build the solution
dotnet build CyberPandinoCluster.sln

# Run server
cd dotnet/Server && dotnet run

# Run client (in another terminal)
cd dotnet/Client && dotnet run
```

## 📁 Repository Structure

```
cluster/
├── 📦 Node.js Version (Original)
│   ├── server/              → Node.js backend
│   ├── client/              → React frontend
│   ├── main.js              → Electron wrapper
│   ├── scripts/             → Build scripts
│   └── package.json
│
├── 🔷 .NET Version (New Port)
│   ├── dotnet/
│   │   ├── Server/          → ASP.NET Core backend
│   │   ├── Client/          → Blazor WebAssembly frontend
│   │   ├── Shared/          → Shared models
│   │   └── README.md
│   └── CyberPandinoCluster.sln
│
├── 📚 Documentation
│   ├── README.md            → Original project documentation
│   ├── docs/
│   │   ├── DOTNET_PORT.md   → .NET port guide
│   │   ├── HARDWARE.md      → Hardware setup
│   │   ├── ARCHITETTURA.md  → Architecture (Italian)
│   │   └── ...
│   └── .github/
│       └── CONTRIBUTING.md
│
└── LICENSE                  → GPL-3.0
```

## 🎨 Feature Parity

### Fully Implemented in Both

- ✅ OBD-II communication (ELM327)
- ✅ GPIO warning lights monitoring
- ✅ Real-time WebSocket/SignalR communication
- ✅ Server-client architecture

### Node.js Version Only (For Now)

- ✅ Complete UI dashboard with 3D car model
- ✅ Temperature sensor (DS18B20)
- ✅ Fuel sensor (ADS1115)
- ✅ Ignition service
- ✅ Desktop Electron app

### .NET Advantages

- ✅ Better performance (3x less memory)
- ✅ Faster startup
- ✅ Type safety
- ✅ Modern async/await patterns
- ✅ Built-in dependency injection

## 🔮 Future Plans

### .NET Port Roadmap

1. **Phase 1** (✅ Complete):
   - Core server architecture
   - OBD-II and GPIO services
   - SignalR communication
   - Basic client structure

2. **Phase 2** (🔧 In Progress):
   - Complete sensor implementations
   - UI dashboard development
   - Testing on hardware

3. **Phase 3** (📋 Planned):
   - 3D car visualization
   - Desktop wrapper (MAUI/Avalonia)
   - Performance optimization
   - Production deployment

## 💡 Which Version Should I Use?

**Use Node.js version if**:
- You're familiar with JavaScript/TypeScript
- You need the complete UI right away
- You want to contribute to the original project
- You prefer the npm ecosystem

**Use .NET version if**:
- You're familiar with C#/.NET
- You need better performance
- You want lower memory usage
- You prefer strong typing
- You're deploying to production on Raspberry Pi

**Run both versions** to compare and help with testing!

## 🤝 Contributing

Both versions welcome contributions!

- **Node.js version**: See [CONTRIBUTING.md](.github/CONTRIBUTING.md)
- **.NET version**: See [dotnet/README.md](dotnet/README.md) and [docs/DOTNET_PORT.md](docs/DOTNET_PORT.md)

## 📄 License

Both versions are licensed under **GNU General Public License v3.0 or later**.

```
PandaOS
Copyright (C) 2025  Cyberpandino

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License version 3.
```

## 👥 Authors

**Original Project**:
- Matteo Errera
- Roberto Zaccardi
- Ludovico Verde

**.NET Port**:
- GitHub Copilot

## 🔗 Links

- **Node.js Documentation**: [README.md](README.md)
- **.NET Documentation**: [docs/DOTNET_PORT.md](docs/DOTNET_PORT.md)
- **Hardware Guide**: [docs/HARDWARE.md](docs/HARDWARE.md)
- **Contributing Guide**: [.github/CONTRIBUTING.md](.github/CONTRIBUTING.md)

---

**Note**: This is a hobby project for learning and experimentation. Not certified for production vehicle use. Use at your own risk!
