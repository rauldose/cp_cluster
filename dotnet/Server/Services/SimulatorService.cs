/*
 * PandaOS
 * Copyright (C) 2025  Cyberpandino
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License version 3.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 * GNU General Public License for more details.
 */

using CyberPandinoCluster.Shared.Models;

namespace CyberPandinoCluster.Server.Services;

/// <summary>
/// Simulator service that generates realistic vehicle data for testing without hardware
/// </summary>
public class SimulatorService : IDisposable
{
    private readonly ILogger<SimulatorService> _logger;
    private readonly Random _random = new();
    private Timer? _simulationTimer;
    private bool _isRunning;
    
    // Simulation state
    private double _currentSpeed;
    private double _currentRpm;
    private double _currentCoolantTemp;
    private double _currentExternalTemp;
    private double _currentFuelLevel;
    private double _speedChangeRate;
    private int _warningBlinkCounter;

    public event Action<OBDData>? OnOBDDataSimulated;
    public event Action<GPIOWarnings>? OnGPIOWarningsSimulated;
    public event Action<TemperatureData>? OnTemperatureSimulated;
    public event Action<FuelData>? OnFuelLevelSimulated;

    public SimulatorService(ILogger<SimulatorService> logger)
    {
        _logger = logger;
        InitializeSimulation();
    }

    private void InitializeSimulation()
    {
        // Initialize with realistic starting values
        _currentSpeed = 0;
        _currentRpm = 800; // Idle RPM
        _currentCoolantTemp = 20; // Cold start
        _currentExternalTemp = 15 + _random.NextDouble() * 10; // 15-25°C
        _currentFuelLevel = 50 + _random.NextDouble() * 40; // 50-90%
        _speedChangeRate = 0;
    }

    public void StartSimulation(int intervalMs = 250)
    {
        if (_isRunning)
            return;

        _isRunning = true;
        _logger.LogInformation("🎮 Simulator started - Generating realistic vehicle data every {Interval}ms", intervalMs);
        _logger.LogInformation("💡 Tip: The simulator creates dynamic data - speed changes, warnings blink, temperature varies");

        _simulationTimer = new Timer(_ =>
        {
            UpdateSimulation();
        }, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(intervalMs));
    }

    public void StopSimulation()
    {
        _simulationTimer?.Dispose();
        _simulationTimer = null;
        _isRunning = false;
        _logger.LogInformation("Simulator stopped");
    }

    private void UpdateSimulation()
    {
        // Simulate realistic driving behavior
        SimulateSpeed();
        SimulateRpm();
        SimulateCoolantTemperature();
        SimulateExternalTemperature();
        SimulateFuelLevel();

        // Emit simulated data
        EmitOBDData();
        EmitGPIOWarnings();
        EmitTemperatureData();
        EmitFuelData();
    }

    private void SimulateSpeed()
    {
        // Simulate acceleration/deceleration with some randomness
        if (_random.NextDouble() < 0.05) // 5% chance to change speed pattern
        {
            _speedChangeRate = (_random.NextDouble() - 0.5) * 3; // -1.5 to +1.5 km/h per update
        }

        _currentSpeed += _speedChangeRate;
        
        // Add small random variations
        _currentSpeed += (_random.NextDouble() - 0.5) * 0.5;
        
        // Keep speed within realistic bounds (0-120 km/h)
        _currentSpeed = Math.Max(0, Math.Min(120, _currentSpeed));
        
        // Gradually reduce speed change rate (simulate friction)
        _speedChangeRate *= 0.98;
    }

    private void SimulateRpm()
    {
        // RPM correlates with speed, but with some variation
        if (_currentSpeed < 5)
        {
            // Idle
            _currentRpm = 800 + _random.NextDouble() * 100;
        }
        else
        {
            // Driving - roughly 40 RPM per km/h + gear shifts
            var baseRpm = _currentSpeed * 40;
            var gearFactor = 1.0 - (_currentSpeed / 150.0) * 0.3; // Higher gears = lower RPM
            _currentRpm = baseRpm * gearFactor + _random.NextDouble() * 200;
            _currentRpm = Math.Min(6000, _currentRpm); // Redline at 6000
        }
    }

