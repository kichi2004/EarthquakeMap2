using System.Xml.Serialization;
using EarthquakeLibrary;
using EarthquakeMap2.Drawing;
using EarthquakeMap2.Objects;
using EarthquakeMap2.Properties;
using EarthquakeMap2.Services.Eew;
using EarthquakeMap2.Services.Information;
using EarthquakeMap2.Utilities;
using JmaXmlLib.Scheme.Earthquake;
using KyoshinMonitorLib;
using LibEew = EewLibrary.EEW;
using ColorConverter = KyoshinMonitorLib.SkiaImages.ColorConverter;

namespace EarthquakeMap2;

public partial class Form1 : Form
{
    public Form1()
    {
        InitializeComponent();
        this.Load += (_, _) => UpdateEarthquake();
        this.SizeChanged += (_, e) => UpdateEarthquake();

        Initialize();
    }

    private static readonly (string, string)[] TestInformationList =
    {
        ("震源震度 福島県沖 2016/11/21", "fukushima_1611_shindo.xml"),
        ("震度速報 福島県沖 2022/03/17", "fukushima_2203_sokuhou.xml"),
        ("震源震度 福島県沖 2022/03/17", "fukushima_2203_shindo.xml"),
        ("震度速報 福島県沖 2022/05/22", "fukushima_2205_sokuhou.xml"),
        ("震源震度 熊本県熊本地方 2016/04/16", "kumamoto_shindo.xml"),
    };

    private static readonly (string, DateTime)[] TestEewList =
    {
        ("石川県能登地方 2022/06/19", new DateTime(2022, 6, 19, 15, 8, 15)),
        ("石川県能登地方 2023/01/06", new DateTime(2023, 1, 6, 13, 44, 11)),
    };


    public async void Initialize()
    {
        // pictureBox1.MouseClick += (_, e) =>
        // {
        //     if ((e.Button & MouseButtons.Right) == 0) return;
        //     _earthquake = TestInformation();
        //     UpdateEarthquake();
        // };
        pictureBox1.MouseWheel += (_, e) =>
        {
            if (e.Delta < 0) _zoom /= 1.2;
            else _zoom *= 1.2;

            if (_isLastEew) UpdateEew();
            else UpdateEarthquake();
        };

        Map.Initialize();

        MinimumSize = new Size(376, 200);
        var infoService = new JmaPullInformationService(DateTime.Now);
        infoService.InformationUpdated += (_, e) =>
        {
            _earthquake = e.Information;
            _isLastEew = false;
            _zoom = 1;
            UpdateEarthquake();
        };
        infoService.Start();
        infoService.InvokeForLatest();

        var eewService = new KmoniEewService();
        eewService.EewUpdatedForFirst += (s, e) =>
        {
            _zoom = 1;
            OnEew(s, e);
        };
        eewService.EewUpdatedForContinue += OnEew;
        await eewService.Start();

        foreach (var (text, file) in TestInformationList)
        {
            toolStripInformationTest.DropDownItems.Add(text, null, (_, _) =>
            {
                _earthquake = TestInformation(file);
                UpdateEarthquake();
            });
        }

        toolStripInformationTestEew.DropDownItems.Add("テスト終了", null, (_, _) => eewService.UpdateTime(null));
        foreach (var (text, dt) in TestEewList)
        {
            toolStripInformationTestEew.DropDownItems.Add(text, null, (_, _) => eewService.UpdateTime(dt));
        }
        // eewService.UpdateTime(new DateTime(2023, 1, 6, 13, 44, 12));

        foreach (var (key, value) in ColorSchemes)
        {
            colorSchemeToolStrip.DropDownItems.Add(key, null, (_, _) =>
            {
                CurrentScheme = value;

                if (_isLastEew) UpdateEew();
                else UpdateEarthquake();
            });
        }
    }

