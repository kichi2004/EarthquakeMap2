﻿namespace EarthquakeMap2.Objects;

public record Coordinate(double Longitude, double Latitude)
{
    public Coordinate(KyoshinMonitorLib.Location loc) : this(loc.Longitude, loc.Latitude) { }
    public Coordinate(EewLibrary.Location loc) : this(loc.Longitude, loc.Latitude) { }
    public static implicit operator Coordinate(KyoshinMonitorLib.Location loc) => new(loc);
    public static implicit operator Coordinate(EewLibrary.Location loc) => new(loc);
}
