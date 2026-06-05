using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace ACUEDUCTO.Services;

public class ImageAnalysisService
{
    private readonly HttpClient _http;
    private readonly string _endpoint;
    private readonly string _key;

    public ImageAnalysisService(HttpClient http, string endpoint, string key)
    {
        _http = http;
        _endpoint = endpoint;
        _key = key;
    }

    public async Task<(double? MeterNumber, double? Consumption)> AnalyzeImageAsync(string imagePath)
    {
        var url = $"{_endpoint}/computervision/imageanalysis:analyze?api-version=2023-02-01-preview&features=read";

        using var content = new MultipartFormDataContent();
        using var fileStream = File.OpenRead(imagePath);
        var fileContent = new StreamContent(fileStream);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
        content.Add(fileContent, "image", Path.GetFileName(imagePath));

        using var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Headers.Add("Ocp-Apim-Subscription-Key", _key);
        request.Content = content;

        var response = await _http.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Azure Vision API error: {response.StatusCode}");
        }

        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(json);

        var text = ExtractText(result);
        return ExtractNumbers(text);
    }

    private string ExtractText(JsonElement result)
    {
        if (result.TryGetProperty("readResult", out var readResult) &&
            readResult.TryGetProperty("content", out var content))
        {
            return content.GetString() ?? "";
        }
        return "";
    }

    private (double? MeterNumber, double? Consumption) ExtractNumbers(string text)
    {
        var numbers = System.Text.RegularExpressions.Regex.Matches(text, @"\d+(\.\d+)?")
            .Cast<System.Text.RegularExpressions.Match>()
            .Select(m => double.Parse(m.Value))
            .OrderByDescending(n => n)
            .ToList();

        if (numbers.Count >= 2)
        {
            return (numbers[0], numbers[1]);
        }
        else if (numbers.Count == 1)
        {
            return (numbers[0], null);
        }

        return (null, null);
    }
}