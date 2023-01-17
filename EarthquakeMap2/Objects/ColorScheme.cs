using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EarthquakeMap2.Objects;

internal record ColorScheme(
    Dictionary<int, (Color background, Color foreground, Color? edge)> IntensityColors,
    Color Sea,
    Color Land,
    Color Line
);
