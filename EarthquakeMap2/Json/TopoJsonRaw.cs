using System.Text.Json.Serialization;

#nullable disable

namespace EarthquakeMap2.Json
{
    public class Transform
    {
        [JsonPropertyName("scale")]
        public List<double> Scale { get; set; }

        [JsonPropertyName("translate")]
        public List<double> Translate { get; set; }
    }
    

    public class Geometry<T> where T : IProperties
    {
        [JsonPropertyName("arcs")]
        public List<List<int>> Arcs { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("properties")]
        public T Properties { get; set; }
    }

    public class Layer<T> where T : IProperties
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("geometries")]
        public List<Geometry<T>> Geometries { get; set; }
    }

    public class Objects
    {
        // [JsonPropertyName("pref")]
        // public Layer<PrefProperties> Pref { get; set; }

        [JsonPropertyName("area")]
        public Layer<AreaProperties> Area { get; set; }

        // [JsonPropertyName("city")]
        // public Layer<CityProperties> City { get; set; }
    }

    public class TopoJsonRaw
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("arcs")]
        public List<List<List<int>>> Arcs { get; set; }

        [JsonPropertyName("transform")]
        public Transform Transform { get; set; }

        [JsonPropertyName("objects")]
        public Objects Objects { get; set; }
    }
}
