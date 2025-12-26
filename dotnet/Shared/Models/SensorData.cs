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

namespace CyberPandinoCluster.Shared.Models;

public class TemperatureData
{
    public double Temperature { get; set; }
    public DateTime Timestamp { get; set; }
}

public class FuelData
{
    public double Level { get; set; }
    public double Voltage { get; set; }
    public DateTime Timestamp { get; set; }
}
