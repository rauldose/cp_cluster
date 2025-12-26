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

using System.IO.Ports;
using System.Text;
using CyberPandinoCluster.Shared.Models;

namespace CyberPandinoCluster.Server.Services;

public class OBDCommunicationService : IDisposable
{
    private readonly ILogger<OBDCommunicationService> _logger;
    private SerialPort? _serialPort;
    private readonly string _portPath = "/dev/ttyUSB0";
    private readonly int _baudRate = 38400;
    private bool _isConnected;

    public event Action<OBDData>? OnDataReceived;

    public OBDCommunicationService(ILogger<OBDCommunicationService> logger)
    {
        _logger = logger;
    }

    public async Task<bool> ConnectAsync()
    {
        try
        {
            _serialPort = new SerialPort(_portPath, _baudRate)
            {
                Parity = Parity.None,
                DataBits = 8,
                StopBits = StopBits.One,
                Handshake = Handshake.None,
                ReadTimeout = 2000,
                WriteTimeout = 2000
            };

            _serialPort.Open();
            _isConnected = true;

            // Initialize ELM327
            await SendCommandAsync("ATZ"); // Reset
            await Task.Delay(1000);
            await SendCommandAsync("ATE0"); // Echo off
            await SendCommandAsync("ATL0"); // Line feeds off
            await SendCommandAsync("ATSP0"); // Auto protocol

            _logger.LogInformation("OBD-II connected successfully on {Port}", _portPath);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect to OBD-II on {Port}", _portPath);
            _isConnected = false;
            return false;
        }
    }

    public async Task<string?> SendCommandAsync(string command)
    {
        if (_serialPort == null || !_serialPort.IsOpen)
            return null;

        try
        {
            _serialPort.WriteLine(command + "\r");
            await Task.Delay(100);
            return _serialPort.ReadExisting();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending OBD command: {Command}", command);
            return null;
        }
    }

    public async Task<OBDData> ReadDataAsync()
    {
        var data = new OBDData
        {
            Timestamp = DateTime.UtcNow
        };

        // Read speed (PID 0D)
        var speedResponse = await SendCommandAsync("010D");
        if (speedResponse != null)
        {
            data.Speed = ParseSpeed(speedResponse);
        }

        // Read RPM (PID 0C)
        var rpmResponse = await SendCommandAsync("010C");
        if (rpmResponse != null)
        {
            data.Rpm = ParseRPM(rpmResponse);
        }

        // Read coolant temperature (PID 05)
        var coolantResponse = await SendCommandAsync("0105");
        if (coolantResponse != null)
        {
            data.CoolantTemperature = ParseTemperature(coolantResponse);
        }

        return data;
    }

    private int ParseSpeed(string response)
    {
        // Parse OBD-II response for speed (km/h)
        // Response format: "41 0D XX" where XX is speed in hex
        var parts = response.Split(' ');
        if (parts.Length >= 3 && int.TryParse(parts[2], System.Globalization.NumberStyles.HexNumber, null, out int speed))
        {
            return speed;
        }
        return 0;
    }

    private int ParseRPM(string response)
    {
        // Parse OBD-II response for RPM
        // Response format: "41 0C AA BB" where RPM = ((AA * 256) + BB) / 4
        var parts = response.Split(' ');
        if (parts.Length >= 4 &&
            int.TryParse(parts[2], System.Globalization.NumberStyles.HexNumber, null, out int a) &&
            int.TryParse(parts[3], System.Globalization.NumberStyles.HexNumber, null, out int b))
        {
            return ((a * 256) + b) / 4;
        }
        return 0;
    }

    private double ParseTemperature(string response)
    {
        // Parse OBD-II response for temperature (°C)
        // Response format: "41 05 XX" where temp = XX - 40
        var parts = response.Split(' ');
        if (parts.Length >= 3 && int.TryParse(parts[2], System.Globalization.NumberStyles.HexNumber, null, out int temp))
        {
            return temp - 40;
        }
        return 0;
    }

    public void Disconnect()
    {
        if (_serialPort != null && _serialPort.IsOpen)
        {
            _serialPort.Close();
            _isConnected = false;
            _logger.LogInformation("OBD-II disconnected");
        }
    }

    public void Dispose()
    {
        Disconnect();
        _serialPort?.Dispose();
    }
}
