using System.Text.Json.Serialization;

namespace EarthquakeMap2.Objects;

public record Result(
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("message")] string Message
);


public record Security(
    [property: JsonPropertyName("realm")] string Realm,
    [property: JsonPropertyName("hash")] string Hash
);

public record ServerProps(
    [property: JsonPropertyName("security")] Security Security,
    [property: JsonPropertyName("latest_time")] string LatestTime,
    [property: JsonPropertyName("request_time")] string RequestTime,
    [property: JsonPropertyName("result")] Result Result
);


public record KmoniEewJson(
    [property: JsonPropertyName("result")] Result Result,
    [property: JsonPropertyName("report_time")] string ReportTime,
    [property: JsonPropertyName("region_code")] string RegionCode,
    [property: JsonPropertyName("request_time")] string RequestTime,
    [property: JsonPropertyName("region_name")] string RegionName,
    [property: JsonPropertyName("longitude")] string Longitude,
    [property: JsonPropertyName("is_cancel")] bool IsCancel,
    [property: JsonPropertyName("depth")] string Depth,
    [property: JsonPropertyName("calcintensity")] string CalcIntensity,
    [property: JsonPropertyName("is_final")] bool IsFinal,
    [property: JsonPropertyName("is_training")] bool IsTraining,
    [property: JsonPropertyName("latitude")] string Latitude,
    [property: JsonPropertyName("origin_time")] string OriginTime,
    [property: JsonPropertyName("security")] Security Security,
    [property: JsonPropertyName("magunitude")] string Magnitude,
    [property: JsonPropertyName("report_num")] string ReportNum,
    [property: JsonPropertyName("request_hypo_type")] string RequestHypoType,
    [property: JsonPropertyName("report_id")] string ReportId,
    [property: JsonPropertyName("alertflg")] string AlertFlag
);
