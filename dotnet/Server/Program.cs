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

using CyberPandinoCluster.Server.Hubs;
using CyberPandinoCluster.Server.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddSignalR();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Register hosted service
builder.Services.AddHostedService<ClusterHostedService>();

// Add OpenAPI/Swagger for development
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors();

// Map SignalR hub
app.MapHub<ClusterHub>("/clusterhub");

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }));

app.Run();
