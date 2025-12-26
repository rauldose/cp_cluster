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
using CyberPandinoCluster.Shared.Models;

namespace CyberPandinoCluster.Server.Hubs;

public class ClusterHub : Hub
{
    private readonly ILogger<ClusterHub> _logger;

    public ClusterHub(ILogger<ClusterHub> logger)
    {
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation("Client connected: {ConnectionId}", Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation("Client disconnected: {ConnectionId}", Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }

    public async Task ForceRestart()
    {
        _logger.LogInformation("Force restart requested by client");
        // Implement force restart logic
    }
}
