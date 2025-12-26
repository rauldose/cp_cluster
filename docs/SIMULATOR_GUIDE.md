# Simulator Mode - Testing Without Hardware

## Overview

The .NET port includes a built-in simulator that generates realistic vehicle data, allowing you to test the complete cluster system on any Windows, Linux, or macOS machine without Raspberry Pi hardware.

## 🎮 What Gets Simulated

### Vehicle Dynamics
- **Speed (0-120 km/h)**: Realistic acceleration and deceleration with momentum
- **RPM (800-6000)**: Correlated with speed, includes idle and gear shifts
- **Engine Temperature**: Gradual warm-up from 20°C to operating temp (90°C)
- **Fuel Consumption**: Decreases slowly when driving (0.1% per 100 updates at speed)

### Environmental Data
- **External Temperature**: Slowly varying ambient temperature (5-35°C)
- **Fuel Level**: Includes voltage simulation for ADS1115 ADC

### Warning Lights (Dynamic)
- ✅ **Turn Signal**: Blinks when moving (500ms on/off pattern)
- ✅ **Alternator**: Activates when RPM < 700 (low charging)
- ✅ **Oil Pressure**: Activates when RPM > 5500 (high stress)
- ✅ **Hazard Lights**: Blinks when stopped
- ✅ **Headlights**: High beam and low beam toggle
- ✅ **Fog Light**: On at low speeds
- ✅ **Fuel Reserve**: Activates when fuel < 15%
- ✅ **Coolant Warning**: Activates when temp > 105°C
- ✅ **Key/Ignition**: On when engine running

## 🚀 Quick Start

### 1. Run the Server (Simulator Mode)

```bash
cd dotnet/Server
dotnet run
```

Look for these log messages:
```
🎮 Not running on Raspberry Pi. Starting in SIMULATOR mode.
💡 Simulator will generate realistic vehicle data for testing.
🎮 Simulator started - Generating realistic vehicle data every 250ms
💡 Tip: The simulator creates dynamic data - speed changes, warnings blink, temperature varies
```

### 2. Run the Client

```bash
cd dotnet/Client
dotnet run
```

### 3. Open Dashboard

Navigate to: http://localhost:5290/dashboard

You should see:
- Speed and RPM changing dynamically
- Turn signals blinking
- Fuel level slowly decreasing
- Temperature gradually increasing
- Warning lights activating based on conditions

## 🎯 Simulation Behavior

### Driving Pattern
The simulator creates realistic driving behavior:
1. Vehicle starts at rest (0 km/h)
2. Random acceleration/deceleration
3. Speed changes smoothly with momentum
4. RPM follows speed with gear simulation
5. Engine warms up gradually

### Example Scenario
```
Time 0s:   Speed: 0 km/h,   RPM: 800,  Temp: 20°C,  Fuel: 75%
Time 30s:  Speed: 45 km/h,  RPM: 2200, Temp: 35°C,  Fuel: 75%
Time 60s:  Speed: 80 km/h,  RPM: 3400, Temp: 58°C,  Fuel: 74.8%
Time 120s: Speed: 60 km/h,  RPM: 2800, Temp: 82°C,  Fuel: 74.5%
Time 180s: Speed: 90 km/h,  RPM: 3800, Temp: 89°C,  Fuel: 74.2%
```

### Warning Light Patterns
- **Turn Signal**: Blinks 4 times per second when moving
- **Hazard**: Blinks when stopped (vehicle at rest)
- **Headlights**: Toggle between high/low beam periodically
- **Fuel Reserve**: Stays on when fuel < 15%
- **Oil Pressure**: Activates during high RPM (> 5500)

## ⚙️ Configuration

### Enable/Disable Simulator

Edit `dotnet/Server/appsettings.json`:

```json
{
  "Simulator": {
    "Enabled": true
  }
}
```

Set to `false` to disable the simulator (server will show no data on non-Raspberry Pi).