    private void SimulateCoolantTemperature()
    {
        // Engine warms up gradually
        var targetTemp = _currentSpeed > 0 ? 90 : 75; // Operating temp or cooldown
        var tempDiff = targetTemp - _currentCoolantTemp;
        _currentCoolantTemp += tempDiff * 0.01; // Slow temperature change
        _currentCoolantTemp += (_random.NextDouble() - 0.5) * 0.5; // Small variations
    }

    private void SimulateExternalTemperature()
    {
        // External temperature changes very slowly
        if (_random.NextDouble() < 0.01) // 1% chance per update
        {
            _currentExternalTemp += (_random.NextDouble() - 0.5) * 0.2;
            _currentExternalTemp = Math.Max(5, Math.Min(35, _currentExternalTemp));
        }
    }

    private void SimulateFuelLevel()
    {
        // Fuel decreases very slowly when driving
        if (_currentSpeed > 10)
        {
            _currentFuelLevel -= 0.001; // Decreases 0.1% every 100 updates
            _currentFuelLevel = Math.Max(0, _currentFuelLevel);
        }
    }

    private void EmitOBDData()
    {
        var data = new OBDData
        {
            Speed = (int)Math.Round(_currentSpeed),
            Rpm = (int)Math.Round(_currentRpm),
            CoolantTemperature = Math.Round(_currentCoolantTemp, 1),
            Timestamp = DateTime.UtcNow
        };

        OnOBDDataSimulated?.Invoke(data);
    }

    private void EmitGPIOWarnings()
    {
        _warningBlinkCounter++;

        var warnings = new GPIOWarnings
        {
            // Turn signal blinks
            TurnSignal = (_warningBlinkCounter % 8 < 4) && _currentSpeed > 5,
            
            // Alternator warning if RPM too low (battery not charging)
            Alternator = _currentRpm < 700,
            
            // Oil pressure warning if RPM too high
            OilPressure = _currentRpm > 5500,
            
            // Brake system (random occasional warning)
            BrakeSystem = _random.NextDouble() < 0.02,
            
            // Injectors (rarely)
            Injectors = _random.NextDouble() < 0.01,
            
            // Key on when engine running
            KeyOn = _currentRpm > 100,
            
            // High beam (occasionally)
            HighBeam = (_warningBlinkCounter % 100 < 50) && _currentSpeed > 20,
            
            // Low beam (most of the time when driving)
            LowBeam = _currentSpeed > 10 && !(_warningBlinkCounter % 100 < 50),
            
            // Hazard lights (blinks when stopped)
            HazardLights = _currentSpeed < 1 && (_warningBlinkCounter % 6 < 3),
            
            // Fog light (when speed is low)
            FogLight = _currentSpeed > 0 && _currentSpeed < 30,
            
            // Coolant temperature warning when too hot
            CoolantTemperature = _currentCoolantTemp > 105,
            
            // Rear defrost (occasionally)
            RearDefrost = _random.NextDouble() < 0.1,
            
            // Fuel reserve when low
            FuelReserve = _currentFuelLevel < 15,
            
            // Ignition always on when engine running
            Ignition = _currentRpm > 100,
            
            Timestamp = DateTime.UtcNow
        };

        OnGPIOWarningsSimulated?.Invoke(warnings);
    }

    private void EmitTemperatureData()
    {
        var data = new TemperatureData
        {
            Temperature = Math.Round(_currentExternalTemp, 1),
            Timestamp = DateTime.UtcNow
        };

        OnTemperatureSimulated?.Invoke(data);
    }

    private void EmitFuelData()
    {
        // Simulate voltage reading based on fuel level
        var voltageEmpty = 0.5;
        var voltageFull = 4.0;
        var voltage = voltageEmpty + (_currentFuelLevel / 100.0) * (voltageFull - voltageEmpty);

        var data = new FuelData
        {
            Level = Math.Round(_currentFuelLevel, 1),
            Voltage = Math.Round(voltage, 2),
            Timestamp = DateTime.UtcNow
        };

        OnFuelLevelSimulated?.Invoke(data);
    }

    public bool IsRunning() => _isRunning;

    public void Dispose()
    {
        StopSimulation();
    }
}
