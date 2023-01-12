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
        // Feed が更新されるのは毎分 20 秒
        if (!_enabled || dt.Second != 20) return;
        await Update();
    }

    private static async Task<SyndicationFeed> GetFeedFromUrl(string url = XmlFeedUrl)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        var response = await HttpClient.SendAsync(request);

        using var reader = XmlReader.Create(await response.Content.ReadAsStreamAsync());
        return SyndicationFeed.Load(reader);
    }

    private static bool IsEarthquakeTitle(string title) => title is "震度速報" or "震源に関する情報" or "震源・震度に関する情報";

    private static EarthquakeInformation? ParseFromXml(string xml) =>
        Serializer.Deserialize(new StringReader(xml)) is not typereport xmlData
            ? null
            : EarthquakeInformation.ParseFromXml(xmlData);

    private async Task Update(bool ignoreEnqueue = false)
    {
        Console.WriteLine("Information Update Check...");
        var feed = await GetFeedFromUrl();
        var feedItems = feed.Items.Where(x => !_feedIds.Contains(x.Id)).OrderBy(x => x.LastUpdatedTime).ToArray();
        Console.WriteLine($"New Items: {feedItems.Length}");
        foreach (var feedItem in feedItems)
        {
            _feedIds.Add(feedItem.Id);
            if (!ignoreEnqueue && IsEarthquakeTitle(feedItem.Title.Text))
                _queue.Enqueue(feedItem.Links[0].Uri.AbsoluteUri);
        }

        while (_queue.Count > 1) await ProcessOnce(invoke: false);
        await ProcessOnce();
    }

    private async Task<bool> ProcessOnce(string? url = null, bool invoke = true)
    {
        if (url == null)
        {
            if (!_queue.Any()) return false;
            url = _queue.Dequeue();
        }
        var xmlText = await HttpClient.GetStringAsync(url);
        var earthquake = ParseFromXml(xmlText);
        if (earthquake == null) return false;

        if (invoke)
        {
            await Task.Run(() => this.InformationUpdated?.Invoke(this, new InformationUpdatedEventArgs(earthquake)));
        }

        return true;
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

    public void UpdateTime(DateTime dt)
    {
        Timer.UpdateTime(dt);
    }

    public async void InvokeForLatest()
    {
        var feed = await GetFeedFromUrl();
        var items = feed.Items.Where(x => IsEarthquakeTitle(x.Title.Text)).ToArray();
        foreach (var item in items)
        {
            if (await ProcessOnce(item.Links[0].Uri.AbsoluteUri))
            {
                return;
            }
        }

        feed = await GetFeedFromUrl(XmlLongFeedUrl);
        items = feed.Items.Where(x => IsEarthquakeTitle(x.Title.Text)).ToArray();
        foreach (var item in items)
        {
            if (await ProcessOnce(item.Links[0].Uri.AbsoluteUri))
            {
                return;
            }
        }
    }

    public event EventHandler<InformationUpdatedEventArgs>? InformationUpdated;
}
