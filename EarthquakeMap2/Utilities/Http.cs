using System.Net;
using System.Net.Http.Json;
using System.ServiceModel.Syndication;
using System.Text.Json;
using System.Xml;

namespace EarthquakeMap2.Utilities;

public static class Http
{
    private static readonly HttpClient HttpClient = new();
    public static async Task<string> DownloadString(string url)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        var response = await HttpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode) throw new HttpRequestException($"サーバーがエラーを返しました: {response.StatusCode}", null, response.StatusCode);

        return await response.Content.ReadAsStringAsync();
    }

    public static async Task<T?> DownloadJson<T>(string url)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        var response = await HttpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode) throw new HttpRequestException($"サーバーがエラーを返しました: {response.StatusCode}", null, response.StatusCode);

        return await response.Content.ReadFromJsonAsync<T>();
    }
}
