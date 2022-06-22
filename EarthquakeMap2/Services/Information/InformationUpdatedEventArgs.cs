using EarthquakeMap2.Objects;

namespace EarthquakeMap2.Services.Information;

public class InformationUpdatedEventArgs : EventArgs
{
    public InformationUpdatedEventArgs(EarthquakeInformation info) => Information = info;
    public EarthquakeInformation Information { get; }
}
