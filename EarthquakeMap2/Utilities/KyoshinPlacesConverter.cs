using KyoshinMonitorLib;

namespace EarthquakeMap2.Utilities;

public static class KyoshinPlacesConverter
{
    private record SpotData(
        bool Enabled,
        bool Suspended,
        ObservationPointType DataKind,
        string SpotCode,
        string SpotName,
        string NameAlpha,
        int AreaCode,
        int AreaIndex,
        string Prefecture,
        int PrefCode,
        int PrefIndex,
        float Latitude,
        float Longitude,
        float Elevation,
        int Depth,
        int X,
        int Y,
        int OffsetX,
        int OffsetY,
        int Trace50
    );

    public static ObservationPoint[] Convert(string knetPath, string kiknetPath, string spotListPath)
    {
        var kyoshin = new Dictionary<string, (float lat, float lon)>();

        var linesKyoshin = File.ReadAllLines(knetPath).Concat(File.ReadAllLines(kiknetPath));
        foreach (var line in linesKyoshin)
        {
            var s = line.Split(',');
            var lat = float.Parse(s[9]);
            var lon = float.Parse(s[10]);
            kyoshin[s[0]] = (lat, lon);
        }


        var linesEq = File.ReadAllLines(spotListPath);
        var observationPoints = new List<ObservationPoint>();

        foreach (var line in linesEq[1..])
        {
            var s = line.Split(',');
            var kind = s[2] switch
            {
                "Knet" => ObservationPointType.K_NET,
                "Kiknet" => ObservationPointType.KiK_net,
                _ => ObservationPointType.Unknown
            };
            var spotData = new SpotData(s[0] == "1", s[1] == "1", kind,
                s[3], s[4], s[5],
                int.Parse(s[6]), int.Parse(s[7]),
                s[8], int.Parse(s[9]), int.Parse(s[10]),
                float.Parse(s[11]), float.Parse(s[12]), float.Parse(s[13]), int.Parse(s[14]),
                int.Parse(s[15]), int.Parse(s[16]), int.Parse(s[17]), int.Parse(s[18]), int.Parse(s[19]));
            var (latOld, lonOld) = kyoshin[spotData.SpotCode];
            observationPoints.Add(new ObservationPoint
            {
                Type = spotData.DataKind,
                Code = spotData.SpotCode,
                Name = spotData.SpotName,
                Region = spotData.Prefecture,
                IsSuspended = spotData.Suspended,
                Location = new Location(spotData.Latitude, spotData.Longitude),
                OldLocation = new Location(latOld, lonOld),
                Point = new Point2(spotData.X + spotData.OffsetX, spotData.Y + spotData.OffsetY),
                ClassificationId = spotData.AreaCode,
                PrefectureClassificationId = spotData.PrefCode
            });
        }

        return observationPoints.ToArray();
    }
}