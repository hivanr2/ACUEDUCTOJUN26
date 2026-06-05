using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ACUEDUCTO.Services;

public sealed class WhatsAppService : IWhatsAppService
{
    private readonly HttpClient _http;
    private readonly WhatsAppOptions _options;
    private readonly ILogger<WhatsAppService> _logger;

    public WhatsAppService(HttpClient http, IOptions<WhatsAppOptions> options, ILogger<WhatsAppService> logger)
    {
        _http = http;
        _options = options.Value;
        _logger = logger;
    }

    public async Task SendMessageAsync(string toE164, string message, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.AccessToken) || string.IsNullOrWhiteSpace(_options.PhoneNumberId))
            throw new InvalidOperationException("WhatsApp configuration is missing. Set AccessToken and PhoneNumberId.");

        var uri = $"{_options.ApiVersion}/{_options.PhoneNumberId}/messages";

        using var req = new HttpRequestMessage(HttpMethod.Post, uri);
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.AccessToken);

        var payload = new
        {
            messaging_product = "whatsapp",
            to = toE164, // e.g., "573001112233"
            type = "text",
            text = new { body = message }
        };

        req.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        var res = await _http.SendAsync(req, cancellationToken);
        var content = await res.Content.ReadAsStringAsync(cancellationToken);

        if (!res.IsSuccessStatusCode)
        {
            _logger.LogError("WhatsApp API error. Status: {Status}, Response: {Body}", (int)res.StatusCode, content);
            throw new HttpRequestException($"WhatsApp API error: {(int)res.StatusCode} - {content}");
        }
    }

    public async Task SendDocumentAsync(string toE164, byte[] documentBytes, string fileName, string caption = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.AccessToken) || string.IsNullOrWhiteSpace(_options.PhoneNumberId))
            throw new InvalidOperationException("WhatsApp configuration is missing. Set AccessToken and PhoneNumberId.");

        // First, upload the media
        var mediaId = await UploadMediaAsync(documentBytes, fileName, cancellationToken);

        // Then, send the message with the document
        var uri = $"{_options.ApiVersion}/{_options.PhoneNumberId}/messages";

        using var req = new HttpRequestMessage(HttpMethod.Post, uri);
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.AccessToken);

        var payload = new
        {
            messaging_product = "whatsapp",
            to = toE164,
            type = "document",
            document = new
            {
                id = mediaId,
                filename = fileName,
                caption = caption
            }
        };

        req.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        var res = await _http.SendAsync(req, cancellationToken);
        var content = await res.Content.ReadAsStringAsync(cancellationToken);

        if (!res.IsSuccessStatusCode)
        {
            _logger.LogError("WhatsApp API error sending document. Status: {Status}, Response: {Body}", (int)res.StatusCode, content);
            throw new HttpRequestException($"WhatsApp API error: {(int)res.StatusCode} - {content}");
        }
    }

    private async Task<string> UploadMediaAsync(byte[] fileBytes, string fileName, CancellationToken cancellationToken)
    {
        var uri = $"{_options.ApiVersion}/{_options.PhoneNumberId}/media";

        using var req = new HttpRequestMessage(HttpMethod.Post, uri);
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.AccessToken);

        var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(fileBytes);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/pdf"); // Assuming PDF
        content.Add(fileContent, "file", fileName);

        req.Content = content;

        var res = await _http.SendAsync(req, cancellationToken);
        var responseContent = await res.Content.ReadAsStringAsync(cancellationToken);

        if (!res.IsSuccessStatusCode)
        {
            _logger.LogError("WhatsApp API error uploading media. Status: {Status}, Response: {Body}", (int)res.StatusCode, responseContent);
            throw new HttpRequestException($"WhatsApp API error uploading media: {(int)res.StatusCode} - {responseContent}");
        }

        var responseJson = JsonSerializer.Deserialize<JsonElement>(responseContent);
        if (responseJson.TryGetProperty("id", out var idElement))
        {
            return idElement.GetString()!;
        }

        throw new InvalidOperationException("Media upload response does not contain 'id'");
    }
}