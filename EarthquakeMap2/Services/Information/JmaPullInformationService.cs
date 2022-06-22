using System.ServiceModel.Syndication;
using System.Xml;
using System.Xml.Serialization;
using EarthquakeMap2.Objects;
using JmaXmlLib.Scheme.Earthquake;
using KyoshinMonitorLib.Timers;

namespace EarthquakeMap2.Services.Information;

public class JmaPullInformationService : IInformationService
{
    public JmaPullInformationService(DateTime now)
    {
        Timer = new SecondBasedTimer
        {
            BlockingMode = false,
            Accuracy = TimeSpan.FromMilliseconds(100)
        };
        Timer.Elapsed += TimerOnElapsed;
        Timer.Start(now);
    }

    private const string XmlFeedUrl = "http://www.data.jma.go.jp/developer/xml/feed/eqvol.xml";
    private const string XmlLongFeedUrl = "http://www.data.jma.go.jp/developer/xml/feed/eqvol_l.xml";
    private static readonly XmlSerializer Serializer = new(typeof(typereport));
    private static readonly HttpClient HttpClient = new();
    private readonly HashSet<string> _feedIds = new();
    private readonly Queue<string> _queue = new();
    private bool _enabled;

    private async Task TimerOnElapsed(DateTime dt)
    {
        if (!_enabled || dt.Second != 20) return;
        await Update();
    }

    private static async Task<SyndicationFeed> GetFeedFromUrl(string url)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, XmlFeedUrl);
        var response = await HttpClient.SendAsync(request);

        using var reader = XmlReader.Create(await response.Content.ReadAsStreamAsync());
        return SyndicationFeed.Load(reader);
    }

    private static bool IsEarthquakeTitle(string title) => title is "震度速報" or "震源に関する情報" or "震源・震度に関する情報";

    private EarthquakeInformation? ParseFromXml(string xml) =>
        Serializer.Deserialize(new StringReader(xml)) is not typereport xmlData
            ? null
            : EarthquakeInformation.ParseFromXml(xmlData);

    private async Task Update(bool ignoreEnqueue = false)
    {
        var feed = await GetFeedFromUrl(XmlFeedUrl);
        var feedItems = feed.Items.Where(x => !_feedIds.Contains(x.Id)).OrderBy(x => x.LastUpdatedTime);
        foreach (var feedItem in feedItems)
        {
            _feedIds.Add(feedItem.Id);
            if (!ignoreEnqueue && IsEarthquakeTitle(feedItem.Title.Text)) _queue.Enqueue(feedItem.Links[0].Uri.AbsoluteUri);
        }

        await ProcessOnce();
    }

    private async Task ProcessOnce(string? url = null)
    {
        if (url == null)
        {
            if (!_queue.Any()) return;
            url = _queue.Dequeue();
        }
        var xmlText = await HttpClient.GetStringAsync(url);
        var earthquake = ParseFromXml(xmlText);
        if (earthquake == null) return;

        await Task.Run(() => this.InformationUpdated?.Invoke(this, new InformationUpdatedEventArgs(earthquake)));
    }

    public SecondBasedTimer Timer;

    public void Dispose()
    {
        throw new NotImplementedException();
    }

    public void Start()
    {
        _enabled = true;
        _ = Update(true);
    }

    public void UpdateTime(DateTime _)
    {
        throw new NotImplementedException();
    }

    public async void InvokeForLatest()
    {
        var feed = await GetFeedFromUrl(XmlFeedUrl);
        var item = feed.Items.FirstOrDefault(x => IsEarthquakeTitle(x.Title.Text));
        if (item != null) await ProcessOnce(item.Links[0].Uri.AbsoluteUri);
        feed = await GetFeedFromUrl(XmlLongFeedUrl);
        item = feed.Items.FirstOrDefault(x => IsEarthquakeTitle(x.Title.Text));
        if (item != null) await ProcessOnce(item.Links[0].Uri.AbsoluteUri);
    }

    public event EventHandler<InformationUpdatedEventArgs>? InformationUpdated;
}
