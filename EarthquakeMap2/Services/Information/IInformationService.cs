using EarthquakeMap2.Objects;

namespace EarthquakeMap2.Services.Information;

public interface IInformationService : IDisposable
{
    public void Start();
    public void UpdateTime(DateTime dateTime);
    public void InvokeForLatest();
    public event EventHandler<InformationUpdatedEventArgs> InformationUpdated;
}
