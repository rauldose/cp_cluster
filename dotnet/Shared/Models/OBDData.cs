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

public class OBDData
{
    public int Speed { get; set; }
    public int Rpm { get; set; }
    public double CoolantTemperature { get; set; }
    public double OilPressure { get; set; }
    public double FuelLevel { get; set; }
    public double ExternalTemperature { get; set; }
    public DateTime Timestamp { get; set; }
}
