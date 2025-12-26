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

using Microsoft.AspNetCore.SignalR.Client;
using CyberPandinoCluster.Shared.Models;

namespace CyberPandinoCluster.Client.Services;

public class ClusterDataService : IAsyncDisposable
{
    private HubConnection? _connection;
    private readonly string _hubUrl;

    public event Action<OBDData>? OnOBDData;
    public event Action<GPIOWarnings>? OnGPIOWarnings;
    public event Action<TemperatureData>? OnTemperatureData;
    public event Action<FuelData>? OnFuelData;

    public ClusterDataService(string hubUrl = "http://localhost:5086/clusterhub")
    {
        _hubUrl = hubUrl;
    }

    public async Task StartAsync()
    {
        _connection = new HubConnectionBuilder()
            .WithUrl(_hubUrl)
            .WithAutomaticReconnect()
            .Build();

        _connection.On<OBDData>("obd-data", (data) =>
        {
            OnOBDData?.Invoke(data);
        });

        _connection.On<GPIOWarnings>("gpio-warnings", (warnings) =>
        {
            OnGPIOWarnings?.Invoke(warnings);
        });

        _connection.On<TemperatureData>("external-temperature", (data) =>
        {
            OnTemperatureData?.Invoke(data);
        });

        _connection.On<FuelData>("fuel-level", (data) =>
        {
            OnFuelData?.Invoke(data);
        });

        await _connection.StartAsync();
    }

    public async Task StopAsync()
    {
        if (_connection != null)
        {
            await _connection.StopAsync();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_connection != null)
        {
            await _connection.DisposeAsync();
        }
    }
}
