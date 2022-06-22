using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Text;
using System.Text.Json;
using EarthquakeLibrary;
using EarthquakeMap2.Json;
using EarthquakeMap2.Objects;
using EarthquakeMap2.Properties;

namespace EarthquakeMap2.Drawing;

public enum MapType
{
    PointIcon,
    AreaIcon,
    AreaFill,
}

public static class Map
{
    private static TopoJsonData? TopoJsonData { get; set; }
    private static Dictionary<string, Coordinate> Coordinates { get; } = new();
    private static Dictionary<string, string> AreaNames { get; } = new();

    internal static void Initialize()
    {
        var topoJson = Encoding.UTF8.GetString(Resources.AreaTopo);
        var topoJsonData = JsonSerializer.Deserialize<TopoJsonRaw>(topoJson);
        TopoJsonData = new TopoJsonData(topoJsonData ?? throw new Exception());

        var stationsJson = Encoding.UTF8.GetString(Resources.Stations);
        var stationsJsonData = JsonSerializer.Deserialize<StationDataItem[]>(stationsJson);
        foreach (var item in stationsJsonData ?? throw new Exception())
            Coordinates[item.Code] = new Coordinate(double.Parse(item.Longitude), double.Parse(item.Latitude));

        var reader = new StringReader(Resources.AreaPoint);
        string? line;
        while ((line = reader.ReadLine()) != null)
        {
            var lineSplit = line.Split(',');
            Coordinates[lineSplit[0]] = new Coordinate(double.Parse(lineSplit[3]), double.Parse(lineSplit[2]));
            AreaNames[lineSplit[0]] = lineSplit[1];
        }

        reader.Dispose();

        reader = new StringReader(Resources.CityPoint);
        while ((line = reader.ReadLine()) != null)
        {
            var lineSplit = line.Split(',');
            Coordinates[lineSplit[0]] = new Coordinate(double.Parse(lineSplit[3]), double.Parse(lineSplit[2]));
        }

        reader.Dispose();

        reader = new StringReader(Resources.AreaName);
        while ((line = reader.ReadLine()) != null)
        {
            var lineSplit = line.Split(',');
            AreaNames[lineSplit[0]] = lineSplit[1];
        }

        reader.Dispose();

        foreach (var observationPoint in Form1.ObservationPoints)
        {
            Coordinates[observationPoint.Code] = new Coordinate(observationPoint.Location);
        }
    }

