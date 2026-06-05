using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using ACUEDUCTO.API.Options;
using Microsoft.Extensions.Options;

namespace ACUEDUCTO.API.Services;

// OpenAI-compatible vision call (works with OpenAI or Azure OpenAI compatible endpoints)
public sealed class GptVisionConsumoOcrService : IConsumoOcrService
{
    private readonly HttpClient _http;
    private readonly OcrOptions _options;

    public GptVisionConsumoOcrService(HttpClient http, IOptions<OcrOptions> options)
    {
        _http = http;
        _options = options.Value;
    }

    public async Task<(string ocrText, double? lectura, string? numeroMedidor, double? confianza)> ReadAsync(
        Stream imageStream,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_options.GptVisionEndpoint))
            throw new InvalidOperationException("GptVisionEndpoint is not configured.");

        if (!Uri.TryCreate(_options.GptVisionEndpoint, UriKind.Absolute, out var endpointUri))
            throw new InvalidOperationException($"GptVisionEndpoint must be an absolute URI. Current value: '{_options.GptVisionEndpoint}'.");

        if (string.IsNullOrWhiteSpace(_options.GptVisionApiKey))
            throw new InvalidOperationException("GptVisionApiKey is not configured.");

        using var ms = new MemoryStream();
        await imageStream.CopyToAsync(ms, cancellationToken);
        var base64 = Convert.ToBase64String(ms.ToArray());

        var prompt = "You are reading a mechanical water meter. Extract (1) meter serial/number and (2) reading in m3 (BLACK digits only). Ignore RED digits (liters/decimals). Return ONLY valid JSON with fields: lectura_m3 (integer or null), numero_medidor (string or null), notes (string). If uncertain, set the field to null. Do not guess.";

        object payload = _options.GptVisionApiStyle.Equals("responses", StringComparison.OrdinalIgnoreCase)
            ? new
            {
                model = _options.GptVisionModel,
                input = new object[]
                {
                    new
                    {
                        role = "user",
                        content = new object[]
                        {
                            new { type = "input_text", text = prompt },
                            new { type = "input_image", image_url = $"data:image/jpeg;base64,{base64}" }
                        }
                    }
                },
                response_format = new { type = "json_object" }
            }
            : new
            {
                model = _options.GptVisionModel,
                messages = new object[]
                {
                    new
                    {
                        role = "user",
                        content = new object[]
                        {
                            new { type = "text", text = prompt },
                            new { type = "image_url", image_url = new { url = $"data:image/jpeg;base64,{base64}" } }
                        }
                    }
                },
                response_format = new { type = "json_object" }
            };

        using var req = new HttpRequestMessage(HttpMethod.Post, endpointUri);
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.GptVisionApiKey);
        req.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        using var resp = await _http.SendAsync(req, cancellationToken);
        var body = await resp.Content.ReadAsStringAsync(cancellationToken);
        if (!resp.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"GPT Vision request failed with status {(int)resp.StatusCode} ({resp.ReasonPhrase}). Body: {body}");
        }

        // Try to find the JSON result inside the response.
        // Supports chat.completions and responses APIs.
        string json = TryExtractAssistantContentAsJson(body) ?? ExtractJson(body) ?? body;

        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            double? lectura = null;
            if (root.TryGetProperty("lectura_m3", out var lecturaEl) && lecturaEl.ValueKind == JsonValueKind.Number)
                lectura = lecturaEl.GetDouble();

            string? numero = null;
            if (root.TryGetProperty("numero_medidor", out var numEl) && numEl.ValueKind == JsonValueKind.String)
                numero = numEl.GetString();

            string? notes = null;
            if (root.TryGetProperty("notes", out var notesEl) && notesEl.ValueKind == JsonValueKind.String)
                notes = notesEl.GetString();

            return (notes ?? string.Empty, lectura, string.IsNullOrWhiteSpace(numero) ? null : numero.Trim(), null);
        }
        catch
        {
            // If parsing fails, return raw text for debugging.
            return (body, null, null, null);
        }
    }

    private static string? ExtractJson(string responseBody)
    {
        // Very small heuristic: find first '{' and last '}'
        var start = responseBody.IndexOf('{');
        var end = responseBody.LastIndexOf('}');
        if (start >= 0 && end > start)
            return responseBody.Substring(start, end - start + 1);
        return null;
    }

    private static string? TryExtractAssistantContentAsJson(string responseBody)
    {
        try
        {
            using var doc = JsonDocument.Parse(responseBody);
            var root = doc.RootElement;

            // Chat Completions: choices[0].message.content
            if (root.TryGetProperty("choices", out var choices) && choices.ValueKind == JsonValueKind.Array && choices.GetArrayLength() > 0)
            {
                var msg = choices[0].GetProperty("message");
                if (msg.TryGetProperty("content", out var content) && content.ValueKind == JsonValueKind.String)
                    return content.GetString();
            }

            // Responses API style: output[0].content[0].text
            if (root.TryGetProperty("output", out var output) && output.ValueKind == JsonValueKind.Array && output.GetArrayLength() > 0)
            {
                var out0 = output[0];
                if (out0.TryGetProperty("content", out var contentArr) && contentArr.ValueKind == JsonValueKind.Array && contentArr.GetArrayLength() > 0)
                {
                    var c0 = contentArr[0];
                    if (c0.TryGetProperty("text", out var textEl) && textEl.ValueKind == JsonValueKind.String)
                        return textEl.GetString();
                }
            }
        }
        catch
        {
            // ignore
        }

        return null;
    }
}