    internal static Dictionary<string, ColorScheme> ColorSchemes = new()
    {
        {
            "Kiwi Monitor カラースキーム 第3版", new(new Dictionary<int, (Color, Color, Color?)>
            {
                { 1, (Color.FromArgb(60, 90, 130), Color.FromArgb(240, 255, 255, 255), null) },
                { 2, (Color.FromArgb(30, 130, 230), Color.FromArgb(240, 255, 255, 255), null) },
                { 3, (Color.FromArgb(120, 230, 220), Color.FromArgb(240, 0, 0, 00), null) },
                { 4, (Color.FromArgb(255, 255, 150), Color.FromArgb(240, 0, 0, 00), null) },
                { 5, (Color.FromArgb(255, 210, 10), Color.FromArgb(240, 0, 0, 00), null) },
                { 6, (Color.FromArgb(255, 150, 0), Color.FromArgb(240, 0, 0, 00), null) },
                { 7, (Color.FromArgb(240, 50, 0), Color.FromArgb(240, 255, 255, 255), null) },
                { 8, (Color.FromArgb(190, 0, 0), Color.FromArgb(240, 255, 255, 255), null) },
                { 9, (Color.FromArgb(140, 0, 40), Color.FromArgb(240, 255, 255, 255), null) },
                // 不明: 震度５弱以上と推定
                { -1, (Color.FromArgb(255, 210, 10), Color.FromArgb(240, 0, 0, 00), null) }
            }, Color.FromArgb(37, 37, 50), Color.FromArgb(50, 50, 50), Color.FromArgb(120, 120, 120))
        },
        {
            "気象庁", new(new Dictionary<int, (Color, Color, Color?)>
            {
                { 1, (Color.FromArgb(242, 242, 255), Color.FromArgb(240, 0, 0, 00), Color.Black) },
                { 2, (Color.FromArgb(0, 170, 255), Color.FromArgb(240, 0, 0, 00), Color.Black) },
                { 3, (Color.FromArgb(0, 65, 255), Color.FromArgb(240, 255, 255, 255), Color.Black) },
                { 4, (Color.FromArgb(250, 230, 150), Color.FromArgb(240, 0, 0, 00), Color.Black) },
                { 5, (Color.FromArgb(255, 230, 0), Color.FromArgb(240, 0, 0, 00), Color.Black) },
                { 6, (Color.FromArgb(255, 153, 0), Color.FromArgb(240, 0, 0, 00), Color.Black) },
                { 7, (Color.FromArgb(255, 40, 0), Color.FromArgb(240, 255, 255, 255), Color.Black) },
                { 8, (Color.FromArgb(165, 0, 33), Color.FromArgb(240, 255, 255, 255), Color.Black) },
                { 9, (Color.FromArgb(180, 0, 104), Color.FromArgb(240, 255, 255, 255), Color.Black) },
                // 不明: 震度５弱以上と推定
                { -1, (Color.FromArgb(255, 230, 0), Color.FromArgb(240, 0, 0, 00), Color.Black) }
            }, Color.LightSkyBlue, Color.WhiteSmoke, Color.Black)
        }
    };


    internal static ColorScheme CurrentScheme { get; private set; } = ColorSchemes["Kiwi Monitor カラースキーム 第3版"];
    
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
    private double _zoom = 1;
    private bool _isLastEew = false;
    private EarthquakeInformation? _earthquake;
    private KmoniEew? _eew;

    private void ChangePicture(Image image)
    {
        var current = pictureBox1.Image;
        pictureBox1.Image = image;
        current?.Dispose();
    }

    private void OnEew(object? _, EewUpdatedEventArgs<LibEew> e)
    {
        var eewRaw = e.Eew;
        if (eewRaw is not KmoniEew eew) return;
        
            _zoom = 1;
        _eew = eew;
        _isLastEew = true;

        UpdateEew();

        // result.Where(x => x.AnalysisResult != null)
        // .Select(x => (x.ObservationPoint.Code,
        //         Intensity.FromValue(
        //             (float)ColorConverter.ConvertToIntensityFromScale(x.AnalysisResult!.Value))))
        //     .Where(x => x.Item2 >= Intensity.Int1)
    }