    public static Bitmap DrawMap((string code, Intensity intensity)[] intensityList, Coordinate? epicenter,
        Size size, MapType mapType, Intensity filterIntensity, Bitmap? sideInfo = null, double zoom = 1)
    {
        if (TopoJsonData == null) throw new Exception("Not Initialized");

        var intensityDict = intensityList.ToDictionary(x => x.code, x => x.intensity);
        var maxIntensity = intensityList.Max(x => x.intensity)!;

        var geometries = new Dictionary<string, List<Coordinate[]>>();

        double geoMinLon = double.MaxValue,
            geoMaxLon = double.MinValue,
            geoMinLat = double.MaxValue,
            geoMaxLat = double.MinValue;
        // set bounding box
        // × 62.247 (1933 x 1339)
        foreach (var geometry in TopoJsonData.Geometries)
        {
            if (geometry?.Properties?.Code is null) continue;

            var coordinates = new List<Coordinate>();
            foreach (var idx in geometry.Arcs[0].Select(arc =>
                         arc >= 0 ? TopoJsonData.Arcs[arc] : TopoJsonData.Arcs[~arc].Reverse()))
            {
                coordinates.AddRange(idx);
            }

            var code = geometry.Properties.Code;
            if (!geometries.ContainsKey(code))
                geometries.Add(code, new List<Coordinate[]>());
            geometries[code].Add(coordinates.ToArray());
            foreach (var (lon, lat) in coordinates)
            {
                geoMinLon = Math.Min(geoMinLon, lon);
                geoMaxLon = Math.Max(geoMaxLon, lon);
                geoMinLat = Math.Min(geoMinLat, lat);
                geoMaxLat = Math.Max(geoMaxLat, lat);
            }
        }

        double minLon = double.MaxValue, maxLon = double.MinValue, minLat = double.MaxValue, maxLat = double.MinValue;

        if (epicenter is { } epi)
        {
            var (lon, lat) = epi;
            minLon = Math.Min(minLon, lon);
            maxLon = Math.Max(maxLon, lon);
            minLat = Math.Min(minLat, lat);
            maxLat = Math.Max(maxLat, lat);
        }

        foreach (var (k, v) in intensityDict)
        {
            if (v < filterIntensity) continue;
            if (mapType == MapType.AreaFill)
            {
                foreach (var (lon, lat) in geometries[k].SelectMany(x => x))
                {
                    minLon = Math.Min(minLon, lon);
                    maxLon = Math.Max(maxLon, lon);
                    minLat = Math.Min(minLat, lat);
                    maxLat = Math.Max(maxLat, lat);
                }
            }
            else
            {
                if (!Coordinates.ContainsKey(k)) continue;
                var (lon, lat) = Coordinates[k];
                minLon = Math.Min(minLon, lon);
                maxLon = Math.Max(maxLon, lon);
                minLat = Math.Min(minLat, lat);
                maxLat = Math.Max(maxLat, lat);
            }
        }

        if (double.IsInfinity(minLon)) (minLon, maxLon, minLat, maxLat) = (geoMinLon, geoMaxLon, geoMinLat, geoMaxLat);

        var iconSize = mapType == MapType.AreaIcon ? 32 : 18;
        var lonDiff = maxLon - minLon;
        var latDiff = maxLat - minLat;
        var zoomRateOffset = mapType == MapType.AreaFill ? 40.0 : iconSize * 4;
        var zoomRate = Math.Min((size.Width - zoomRateOffset) / lonDiff, (size.Height - zoomRateOffset) / latDiff);
        zoomRate *= zoom;
        zoomRate = Math.Max(Math.Min(zoomRate, 500), 10);
        var width = size.Width;
        var height = size.Height;
        var offsetX = (int) ((width - lonDiff * zoomRate) / 2);
        var offsetY = (int) ((height - latDiff * zoomRate) / 2);

        PointF ConvertCoordinates(Coordinate coordinates) => new(
            (float) ((coordinates.Longitude - minLon) * zoomRate + 20 + offsetX),
            (float) (height - ((coordinates.Latitude - minLat) * zoomRate + 20 + offsetY))
        );

        var bmp = new Bitmap(width, height);
        using var g = Graphics.FromImage(bmp);
        g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.FillRectangle(new SolidBrush(Color.FromArgb(37, 37, 50)), 0, 0, width, height);

        var pen = new Pen(Color.FromArgb(120, 120, 120), 2)
        {
            LineJoin = LineJoin.Round,
            StartCap = LineCap.Round,
            EndCap = LineCap.Round
        };
        var pointsDict = new Dictionary<string, PointF[][]>();

        foreach (var (code, coordinatesList) in geometries)
        {
            var pointsList = coordinatesList
                .Select(x => x.Select(ConvertCoordinates).ToArray())
                .Where(x => x.Any(a => a.X >= 0 && a.X < width && a.Y >= 0 && a.Y < height))
                .ToArray();
            if (!pointsList.Any()) continue;
            pointsDict[code] = pointsList;
        }

        var drawList = intensityDict.OrderBy(x => x.Value)
            .Where(x => Coordinates.ContainsKey(x.Key))
            .Select(kvp =>
            {
                var (code, intensity) = kvp;
                var coordinate = Coordinates[code];
                return (code, intensity, point: ConvertCoordinates(coordinate));
            }).ToArray();

        foreach (var pointFs in pointsDict.SelectMany(kvp => kvp.Value))
            g.FillPolygon(new SolidBrush(Color.FromArgb(50, 50, 50)), pointFs);

        if (mapType == MapType.AreaFill)
        {
            foreach (var (code, intensity, _) in drawList)
            {
                if (!pointsDict.ContainsKey(code)) continue;
                foreach (var pointFs in pointsDict[code])
                    g.FillPolygon(new SolidBrush(Form1.Colors[intensity.EnumOrder]), pointFs);
            }
        }

        foreach (var pointFs in pointsDict.SelectMany(kvp => kvp.Value))
            g.DrawPolygon(pen, pointFs);

        foreach (var (code, intensity, point) in drawList)
        {
            if (mapType is MapType.AreaFill or MapType.AreaIcon)
                DrawAreaIntensityIcon(g, intensity, point,
                    intensity.EnumOrder >=  maxIntensity.EnumOrder - 1 ? AreaNames[code] : null);
            else
                DrawPointIntensityIcon(g, intensity, point, intensity < filterIntensity);
        }

        if (epicenter is { } e)
        {
            const int epicenterSize = 30;
            var epicenterImage = Image.FromFile("epicenter.png");
            var ePoint = ConvertCoordinates(e);
            g.DrawImage(epicenterImage, ePoint.X - epicenterSize / 2f, ePoint.Y - epicenterSize / 2f, epicenterSize,
                epicenterSize);
        }

        if (sideInfo is { }) g.DrawImage(sideInfo, 8, 10);

        return bmp;
    }

