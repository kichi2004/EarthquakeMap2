using System.Collections.ObjectModel;
using EarthquakeLibrary;

namespace EarthquakeMap2.Objects;

public partial class EarthquakeInformation
{
    public record IntensityPref
    {
        protected internal IntensityPref(string name, string code, Intensity maxInt, ReadOnlyCollection<IntensityArea> areas)
        {
            Name = name;
            Code = code;
            MaxInt = maxInt;
            Areas = areas;
        }

        public string Name { get; }
        public string Code { get; }
        public Intensity MaxInt { get; }
        public ReadOnlyCollection<IntensityArea> Areas { get; }
    }

    public record IntensityArea
    {
        protected internal IntensityArea(string name, string code, Intensity maxInt, ReadOnlyCollection<IntensityCity>? cities)
        {
            Name = name;
            Code = code;
            MaxInt = maxInt;
            Cities = cities;
        }

        public string Name { get; }
        public string Code { get; }
        public Intensity MaxInt { get; }
        public ReadOnlyCollection<IntensityCity>? Cities { get; }

    }

    public record IntensityCity
    {
        protected internal IntensityCity(string name, string code, Intensity maxInt, ReadOnlyCollection<IntensityStation>? stations)
        {
            Name = name;
            Code = code;
            MaxInt = maxInt;
            Stations = stations;
        }

        public string Name { get; }
        public string Code { get; }
        public Intensity MaxInt { get; }
        public ReadOnlyCollection<IntensityStation>? Stations { get; }
    }

    public record IntensityStation
    {
        protected internal IntensityStation(string name, string code, Intensity i)
        {
            Name = name;
            Code = code;
            Int = i;
        }

        public string Name { get; }
        public string Code { get; }
        public Intensity Int { get; }
    }
}