    // private async void TestEew()
    // {
    //     var dt = new DateTime(2021, 10, 7, 22, 44, 0);
    //     var webApi = new WebApi();
    //     // 20211209110522
    //     var eewInfo = await webApi.GetEewInfo(dt);
    //     var res = await webApi.GetEstShindoImageData(dt);
    //     if (res.StatusCode != HttpStatusCode.OK || eewInfo.StatusCode != HttpStatusCode.OK || eewInfo.Data!.Result?.Message == "データがありません") return;
    //
    //     using var eewBmp = SKBitmap.Decode(res.Data);
    //     if (eewBmp == null) return;
    //
    //     // ChangePicture(Image.FromStream(new MemoryStream(res.Data)));
    //     // return;
    //
    //     var points = ObservationPoints.Select(x => new ImageAnalysisResult(x)).ToArray();
    //     var result = points.ParseScaleFromImage(eewBmp).ToArray();
    //     var intensityList = result
    //         .Where(x => x.AnalysisResult != null)
    //         .Select(x => (x.ObservationPoint.Code, Intensity.FromValue((float) ColorConverter.ConvertToIntensityFromScale(x.AnalysisResult!.Value))))
    //         .Where(x => x.Item2 >= Intensity.Int1);
    //     
    //     var filterIntensity = Intensity.Parse(eewInfo.Data?.CalcintensityString).EnumOrder switch
    //     {
    //         5 or 6 => Intensity.Int2,
    //         7 or 8 => Intensity.Int3,
    //         9 => Intensity.Int4,
    //         _ => Intensity.Int1
    //     };
    //
    //     var sideInfo = SideInfo.DrawEew(eewInf);
    //     var bmp = Map.DrawMap(intensityList.ToArray(),
    //         eewInfo.Data?.Location == null ? null : new Coordinate(eewInfo.Data.Location!), pictureBox1.Size,
    //         MapType.PointIcon, filterIntensity, sideInfo);
    //     ChangePicture(bmp);
    // }

    private static readonly string[] TestFileNameList =
    {
        "fukushima_2203_sokuhou.xml",
        "fukushima_2203_shindo.xml",
        "fukushima_2203_shindo_max7.xml",
    };

    private static int _testInformationIndex;
    private static EarthquakeInformation TestInformation(string fileName)
    {
        var xmlText = File.ReadAllText($@"TestXml\{fileName}");
        // _testInformationIndex %= TestFileNameList.Length;

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
                    city.Stations is { Count: > 0 }
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
        
        var bmp = Map.DrawMap(intensityList.ToArray(),
            _earthquake.Earthquake?.Hypocenter?.Coordinate, pictureBox1.Size,
            _earthquake.Type == EarthquakeInformationType.SeismicIntensityInformation
                ? MapType.AreaFill
                : MapType.PointIcon,
            filterIntensity, sideInfo, _zoom);
        ChangePicture(bmp);
    }

    private void UpdateEew()
    {
        var intensityList = _eew.EstimationResults
            .Where(x => x.AnalysisResult != null)
            .Select(x => (x.ObservationPoint.Code, Intensity.FromValue((float)ColorConverter.ConvertToIntensityFromScale(x.AnalysisResult!.Value))))
            .Where(x => x.Item2 >= Intensity.Int1);

        var filterIntensity = _eew.MaxInt.EnumOrder switch
        {
            5 or 6 => Intensity.Int2,
            7 or 8 => Intensity.Int3,
            9 => Intensity.Int4,
            _ => Intensity.Int1
        };

        var sideInfo = SideInfo.DrawEew(_eew);
        var bmp = Map.DrawMap(intensityList.ToArray(), _eew.Location, pictureBox1.Size, MapType.PointIcon,
            filterIntensity, sideInfo, _zoom);
        ChangePicture(bmp);

        // result.Where(x => x.AnalysisResult != null)
        // .Select(x => (x.ObservationPoint.Code,
        //         Intensity.FromValue(
        //             (float)ColorConverter.ConvertToIntensityFromScale(x.AnalysisResult!.Value))))
        //     .Where(x => x.Item2 >= Intensity.Int1)

    }
}
