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

public class GPIOWarnings
{
    public bool TurnSignal { get; set; }
    public bool Alternator { get; set; }
    public bool OilPressure { get; set; }
    public bool BrakeSystem { get; set; }
    public bool Injectors { get; set; }
    public bool KeyOn { get; set; }
    public bool HighBeam { get; set; }
    public bool LowBeam { get; set; }
    public bool HazardLights { get; set; }
    public bool FogLight { get; set; }
    public bool CoolantTemperature { get; set; }
    public bool RearDefrost { get; set; }
    public bool FuelReserve { get; set; }
    public bool Ignition { get; set; }
    public DateTime Timestamp { get; set; }
}
