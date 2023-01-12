using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using EarthquakeLibrary;
using JmaXmlLib;
using JmaXmlLib.Scheme.Earthquake;
using EarthquakeLibrary.Core;
using EarthquakeMap2.Utilities;

namespace EarthquakeMap2.Objects;

using LibIntensity = Intensity;

public partial class EarthquakeInformation
{
    [return: NotNullIfNotNull("coordinateStr")]
    private static (Coordinate?, int?) ParseCoordinate(string? coordinateStr)
    {
        if (coordinateStr == null)
            return (null, null);
        var coordinate = new Coordinate(double.Parse(coordinateStr[5..11]), double.Parse(coordinateStr[..5]));
        if (coordinateStr.Length == 11) return (coordinate, null);
        var depth = coordinateStr[12..^2];
        return (coordinate, int.Parse(depth) / 100);
    }

    private static readonly Dictionary<string, (ReadOnlyCollection<IntensityPref> intensity, DateTime dt)> LatestIntensityDict = new();

    public static EarthquakeInformation? ParseFromXml(typereport? xmlData)
    {
        var xmlBody = xmlData.Any.Deserialize<typeBody>();
        EarthquakeInformationType? type = xmlData.Control.Title switch
        {
            "震度速報" => EarthquakeInformationType.SeismicIntensityInformation,
            "震源に関する情報" => EarthquakeInformationType.HypocenterInformation,
            "震源・震度に関する情報" when xmlBody.Intensity != null
                => EarthquakeInformationType.HypocenterAndSeismicIntensityInformation,
            _ => null
        };
        if (type == null) return null;
        var intensity = xmlBody.Intensity == null
            ? new ReadOnlyCollection<IntensityPref>(new ImmutableArray<IntensityPref>())
            : SelectReadOnly(xmlBody.Intensity.Observation.Pref, ToIntensityPref);

        var existIntensity = LatestIntensityDict.TryGetValue(xmlData.Head.EventID, out var lt);


        if (existIntensity)
        {
            if (!intensity.Any())
                intensity = lt.intensity;
            else if (lt.dt > xmlData.Head.ReportDateTime)
                LatestIntensityDict[xmlData.Head.EventID] = (intensity, xmlData.Head.ReportDateTime);
        }

        if (type is EarthquakeInformationType.SeismicIntensityInformation)
            return new EarthquakeInformation
            {
                Intensity = intensity,
                Earthquake = new Earthquake(xmlData.Head.TargetDateTime ?? DateTime.Now),
                ForecastCommentCodes = Array.AsReadOnly(xmlBody.Comments?.ForecastComment == null
                    ? Array.Empty<string>()
                    : xmlBody.Comments.ForecastComment.Code.Split(' ')),
                MaxIntensity = LibIntensity.Parse(xmlBody.Intensity.Observation.MaxInt),
                Type = type.Value
            };

        var earthquakeData = xmlBody.Earthquake[0];
        var hypocenterData = earthquakeData.Hypocenter.Area;
        var (coordinate, depth) = ParseCoordinate(hypocenterData.Coordinate[0].Value);
        var earthquake = new Earthquake(
            earthquakeData.OriginTime,
            new Hypocenter(hypocenterData.Code.Value, hypocenterData.Name, hypocenterData.DetailedName,
                hypocenterData.DetailedCode?.Value, coordinate, depth),
            StringUtil.ToHankaku(earthquakeData.Magnitude[0].description[1..])
        );

        return new EarthquakeInformation
        {
            Intensity = intensity,
            Earthquake = earthquake,
            ForecastCommentCodes = Array.AsReadOnly(xmlBody.Comments.ForecastComment.Code.Split(' ')),
            MaxIntensity = LibIntensity.Parse(xmlBody.Intensity.Observation.MaxInt),
            Type = type.Value
        };
    }

    private static ReadOnlyCollection<TResult> SelectReadOnly<T, TResult>(IEnumerable<T> source, Func<T, TResult> selector) =>
        new(source.Select(selector).ToImmutableList());

    private static IntensityPref ToIntensityPref(typeIntensityPref pref)
        => new(pref.Name, pref.Code, IntToIntensity(pref.MaxInt),
            SelectReadOnly(pref.Area, ToIntensityArea));

    private static IntensityArea ToIntensityArea(typeIntensityArea area)
        => new(area.Name, area.Code, IntToIntensity(area.MaxInt),
            area.City == null ? null : SelectReadOnly(area.City, ToIntensityCity));

    private static IntensityCity ToIntensityCity(typeIntensityCity city)
        => new(city.Name, city.Code, IntToIntensity(city.MaxInt),
            SelectReadOnly(city.IntensityStation, ToIntensityStation));

    private static IntensityStation ToIntensityStation(typeIntensityStation station)
        => new(station.Name, station.Code, IntToIntensity(station.Int));

    private static Intensity IntToIntensity(string? intensity) =>
        intensity is null or "震度５弱以上未入電" ? LibIntensity.Unknown : LibIntensity.Parse(intensity);
}
