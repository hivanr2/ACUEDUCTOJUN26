namespace ACUEDUCTO2.Services;

public sealed class NoopConsumoOcrService : IConsumoOcrService
{
    public Task<(string? ocrText, double? lectura, string? numeroMedidor, double? confianza)> ReadAsync(Stream imageStream, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<(string?, double?, string?, double?)>(("OCR no configurado", null, null, null));
    }
}

public sealed class NoopWhatsAppService : IWhatsAppService
{
    public Task SendMessageAsync(string toE164, string message, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task SendDocumentAsync(string toE164, byte[] documentBytes, string fileName, string caption = "", CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