    private static void DrawAreaIntensityIcon(Graphics g, Intensity intensity, PointF centerPoint, string? areaName = null)
    {
        const int iconSize = 22;
        var topX = centerPoint.X - iconSize / 2f;
        var topY = centerPoint.Y - iconSize / 2f;

        g.DrawRectangle(new Pen(Color.FromArgb(222, 222, 222), 2), topX, topY, iconSize, iconSize);
        g.FillRectangle(new SolidBrush(Form1.Colors[intensity.EnumOrder]), topX, topY, iconSize, iconSize);
        DrawIntensityIconString(g, intensity, new PointF(topX, topY), iconSize, 
            2, -1, 21, 3, 9, -3, 18, 13, 7, 7, 2);

        if (areaName is null) return;
        using var gPath = new GraphicsPath();
        gPath.AddString(areaName, new FontFamily("Koruri Regular"), (int) FontStyle.Bold, 
            20, new PointF(topX + iconSize, topY - 1), StringFormat.GenericDefault);

        g.FillPath(Brushes.White, gPath);
        g.DrawPath(Pens.Black, gPath);
    }

    private static void DrawPointIntensityIcon(Graphics g, Intensity intensity, PointF centerPoint,
        bool noChars = false)
    {
        var iconSize = noChars ? 8 : 18;
        var topX = centerPoint.X - iconSize / 2f;
        var topY = centerPoint.Y - iconSize / 2f;
        if (noChars)
        {
            g.FillEllipse(new SolidBrush(Form1.Colors[intensity.EnumOrder]), topX, topY, iconSize, iconSize);
            return;
        }

        g.FillEllipse(new SolidBrush(Form1.Colors[intensity.EnumOrder]), topX, topY, iconSize, iconSize);
        DrawIntensityIconString(g, intensity, new PointF(topX, topY), iconSize, 2, 1, 16, 2, 9, 0, 11, 10, 7, 5, 2);
    }

    private static void DrawIntensityIconString(Graphics g, Intensity intensity, PointF topPoint,
        int iconSize, int offsetX1, int offsetY1, int fontSize1,
        int offsetDiff, int offsetX2, int offsetY2, int fontSize2,
        int offsetX3, int offsetY3, int length3, int width3)
    {
        var font = new Font(Form1.RobotoFont, fontSize1, FontStyle.Bold | FontStyle.Italic, GraphicsUnit.Pixel);
        var font2 = new Font(Form1.RobotoFont, fontSize2, FontStyle.Bold, GraphicsUnit.Pixel);
        var intensityPlusMinus = intensity.EnumOrder switch
        {
            5 or 7 => "-",
            6 or 8 => "+",
            _ => null
        };
        if (intensity == Intensity.Unknown) return;
        var textColor = intensity.EnumOrder is >= 3 and <= 6 ? Color.FromArgb(30, 30, 30) : Color.White;
        if (intensityPlusMinus is { } a)
        {
            var offset = offsetX1 - offsetDiff;
            g.DrawString(intensity.ShortString[0].ToString(), font, new SolidBrush(textColor),
                new RectangleF(topPoint.X + offset, topPoint.Y + offsetY1, iconSize - offset, iconSize - offsetY1));

            if (a == "-")
            {
                g.DrawLine(new Pen(textColor, width3), topPoint.X + offsetX3, topPoint.Y + offsetY3,
                    topPoint.X + offsetX3 + length3, topPoint.Y + offsetY3);
            }
            else
            {
                g.DrawString(a, font2, new SolidBrush(textColor),
                    new RectangleF(topPoint.X + offsetX2, topPoint.Y + offsetY2, iconSize - offsetX2,
                        iconSize - offsetY2));
            }
        }
        else
        {
            g.DrawString(intensity.ShortString, font, new SolidBrush(textColor),
                new RectangleF(topPoint.X + offsetX1, topPoint.Y + offsetY1, iconSize - offsetX1, iconSize - offsetY1));
        }
    }
}