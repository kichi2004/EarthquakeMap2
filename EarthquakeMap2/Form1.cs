using System.Net;
using System.Xml.Serialization;
using EarthquakeLibrary;
using EarthquakeMap2.Drawing;
using EarthquakeMap2.Objects;
using EarthquakeMap2.Properties;
using EarthquakeMap2.Services.Information;
using EarthquakeMap2.Utilities;
using JmaXmlLib.Scheme.Earthquake;
using KyoshinMonitorLib;
using KyoshinMonitorLib.SkiaImages;
using SkiaSharp;
using ColorConverter = KyoshinMonitorLib.SkiaImages.ColorConverter;

namespace EarthquakeMap2;

public partial class Form1 : Form
{
    public Form1()
    {
        InitializeComponent();
        this.Load += (_, _) => UpdateEarthquake();
        this.SizeChanged += (_, e) => UpdateEarthquake();
        pictureBox1.MouseClick += (_, e) =>
        {
            if ((e.Button & MouseButtons.Right) == 0) return;
            _earthquake = TestInformation();
            UpdateEarthquake();
            // TestEew();
        };
        pictureBox1.MouseWheel += (_, e) =>
        {
            if (e.Delta < 0) Zoom /= 1.2;
            else Zoom *= 1.2;
            UpdateEarthquake();
        };

        Map.Initialize();
        
        MinimumSize = new Size(376, 200);
        var service = new JmaPullInformationService(DateTime.Now);
        service.InformationUpdated += (_, e) =>
        {
            _earthquake = e.Information;
            UpdateEarthquake();
        };
        service.Start();
        service.InvokeForLatest();
    }

    internal static Dictionary<int, Color> Colors = new()
    {
        {1, Color.FromArgb(70, 100, 110)},
        {2, Color.FromArgb(30, 110, 230)},
        {3, Color.FromArgb(0, 200, 200)},
        {4, Color.FromArgb(250, 250, 100)},
        {5, Color.FromArgb(255, 180, 0)},
        {6, Color.FromArgb(255, 120, 0)},
        {7, Color.FromArgb(230, 0, 0)},
        {8, Color.FromArgb(160, 0, 0)},
        {9, Color.FromArgb(150, 0, 150)},
        // 不明: 震度５弱以上と推定
        { -1, Color.FromArgb(230, 0, 0) }
    };
    
    private static ObservationPoint[]? _observationPoints;

    internal static ObservationPoint[] ObservationPoints
    {
        get
        {
            if (_observationPoints is { } points) return points;
            var res = KyoshinPlaceUtil.LoadFromMpkBinary(Resources.KyoshinPoints);
            return _observationPoints = res;
        }
    }

    internal static FontFamily RobotoFont { get; } = new("Roboto");
    private double Zoom = 1;
    private EarthquakeInformation? _earthquake;

    private void ChangePicture(Image image)
    {
        var current = pictureBox1.Image;
        pictureBox1.Image = image;
        current?.Dispose();
    }

    private async void TestEew()
    {
        var dt = new DateTime(2021, 10, 7, 22, 44, 0);
        var webApi = new WebApi();
        // 20211209110522
        var eewInfo = await webApi.GetEewInfo(dt);
        var res = await webApi.GetEstShindoImageData(dt);
        if (res.StatusCode != HttpStatusCode.OK || eewInfo.StatusCode != HttpStatusCode.OK || eewInfo.Data!.Result?.Message == "デーがありません") return;

        using var eewBmp = SKBitmap.Decode(res.Data);
        if (eewBmp == null) return;

        // ChangePicture(Image.FromStream(new MemoryStream(res.Data)));
        // return;

        var points = ObservationPoints.Select(x => new ImageAnalysisResult(x)).ToArray();
        var result = points.ParseScaleFromImage(eewBmp).ToArray();
        var intensityList = result
            .Where(x => x.AnalysisResult != null)
            .Select(x => (x.ObservationPoint.Code, Intensity.FromValue((float) ColorConverter.ConvertToIntensityFromScale(x.AnalysisResult!.Value))))
            .Where(x => x.Item2 >= Intensity.Int1);
        
        var filterIntensity = Intensity.Parse(eewInfo.Data?.CalcintensityString).EnumOrder switch
        {
            5 or 6 => Intensity.Int2,
            7 or 8 => Intensity.Int3,
            9 => Intensity.Int4,
            _ => Intensity.Int1
        };

        var sideInfo = SideInfo.DrawEew(eewInfo.Data!);
        var bmp = Map.DrawMap(intensityList.ToArray(),
            eewInfo.Data?.Location == null ? null : new Coordinate(eewInfo.Data.Location!), pictureBox1.Size,
            MapType.PointIcon, filterIntensity, sideInfo);
        ChangePicture(bmp);
    }

    private static EarthquakeInformation TestInformation()
    {
        var xmlText = File.ReadAllText(@"TestXml\fukushima_sokuhou.xml");

        var xmlSerializer = new XmlSerializer(typeof(typereport));
        var earthquake = EarthquakeInformation.ParseFromXml(xmlSerializer.Deserialize(new StringReader(xmlText)) as typereport)!;

        return earthquake;
    }

    private void UpdateEarthquake()
    {
        if (_earthquake is null) return;
        var filterIntensity = _earthquake.MaxIntensity.EnumOrder switch
        {
            5 or 6 => Intensity.Int2,
            7 or 8 => Intensity.Int3,
            9 => Intensity.Int4,
            _ => Intensity.Int1
        };

        var intensityList = Array.Empty<(string, Intensity)>();
        if (_earthquake.Type == EarthquakeInformationType.SeismicIntensityInformation)
        {
            intensityList = _earthquake.Intensity
                .SelectMany(pref => pref.Areas.Select(x => (x.Code, x.MaxInt)))
                .ToArray();
        }
        else if (_earthquake.Type != EarthquakeInformationType.HypocenterInformation)
        {
            intensityList = _earthquake.Intensity.SelectMany(pref =>
                pref.Areas.SelectMany(area => area.Cities!.SelectMany(city =>
                    city.Stations != null && city.Stations.Any()
                        ? city.Stations.Select(station => (station.Code, station.Int))
                        : new[] { (city.Code, city.MaxInt) }
                ))
            ).ToArray();
        }

        // /* Test */
        // var reader = new StringReader(Resources.AreaPoint);
        // string? line;
        // var list = new List<string>();
        // while ((line = reader.ReadLine()) != null)
        // {
        //     var lineSplit = line.Split(',');
        //     list.Add(lineSplit[0]);
        // }
        //
        // intensityList = list.Where(x => int.Parse(x[..2]) is >= 70 and <= 75).Select(x => (x, Intensity.Int6Minus)).ToArray();
        // /* Test ここまで */


        var sideInfo = SideInfo.DrawInformation(_earthquake);
        var size = pictureBox1.Size;
        size.Width -= 300;
        var picBoxImage = new Bitmap(pictureBox1.Width, pictureBox1.Height);

        var bmp = Map.DrawMap(intensityList.ToArray(),
            _earthquake.Earthquake?.Hypocenter?.Coordinate, pictureBox1.Size,
            _earthquake.Type == EarthquakeInformationType.SeismicIntensityInformation
                ? MapType.AreaFill
                : MapType.PointIcon,
            filterIntensity, sideInfo, Zoom);
        ChangePicture(bmp);
    }
}
