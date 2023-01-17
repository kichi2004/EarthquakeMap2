using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Net;
using System.Timers;
using EarthquakeLibrary;
using EarthquakeMap2.Objects;
using EarthquakeMap2.Utilities;
using EewLibrary;
using KyoshinMonitorLib;
using KyoshinMonitorLib.SkiaImages;
using SkiaSharp;
using LibEew = EewLibrary.EEW;
using Location = EewLibrary.Location;

#nullable disable

namespace EarthquakeMap2.Services.Eew
{
    public class KmoniEew : LibEew
    {
        public IEnumerable<ImageAnalysisResult> EstimationResults { get; init; }
    }

    internal class KmoniEewService : IEewService<LibEew>
    {
        private DateTime _currentTime;
        private const string PropsUrl = "http://www.kmoni.bosai.go.jp/webservice/server/pros/latest.json";
        private const string EewUrl = "http://www.kmoni.bosai.go.jp/webservice/hypo/eew/{0}.json";
        private readonly WebApi _webApi = new ();
        private readonly ConcurrentDictionary<string, int> ReportNums = new();

        public async Task Start()
        {
            var latest = await Http.DownloadJson<ServerProps>(PropsUrl);
            if (latest == null) throw new Exception("強震モニタの現在時刻の取得に失敗しました。");
            UpdateTime(DateTime.Parse(latest.LatestTime, styles: DateTimeStyles.AssumeLocal));
            var timer = new System.Timers.Timer(1000);
            timer.Elapsed += TimerElapsed;
            timer.Start();
        }

        [SuppressMessage("ReSharper", "PossibleInvalidOperationException")]
        private async void TimerElapsed(object s, ElapsedEventArgs elapsedEventArgs)
        {
            var time = _currentTime;
            _currentTime = _currentTime.AddSeconds(1);
            var eewInfo = await _webApi.GetEewInfo(time);
            var res = await _webApi.GetEstShindoImageData(time);

            if (res.StatusCode != HttpStatusCode.OK || eewInfo.StatusCode != HttpStatusCode.OK || eewInfo.Data!.Result?.Message == "データがありません") return;

            var eewData = eewInfo.Data;
            if (eewData == null || string.IsNullOrEmpty(eewData.ReportId)) return;
            bool isFirst = !ReportNums.ContainsKey(eewData.ReportId);
            var reportNum = eewData.ReportNum;
            if (ReportNums.TryGetValue(eewData.ReportId, out var latest) && (latest == -1 || latest >= reportNum)) return;

            if (eewData.IsFinal is true)
            {
                ReportNums.AddOrUpdate(eewData.ReportId, -1, (_, _) => -1);
            }
            else
            {
                ReportNums.AddOrUpdate(eewData.ReportId, reportNum.Value, (_, v) => Math.Max(v, reportNum.Value));
            }
            
            using var eewBmp = SKBitmap.Decode(res.Data);
            ImageAnalysisResult[] results = null;
            if (eewBmp != null)
            {
                var points = Form1.ObservationPoints.Select(x => new ImageAnalysisResult(x)).ToArray();
                results = points.ParseScaleFromImage(eewBmp).ToArray();
            }

            var eew = new KmoniEew
            {
                AnnounceTime = eewData.ReportTime.Value,
                Epicenter = eewData.RegionName,
                Location = new Location
                {
                    Longitude = eewData.Longitude.Value,
                    Latitude = eewData.Latitude.Value
                },
                InfoType = eewData.IsCancel is true ? Types.Cancel :  0,
                Identification = eewData.IsCancel is true ? Identifications.Cancel : Identifications.Normal,
                Depth = eewData.Depth,
                MaxInt = Intensity.Parse(eewData.CalcintensityString),
                Status = eewData.IsFinal is true ? Statuses.Last : Statuses.Normal,
                DetectionTime =  eewData.OriginTime,
                Magnitude = eewData.Magunitude.Value,
                QuakeId = eewData.ReportId,
                Number = reportNum,
                EpicenterCode = null,
                IsWarn = eewData.AlertFlag == "警報",
                EstimationResults = results
            };

            var eventArgs = new EewUpdatedEventArgs<LibEew>(eew);


            Console.WriteLine($"[EEW] No.{eew.Number}(last={eew.IsLast}) {eew.Epicenter} M{eew.Magnitude:0.0} {eew.Depth}km 最大{eew.MaxInt.LongString}");
            if (isFirst) EewUpdatedForFirst?.Invoke(this, eventArgs);
            else EewUpdatedForContinue?.Invoke(this, eventArgs);
        }

        public async void UpdateTime(DateTime? dateTime)
        {
            if (dateTime == null)
            {
                var latest = await Http.DownloadJson<ServerProps>(PropsUrl);
                if (latest == null) throw new Exception("強震モニタの現在時刻の取得に失敗しました。");
                dateTime = DateTime.Parse(latest.LatestTime, styles: DateTimeStyles.AssumeLocal);
            }
            _currentTime = dateTime.Value;
        }

        public event EventHandler<EewUpdatedEventArgs<LibEew>>? EewUpdatedForFirst;
        public event EventHandler<EewUpdatedEventArgs<LibEew>>? EewUpdatedForContinue;
    }
}