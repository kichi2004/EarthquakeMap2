using System.Text.Json.Serialization;

#nullable disable

namespace EarthquakeMap2.Json
{

    public class StationDataItem
    {
        [JsonPropertyName("region")]
        public Region Region { get; set; }
        [JsonPropertyName("city")]
        public City City { get; set; }
        [JsonPropertyName("noCode")]
        public string NoCode { get; set; }
        [JsonPropertyName("code")]
        public string Code { get; set; }
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("kana")]
        public string Kana { get; set; }
        [JsonPropertyName("status")]
        public string Status { get; set; }
        [JsonPropertyName("owner")]
        public string Owner { get; set; }
        [JsonPropertyName("latitude")]
        public string Latitude { get; set; }
        [JsonPropertyName("longitude")]
        public string Longitude { get; set; }
    }

    public class Region
    {
        [JsonPropertyName("code")]
        public string Code { get; set; }
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("kana")]
        public string Kana { get; set; }
    }

    public class City
    {
        [JsonPropertyName("code")]
        public string Code { get; set; }
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("kana")]
        public string Kana { get; set; }
    }

}
