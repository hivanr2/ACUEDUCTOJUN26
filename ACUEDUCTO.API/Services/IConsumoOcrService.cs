namespace ACUEDUCTO.API.Services;

public interface IConsumoOcrService
{
    Task<(string ocrText, double? lectura, string? numeroMedidor, double? confianza)> ReadAsync(
        Stream imageStream,
        CancellationToken cancellationToken);
}
