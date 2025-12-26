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

using Iot.Device.Ads1115;
using System.Device.I2c;
using UnitsNet;
using CyberPandinoCluster.Shared.Models;

namespace CyberPandinoCluster.Server.Services;

/// <summary>
/// Service for reading fuel level via ADS1115 ADC over I2C
/// </summary>
public class FuelSensorService : IDisposable
{
    private readonly ILogger<FuelSensorService> _logger;
    private Ads1115? _adc;
    private bool _isInitialized;
    private Timer? _readTimer;
    private double? _lastFuelLevel;

    // Configuration
    private readonly int _i2cBusId = 1; // Default I2C bus on Raspberry Pi
    private readonly int _i2cAddress = 0x48; // Default ADS1115 address
    private readonly InputMultiplexer _channel = InputMultiplexer.AIN0; // Channel A0
    
    // Voltage divider configuration
    private readonly double _r1 = 100000; // 100kΩ
    private readonly double _r2 = 33000;  // 33kΩ
    
    // Calibration values
    private double _voltageEmpty = 0.5; // Voltage when tank is empty (V)
    private double _voltageFull = 4.0;  // Voltage when tank is full (V)

    public event Action<FuelData>? OnFuelLevelChanged;

    public FuelSensorService(ILogger<FuelSensorService> logger)
    {
        _logger = logger;
    }

    public bool Initialize()
    {
        try
        {
            var settings = new I2cConnectionSettings(_i2cBusId, _i2cAddress);
            var i2cDevice = I2cDevice.Create(settings);
            
            _adc = new Ads1115(i2cDevice, InputMultiplexer.AIN0, MeasuringRange.FS4096);
            
            _isInitialized = true;
            _logger.LogInformation("ADS1115 fuel sensor initialized successfully on I2C bus {Bus} address 0x{Address:X2}", 
                _i2cBusId, _i2cAddress);
            
            // Read initial fuel level
            var initialLevel = ReadFuelLevel();
            if (initialLevel.HasValue)
            {
                _logger.LogInformation("Initial fuel level: {Level:F1}%", initialLevel.Value);
            }
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing ADS1115 fuel sensor. Check I2C connections (SDA/SCL).");
            return false;
        }
    }

    public void StartReading(int intervalMs = 500)
    {
        if (!_isInitialized || _readTimer != null)
            return;

        _readTimer = new Timer(_ =>
        {
            PerformReading();
        }, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(intervalMs));

        _logger.LogInformation("ADS1115 fuel reading started (interval: {Interval}ms)", intervalMs);
    }

    public void StopReading()
    {
        _readTimer?.Dispose();
        _readTimer = null;
        _logger.LogInformation("ADS1115 fuel reading stopped");
    }

    private void PerformReading()
    {
        if (!_isInitialized || _adc == null)
            return;

        try
        {
            var voltage = _adc.ReadVoltage(_channel);
            var fuelLevel = ProcessFuelReading(voltage);

            if (fuelLevel.HasValue)
            {
                // Only notify if change is significant (> 0.5%)
                if (!_lastFuelLevel.HasValue || Math.Abs(fuelLevel.Value - _lastFuelLevel.Value) > 0.5)
                {
                    _lastFuelLevel = fuelLevel;
                    NotifyFuelLevel(fuelLevel.Value, voltage);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading fuel level from ADS1115");
        }
    }

    private double? ProcessFuelReading(ElectricPotential voltage)
    {
        try
        {
            // Convert ADC voltage to actual voltage using voltage divider formula
            var vAdc = voltage.Volts;
            var vReal = vAdc * ((_r1 + _r2) / _r2);
            
            // Calculate fuel percentage using calibration
            var fuelPercent = VoltageToFuelPercent(vReal);
            
            return fuelPercent;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing fuel reading");
            return null;
        }
    }

    private double VoltageToFuelPercent(double voltage)
    {
        // Linear interpolation between empty and full voltages
        var percent = ((voltage - _voltageEmpty) / (_voltageFull - _voltageEmpty)) * 100;
        
        // Clamp between 0 and 100
        return Math.Max(0, Math.Min(100, percent));
    }

    private void NotifyFuelLevel(double fuelPercent, ElectricPotential voltage)
    {
        var data = new FuelData
        {
            Level = Math.Round(fuelPercent, 1), // Round to 1 decimal
            Voltage = voltage.Volts,
            Timestamp = DateTime.UtcNow
        };

        OnFuelLevelChanged?.Invoke(data);
        _logger.LogInformation("Fuel level: {Level:F1}% (Voltage: {Voltage:F2}V)", fuelPercent, voltage.Volts);
    }

    public double? ReadFuelLevel()
    {
        if (!_isInitialized || _adc == null)
            return null;

        try
        {
            var voltage = _adc.ReadVoltage(_channel);
            return ProcessFuelReading(voltage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading fuel level");
            return null;
        }
    }

    public void Calibrate(bool isEmpty, double voltage)
    {
        if (isEmpty)
        {
            _voltageEmpty = voltage;
            _logger.LogInformation("Calibration - Empty tank: {Voltage}V", voltage);
        }
        else
        {
            _voltageFull = voltage;
            _logger.LogInformation("Calibration - Full tank: {Voltage}V", voltage);
        }
    }

    public double? GetLastFuelLevel() => _lastFuelLevel;

    public bool IsActive() => _isInitialized && _readTimer != null;

    public void Dispose()
    {
        StopReading();
        _adc?.Dispose();
    }
}
