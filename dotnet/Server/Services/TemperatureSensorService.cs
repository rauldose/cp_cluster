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
using System.Text.RegularExpressions;

namespace CyberPandinoCluster.Server.Services;

/// <summary>
/// Service for reading DS18B20 temperature sensor via 1-Wire protocol on Raspberry Pi
/// </summary>
public partial class TemperatureSensorService : IDisposable
{
    private readonly ILogger<TemperatureSensorService> _logger;
    private readonly string _basePath = "/sys/bus/w1/devices";
    private string? _sensorPath;
    private bool _isInitialized;
    private Timer? _readTimer;
    private double? _lastTemperature;

    // Use source-generated regex for better performance
    [GeneratedRegex(@"t=(\d+)")]
    private static partial Regex TemperatureRegex();

    public event Action<TemperatureData>? OnTemperatureChanged;

    public TemperatureSensorService(ILogger<TemperatureSensorService> logger)
    {
        _logger = logger;
    }

    public bool Initialize(string? sensorId = null)
    {
        try
        {
            if (!Directory.Exists(_basePath))
            {
                _logger.LogWarning("1-Wire path {Path} not found. DS18B20 temperature sensor unavailable.", _basePath);
                return false;
            }

            if (sensorId != null)
            {
                _sensorPath = Path.Combine(_basePath, sensorId, "w1_slave");
            }
            else
            {
                // Auto-detect first DS18B20 sensor (starts with "28-")
                var devices = Directory.GetDirectories(_basePath);
                var sensor = devices.FirstOrDefault(d => Path.GetFileName(d).StartsWith("28-"));
                
                if (sensor != null)
                {
                    _sensorPath = Path.Combine(sensor, "w1_slave");
                }
            }

            if (_sensorPath != null && File.Exists(_sensorPath))
            {
                _isInitialized = true;
                _logger.LogInformation("DS18B20 temperature sensor found: {Path}", _sensorPath);
                
                // Read initial temperature
                var initialTemp = ReadTemperature();
                if (initialTemp.HasValue)
                {
                    _logger.LogInformation("Initial external temperature: {Temp}°C", initialTemp.Value);
                }
                return true;
            }
            else
            {
                _logger.LogWarning("DS18B20 sensor not found, external temperature functionality disabled");
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing DS18B20 sensor");
            return false;
        }
    }

    public void StartReading(int intervalMs = 5000)
    {
        if (!_isInitialized || _readTimer != null)
            return;

        _readTimer = new Timer(_ =>
        {
            var temperature = ReadTemperature();
            if (temperature.HasValue && temperature != _lastTemperature)
            {
                _lastTemperature = temperature;
                NotifyTemperature(temperature.Value);
            }
        }, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(intervalMs));

        _logger.LogInformation("DS18B20 temperature reading started (interval: {Interval}ms)", intervalMs);
    }

    public void StopReading()
    {
        _readTimer?.Dispose();
        _readTimer = null;
        _logger.LogInformation("DS18B20 temperature reading stopped");
    }

    public double? ReadTemperature()
    {
        if (!_isInitialized || _sensorPath == null)
            return null;

        try
        {
            var lines = File.ReadAllLines(_sensorPath);
            if (lines.Length < 2)
                return null;

            // Verify CRC is valid
            if (!lines[0].Contains("YES"))
                return null;

            // Extract temperature from second line (format: "t=xxxxx")
            var tempMatch = TemperatureRegex().Match(lines[1]);
            if (tempMatch.Success && int.TryParse(tempMatch.Groups[1].Value, out int tempRaw))
            {
                // Value is in thousandths of a degree
                var tempC = tempRaw / 1000.0;
                return Math.Round(tempC, 1); // Round to 1 decimal place
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading DS18B20 sensor");
            return null;
        }
    }

    private void NotifyTemperature(double temperature)
    {
        var data = new TemperatureData
        {
            Temperature = temperature,
            Timestamp = DateTime.UtcNow
        };

        OnTemperatureChanged?.Invoke(data);
        _logger.LogInformation("External temperature: {Temp}°C", temperature);
    }

    public double? GetLastTemperature() => _lastTemperature;

    public bool IsActive() => _isInitialized && _readTimer != null;

    public void Dispose()
    {
        StopReading();
    }
}
