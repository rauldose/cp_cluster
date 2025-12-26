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

using Microsoft.AspNetCore.SignalR;
using CyberPandinoCluster.Server.Hubs;
using CyberPandinoCluster.Shared.Models;

namespace CyberPandinoCluster.Server.Services;

public class ClusterHostedService : BackgroundService
{
    private readonly ILogger<ClusterHostedService> _logger;
    private readonly IHubContext<ClusterHub> _hubContext;
    private readonly OBDCommunicationService _obdService;
    private readonly GPIOService _gpioService;
    private readonly TemperatureSensorService _temperatureService;
    private readonly FuelSensorService _fuelService;
    private readonly SimulatorService _simulatorService;
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;

    public ClusterHostedService(
        ILogger<ClusterHostedService> logger,
        IHubContext<ClusterHub> hubContext,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _hubContext = hubContext;
        _serviceProvider = serviceProvider;
        _configuration = _serviceProvider.GetRequiredService<IConfiguration>();

        // Create services
        var obdLogger = _serviceProvider.GetRequiredService<ILogger<OBDCommunicationService>>();
        var gpioLogger = _serviceProvider.GetRequiredService<ILogger<GPIOService>>();
        var tempLogger = _serviceProvider.GetRequiredService<ILogger<TemperatureSensorService>>();
        var fuelLogger = _serviceProvider.GetRequiredService<ILogger<FuelSensorService>>();
        var simulatorLogger = _serviceProvider.GetRequiredService<ILogger<SimulatorService>>();
        
        _obdService = new OBDCommunicationService(obdLogger);
        _gpioService = new GPIOService(gpioLogger);
        _temperatureService = new TemperatureSensorService(tempLogger);
        _fuelService = new FuelSensorService(fuelLogger, _configuration);
        _simulatorService = new SimulatorService(simulatorLogger);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Cluster Hosted Service starting...");

        // Check if running on Raspberry Pi
        var isRaspberryPi = CheckRaspberryPi();
        
        // Check if simulator mode is enabled
        var useSimulator = _configuration.GetValue<bool>("Simulator:Enabled", true);
        
        if (!isRaspberryPi)
        {
            if (useSimulator)
            {
                _logger.LogInformation("🎮 Not running on Raspberry Pi. Starting in SIMULATOR mode.");
                _logger.LogInformation("💡 Simulator will generate realistic vehicle data for testing.");
                _logger.LogInformation("💡 To disable simulator, set 'Simulator:Enabled' to false in appsettings.json");
                
                // Setup simulator event handlers
                _simulatorService.OnOBDDataSimulated += async (data) =>
                {
                    await _hubContext.Clients.All.SendAsync("obd-data", data, stoppingToken);
                };
                
                _simulatorService.OnGPIOWarningsSimulated += async (data) =>
                {
                    await _hubContext.Clients.All.SendAsync("gpio-warnings", data, stoppingToken);
                };
                
                _simulatorService.OnTemperatureSimulated += async (data) =>
                {
                    await _hubContext.Clients.All.SendAsync("external-temperature", data, stoppingToken);
                };
                
                _simulatorService.OnFuelLevelSimulated += async (data) =>
                {
                    await _hubContext.Clients.All.SendAsync("fuel-level", data, stoppingToken);
                };
                
                // Start simulator
                _simulatorService.StartSimulation(250); // 4Hz update rate
                
                // Keep service running while simulator is active
                while (!stoppingToken.IsCancellationRequested)
                {
                    await Task.Delay(1000, stoppingToken);
                }
            }
            else
            {
                _logger.LogWarning("Not running on Raspberry Pi. Service will run in limited mode.");
                _logger.LogInformation("No data will be generated. Set 'Simulator:Enabled' to true to enable simulator.");
            }
        }
        else
        {
            // Raspberry Pi mode - use real hardware
            _logger.LogInformation("Running on Raspberry Pi. Using real hardware sensors.");
            
            // Initialize services
            var obdConnected = await _obdService.ConnectAsync();
            if (!obdConnected)
            {
                _logger.LogWarning("OBD-II not connected. Continuing without OBD data.");
            }

            var gpioInitialized = _gpioService.Initialize();
            if (!gpioInitialized)
            {
                _logger.LogWarning("GPIO not initialized. Continuing without GPIO data.");
            }

            // Initialize temperature sensor (DS18B20)
            var tempInitialized = _temperatureService.Initialize();
            if (tempInitialized)
            {
                // Setup event handler
                _temperatureService.OnTemperatureChanged += async (data) =>
                {
                    await _hubContext.Clients.All.SendAsync("external-temperature", data, stoppingToken);
                };
                _temperatureService.StartReading(5000); // Read every 5 seconds
            }
            else
            {
                _logger.LogWarning("Temperature sensor not initialized. Continuing without temperature data.");
            }

            // Initialize fuel sensor (ADS1115)
            var fuelInitialized = _fuelService.Initialize();
            if (fuelInitialized)
            {
                // Setup event handler
                _fuelService.OnFuelLevelChanged += async (data) =>
                {
                    await _hubContext.Clients.All.SendAsync("fuel-level", data, stoppingToken);
                };
                _fuelService.StartReading(500); // Read every 500ms
            }
            else
            {
                _logger.LogWarning("Fuel sensor not initialized. Continuing without fuel data.");
            }
            
            // Main loop for hardware
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Read OBD data
                    var obdData = await _obdService.ReadDataAsync();
                    await _hubContext.Clients.All.SendAsync("obd-data", obdData, stoppingToken);

                    // Read GPIO warnings
                    var warnings = _gpioService.ReadWarnings();
                    await _hubContext.Clients.All.SendAsync("gpio-warnings", warnings, stoppingToken);

                    // Wait before next iteration (250ms = 4 times per second)
                    await Task.Delay(250, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in cluster service loop");
                    await Task.Delay(1000, stoppingToken);
                }
            }
        }

        _logger.LogInformation("Cluster Hosted Service stopping...");
        _simulatorService.Dispose();
        _obdService.Dispose();
        _gpioService.Dispose();
        _temperatureService.Dispose();
        _fuelService.Dispose();
    }

    private bool CheckRaspberryPi()
    {
        // Check if running on Linux ARM (Raspberry Pi)
        var isLinux = OperatingSystem.IsLinux();
        var architecture = System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture;
        var isArm = architecture == System.Runtime.InteropServices.Architecture.Arm ||
                    architecture == System.Runtime.InteropServices.Architecture.Arm64;

        return isLinux && isArm;
    }
}
