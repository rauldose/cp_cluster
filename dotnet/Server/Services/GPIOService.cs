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

using System.Device.Gpio;
using CyberPandinoCluster.Shared.Models;

namespace CyberPandinoCluster.Server.Services;

public class GPIOService : IDisposable
{
    private readonly ILogger<GPIOService> _logger;
    private GpioController? _gpioController;
    private readonly Dictionary<string, int> _pinMapping;
    private GPIOWarnings _currentWarnings;

    public event Action<GPIOWarnings>? OnWarningsChanged;

    public GPIOService(ILogger<GPIOService> logger)
    {
        _logger = logger;
        _currentWarnings = new GPIOWarnings();

        // GPIO pin mapping (BCM numbering)
        _pinMapping = new Dictionary<string, int>
        {
            { "TurnSignal", 17 },
            { "Alternator", 27 },
            { "OilPressure", 22 },
            { "BrakeSystem", 23 },
            { "Injectors", 24 },
            { "KeyOn", 25 },
            { "HighBeam", 5 },
            { "LowBeam", 6 },
            { "HazardLights", 12 },
            { "FogLight", 13 },
            { "CoolantTemperature", 16 },
            { "RearDefrost", 19 },
            { "FuelReserve", 20 },
            { "Ignition", 21 }
        };
    }

    public bool Initialize()
    {
        try
        {
            _gpioController = new GpioController();

            // Initialize all pins as input with pull-down
            foreach (var pin in _pinMapping.Values)
            {
                _gpioController.OpenPin(pin, PinMode.InputPullDown);
            }

            _logger.LogInformation("GPIO initialized successfully with {PinCount} pins", _pinMapping.Count);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize GPIO");
            return false;
        }
    }

    public GPIOWarnings ReadWarnings()
    {
        if (_gpioController == null)
            return _currentWarnings;

        try
        {
            _currentWarnings = new GPIOWarnings
            {
                TurnSignal = ReadPin("TurnSignal"),
                Alternator = ReadPin("Alternator"),
                OilPressure = ReadPin("OilPressure"),
                BrakeSystem = ReadPin("BrakeSystem"),
                Injectors = ReadPin("Injectors"),
                KeyOn = ReadPin("KeyOn"),
                HighBeam = ReadPin("HighBeam"),
                LowBeam = ReadPin("LowBeam"),
                HazardLights = ReadPin("HazardLights"),
                FogLight = ReadPin("FogLight"),
                CoolantTemperature = ReadPin("CoolantTemperature"),
                RearDefrost = ReadPin("RearDefrost"),
                FuelReserve = ReadPin("FuelReserve"),
                Ignition = ReadPin("Ignition"),
                Timestamp = DateTime.UtcNow
            };

            OnWarningsChanged?.Invoke(_currentWarnings);
            return _currentWarnings;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading GPIO pins");
            return _currentWarnings;
        }
    }

    private bool ReadPin(string pinName)
    {
        if (_gpioController == null || !_pinMapping.ContainsKey(pinName))
            return false;

        try
        {
            var pinNumber = _pinMapping[pinName];
            return _gpioController.Read(pinNumber) == PinValue.High;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading pin {PinName}", pinName);
            return false;
        }
    }

    public void Dispose()
    {
        if (_gpioController != null)
        {
            foreach (var pin in _pinMapping.Values)
            {
                try
                {
                    _gpioController.ClosePin(pin);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error closing pin {Pin}", pin);
                }
            }
            _gpioController.Dispose();
            _logger.LogInformation("GPIO disposed");
        }
    }
}