### Simulation Parameters

The simulator is configured in `SimulatorService.cs` with these defaults:

- **Update Rate**: 250ms (4Hz) - same as real hardware
- **Speed Range**: 0-120 km/h
- **RPM Range**: 800-6000
- **Temperature Range**: 20-110°C (ambient to overheating)
- **Fuel Start**: 50-90% (random)

## 🔍 Debugging

### Server Logs

When simulator is active, you'll see:
```
info: CyberPandinoCluster.Server.Services.ClusterHostedService[0]
      🎮 Not running on Raspberry Pi. Starting in SIMULATOR mode.
info: CyberPandinoCluster.Server.Services.SimulatorService[0]
      🎮 Simulator started - Generating realistic vehicle data every 250ms
```

### Client SignalR Events

The client receives:
- `obd-data` - Speed, RPM, coolant temp
- `gpio-warnings` - All 14 warning light states
- `external-temperature` - Ambient temperature
- `fuel-level` - Fuel percentage and voltage

## 💡 Use Cases

### UI Development
- Test dashboard layouts without hardware
- Verify real-time updates work correctly
- Test responsive design at different screen sizes
- Validate animations and transitions

### Integration Testing
- Verify SignalR communication
- Test event handling
- Validate data parsing
- Check error handling

### Demonstration
- Show the system to stakeholders
- Create screenshots/videos for documentation
- Demo features without vehicle access
- Test on different operating systems

## 🔧 Customization

To modify simulation behavior, edit `dotnet/Server/Services/SimulatorService.cs`:

### Change Speed Dynamics
```csharp
// Line ~70: Adjust acceleration rate
_speedChangeRate = (_random.NextDouble() - 0.5) * 5; // Faster acceleration
```

### Change Update Frequency
```csharp
// In ClusterHostedService.cs, line ~142
_simulatorService.StartSimulation(100); // 10Hz instead of 4Hz
```

### Add Custom Scenarios
You can extend the simulator to include:
- Traffic scenarios (stop-and-go)
- Highway driving (sustained high speed)
- Racing mode (high RPM, aggressive driving)
- Parking (engine off simulation)

## 📊 Comparison with Real Hardware

| Feature | Simulator | Real Hardware |
|---------|-----------|---------------|
| Speed/RPM | ✅ Simulated | ✅ ELM327 OBD-II |
| GPIO Warnings | ✅ Simulated | ✅ Raspberry Pi GPIO |
| Temperature | ✅ Simulated | ✅ DS18B20 1-Wire |
| Fuel Level | ✅ Simulated | ✅ ADS1115 I2C ADC |
| Update Rate | ✅ 4Hz | ✅ 4Hz |
| Realistic Data | ✅ Yes | ✅ Yes (real) |

## 🎓 Learning

The simulator is also a great learning tool:
- Understand how OBD-II data flows
- See how warning lights correlate with vehicle state
- Learn about SignalR real-time communication
- Explore sensor data patterns

## ❓ FAQ

**Q: Can I use the simulator on Raspberry Pi?**  
A: No, the simulator only activates on non-Raspberry Pi systems. On Raspberry Pi, the system uses real hardware.

**Q: Does the simulator affect performance?**  
A: Minimal impact. Uses <5% CPU and adds ~5MB memory.

**Q: Can I record simulated data?**  
A: Yes! You can add logging in SimulatorService to save data to files.

**Q: Can I replay recorded real data?**  
A: The simulator could be extended to replay CSV/JSON files with real vehicle data.

**Q: Why is data always changing?**  
A: The simulator creates dynamic behavior to test real-time updates. It's more realistic than static test data.

## 🔗 Related Documentation

- [Main README](README.md) - Full project documentation
- [ClusterHostedService.cs](Services/ClusterHostedService.cs) - Integration code
- [SimulatorService.cs](Services/SimulatorService.cs) - Simulation logic

---

**Enjoy testing without hardware! 🎮**
