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
    private readonly IServiceProvider _serviceProvider;

    public ClusterHostedService(
        ILogger<ClusterHostedService> logger,
        IHubContext<ClusterHub> hubContext,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _hubContext = hubContext;
        _serviceProvider = serviceProvider;

        // Create services
        var obdLogger = _serviceProvider.GetRequiredService<ILogger<OBDCommunicationService>>();
        var gpioLogger = _serviceProvider.GetRequiredService<ILogger<GPIOService>>();
        
        _obdService = new OBDCommunicationService(obdLogger);
        _gpioService = new GPIOService(gpioLogger);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Cluster Hosted Service starting...");

        // Check if running on Raspberry Pi
        var isRaspberryPi = CheckRaspberryPi();
        if (!isRaspberryPi)
        {
            _logger.LogWarning("Not running on Raspberry Pi. Service will run in limited mode.");
            // Continue running but without hardware access
        }

        // Initialize services
        if (isRaspberryPi)
        {
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
        }

        // Main loop
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                if (isRaspberryPi)
                {
                    // Read OBD data
                    var obdData = await _obdService.ReadDataAsync();
                    await _hubContext.Clients.All.SendAsync("obd-data", obdData, stoppingToken);

                    // Read GPIO warnings
                    var warnings = _gpioService.ReadWarnings();
                    await _hubContext.Clients.All.SendAsync("gpio-warnings", warnings, stoppingToken);
                }

                // Wait before next iteration
                await Task.Delay(100, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in cluster service loop");
                await Task.Delay(1000, stoppingToken);
            }
        }

        _logger.LogInformation("Cluster Hosted Service stopping...");
        _obdService.Dispose();
        _gpioService.Dispose();
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
