using System.Collections.ObjectModel;
using EarthquakeLibrary;

namespace EarthquakeMap2.Objects;

public partial class EarthquakeInformation
{

    public EarthquakeInformationType Type { get; init; }
    public Intensity MaxIntensity { get; init; }
    public ReadOnlyCollection<IntensityPref> Intensity { get; init; }
    public ReadOnlyCollection<string> ForecastCommentCodes { get; init; }
    public Earthquake Earthquake { get; init; }
}
