using System.Text.Json.Serialization;

namespace EarthquakeMap2.Json;

#nullable disable

public interface IProperties
{
    public string Code { get; set; }
    public string Name { get; set; }
}

public class PrefProperties : IProperties
{
    [JsonPropertyName("code")]
    public string Code { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }
}

public class AreaProperties : IProperties
{
    [JsonPropertyName("code")]
    public string Code { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("namekana")]
    public string NameKana { get; set; }
}

public class CityProperties : IProperties
{
    [JsonPropertyName("regioncode")]
    public string Code { get; set; }

    [JsonPropertyName("regioname")]
    public string RegionName { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("namekana")]
    public string NameKana { get; set; }
}
