using System.Linq;
using EarthquakeMap2.Objects;

namespace EarthquakeMap2.Json
{
    public class TopoJsonData
    {
        public TopoJsonData(TopoJsonRaw raw)
        {
            Scale = (raw.Transform.Scale[0], raw.Transform.Scale[1]);
            Translate = (raw.Transform.Translate[0], raw.Transform.Translate[1]);
            Arcs = raw.Arcs.Select(arc =>
            {
                var list = new List<Coordinate>();
                double xDelta = 0, yDelta = 0;
                foreach (var values in arc)
                {
                    double x = values[0], y = values[1];
                    x = (xDelta += x) * Scale.x + Translate.x;
                    y = (yDelta += y) * Scale.y + Translate.y;
                    list.Add(new Coordinate(x, y));
                }
                return list.ToArray();
            }).ToArray();
            // Cities = raw.Objects.City.Geometries.ToArray();
            Areas = raw.Objects.Area.Geometries.ToArray();
            // Prefs = raw.Objects.Pref.Geometries.ToArray();
        }
        public Coordinate[][] Arcs { get; }
        public (double x, double y) Scale { get; }
        public (double x, double y) Translate { get; }
        // public Geometry<PrefProperties>[] Prefs { get; }
        public Geometry<AreaProperties>[] Areas { get; }
        // public Geometry<CityProperties>[] Cities { get; }

        private Geometry<IProperties>[]? _geometries;

        public Geometry<IProperties>[] Geometries
        {
            get
            {
                if (_geometries != null) return _geometries;
                return _geometries = Areas.Select(x => new Geometry<IProperties>
                    {Type = x.Type, Arcs = x.Arcs, Properties = x.Properties}).ToArray();
                // return _geometries = Prefs.Cast<Geometry<IProperties>>().Concat(Areas.Cast<Geometry<IProperties>>()).Concat(Cities.Cast<Geometry<IProperties>>()).ToArray();
            }
        }
    }
}
