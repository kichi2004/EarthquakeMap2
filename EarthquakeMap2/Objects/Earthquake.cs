namespace EarthquakeMap2.Objects;

public record Earthquake
{
    public Earthquake(DateTime originTime) => OriginTime = originTime;
    public Earthquake(DateTime originTime, Hypocenter hypocenter, string magnitude) : this(originTime)
    {
        Hypocenter = hypocenter;
        Magnitude = magnitude;
    }

    public DateTime OriginTime { get; }
    public Hypocenter? Hypocenter { get; }
    public string? Magnitude { get; }
}

public record Hypocenter
{
    public Hypocenter(string code, string name, string? detailedName, string? detailedCode, Coordinate? coordinate, int? depth)
    {
        Code = code;
        Name = name;
        DetailedName = detailedName;
        DetailedCode = detailedCode;
        Coordinate = coordinate;
        Depth = depth;
    }

    public Hypocenter(string code, string name, Coordinate? coordinate, int? depth)
    {
        Code = code;
        Name = name;
        Coordinate = coordinate;
        Depth = depth;
    }

    public string Code { get; }
    public string Name { get; }
    public string? DetailedName { get; } = null;
    public string? DetailedCode { get; } = null;
    public Coordinate? Coordinate { get; }
    public int? Depth { get; }
}
