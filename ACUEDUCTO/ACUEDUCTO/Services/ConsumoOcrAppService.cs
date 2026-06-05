using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using ACUEDUCTO.API.Services;
using ACUEDUCTO.Models;
using Microsoft.EntityFrameworkCore;

namespace ACUEDUCTO.Services;

public sealed class ConsumoOcrAppService
{
    private readonly IConsumoOcrService _ocr;
    private readonly IDbContextFactory<AcueductoContext> _dbFactory;

    public ConsumoOcrAppService(IConsumoOcrService ocr, IDbContextFactory<AcueductoContext> dbFactory)
    {
        _ocr = ocr;
        _dbFactory = dbFactory;
    }

    public sealed record ScanResult(string? PredioId, string? CodigoUsuario, string? NumeroMedidor, double? LecturaActual, string? OcrText, double? Confianza);

    public async Task<ScanResult> ScanAsync(Stream imageStream, CancellationToken cancellationToken)
    {
        using var ms = new MemoryStream();
        await imageStream.CopyToAsync(ms, cancellationToken);
        var bytes = ms.ToArray();

        var imageSha = ComputeSha256Hex(bytes);

        // If we already have a user correction saved for this image, use it.
        await using (var db0 = await _dbFactory.CreateDbContextAsync(cancellationToken))
        {
            var corr = await db0.OcrCorrections.AsNoTracking()
                .OrderByDescending(x => x.CreatedUtc)
                .FirstOrDefaultAsync(x => x.ImageSha256 == imageSha, cancellationToken);

            if (corr is not null)
            {
                return new ScanResult(null, null, corr.NumeroMedidor, corr.LecturaM3, "user-correction", 100);
            }
        }

        await using var stream = new MemoryStream(bytes);
        var (ocrText, lectura, numeroMedidor, confianza) = await _ocr.ReadAsync(stream, cancellationToken);

        // If a meter number was detected, try to map it to a predio.
        string? predioId = null;
        string? codigoUsuario = null;
        string? numeroMedidorEncontrado = null;

        if (!string.IsNullOrWhiteSpace(numeroMedidor))
        {
            var ocrDigits = Regex.Replace(numeroMedidor, "\\D+", string.Empty);
            const int minDigitsToMatch = 4;

            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            Predio? predio = null;

            if (!string.IsNullOrWhiteSpace(ocrDigits) && ocrDigits.Length >= minDigitsToMatch)
            {
                var candidates = await db.Predios.AsNoTracking()
                    .Where(p => p.NumeroMedidor != null && p.NumeroMedidor != "")
                    .Select(p => new { p.Idpredio, p.CodigoUsuario, p.NumeroMedidor })
                    .ToListAsync(cancellationToken);

                var best = candidates
                    .Select(p =>
                    {
                        var digits = Regex.Replace(p.NumeroMedidor ?? string.Empty, "\\D+", string.Empty);
                        return new
                        {
                            p.Idpredio,
                            p.CodigoUsuario,
                            p.NumeroMedidor,
                            Digits = digits,
                            Score = LongestCommonEdgeMatch(digits, ocrDigits)
                        };
                    })
                    .Where(x => x.Score >= minDigitsToMatch)
                    .OrderByDescending(x => x.Score)
                    .ThenByDescending(x => x.Digits.Length)
                    .FirstOrDefault();

                if (best is not null)
                {
                    predio = new Predio { Idpredio = best.Idpredio, CodigoUsuario = best.CodigoUsuario, NumeroMedidor = best.NumeroMedidor };
                }
            }

            predioId = predio?.Idpredio;
            codigoUsuario = predio?.CodigoUsuario;
            numeroMedidorEncontrado = predio?.NumeroMedidor;

            // Preserve previous behavior: update UltimaMedicion on predio.
            if (predioId is not null && lectura is not null)
            {
                var toUpdate = await db.Predios.FirstOrDefaultAsync(p => p.Idpredio == predioId, cancellationToken);
                if (toUpdate is not null)
                {
                    toUpdate.UltimaMedicion = lectura;
                    await db.SaveChangesAsync(cancellationToken);
                }
            }
        }

        return new ScanResult(predioId, codigoUsuario, numeroMedidorEncontrado ?? numeroMedidor, lectura, ocrText, confianza);
    }

    public sealed record SaveCorrectionResult(string Status, string ImageSha256, int CorrectionId, object Applied);

    public async Task<SaveCorrectionResult> SaveCorrectionAsync(byte[] imageBytes, string? numeroMedidor, double? lecturaM3, string? idPlanilla, CancellationToken cancellationToken)
    {
        var hash = ComputeSha256Hex(imageBytes);

        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);

        var existing = await db.OcrCorrections
            .OrderByDescending(x => x.CreatedUtc)
            .FirstOrDefaultAsync(x => x.ImageSha256 == hash, cancellationToken);

        var serial = string.IsNullOrWhiteSpace(numeroMedidor) ? null : numeroMedidor.Trim();

        if (existing is null)
        {
            var correction = new OcrCorrection
            {
                ImageSha256 = hash,
                NumeroMedidor = serial,
                LecturaM3 = lecturaM3,
                CreatedUtc = DateTime.UtcNow
            };

            db.OcrCorrections.Add(correction);
            await db.SaveChangesAsync(cancellationToken);

            var applied = await TryApplyToPlanillaAsync(db, idPlanilla, serial, lecturaM3, cancellationToken);
            return new SaveCorrectionResult("created", hash, correction.Id, applied);
        }

        existing.NumeroMedidor = serial;
        existing.LecturaM3 = lecturaM3;
        existing.CreatedUtc = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);

        var applied2 = await TryApplyToPlanillaAsync(db, idPlanilla, serial, lecturaM3, cancellationToken);
        return new SaveCorrectionResult("updated", hash, existing.Id, applied2);
    }

    private static string DigitsOnly(string? s)
        => string.IsNullOrWhiteSpace(s) ? string.Empty : Regex.Replace(s, "\\D+", string.Empty);

    private static async Task<object> TryApplyToPlanillaAsync(
        AcueductoContext db,
        string? idPlanilla,
        string? numeroMedidor,
        double? lecturaM3,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(idPlanilla) || string.IsNullOrWhiteSpace(numeroMedidor) || lecturaM3 is null)
            return new { updated = 0, reason = "missing_idPlanilla_or_data" };

        var ocrDigits = DigitsOnly(numeroMedidor);
        if (string.IsNullOrWhiteSpace(ocrDigits))
            return new { updated = 0, reason = "no_digits_in_numeroMedidor" };

        var facturas = await db.Facturacions
            .Where(f => f.Idplanilla == idPlanilla)
            .Include(f => f.IdpredioNavigation)
            .ToListAsync(cancellationToken);

        var target = facturas
            .Select(f => new
            {
                Factura = f,
                MeterDigits = DigitsOnly(f.IdpredioNavigation?.NumeroMedidor)
            })
            .Where(x => !string.IsNullOrWhiteSpace(x.MeterDigits))
            .OrderByDescending(x => LongestCommonEdgeMatch(x.MeterDigits, ocrDigits))
            .FirstOrDefault();

        if (target is null)
            return new { updated = 0, reason = "no_facturas_or_no_meters" };

        var score = LongestCommonEdgeMatch(target.MeterDigits, ocrDigits);
        if (score <= 0)
            return new { updated = 0, reason = "no_match" };

        // Project rule: only update MedicionActual; do not recalculate other fields.
        target.Factura.MedicionActual = lecturaM3;
        await db.SaveChangesAsync(cancellationToken);

        return new { updated = 1, idFactura = target.Factura.Idfactura, matchScore = score };
    }

    private static int LongestCommonEdgeMatch(string a, string b)
    {
        if (string.IsNullOrEmpty(a) || string.IsNullOrEmpty(b)) return 0;

        var max = Math.Min(a.Length, b.Length);
        var best = 0;
        for (var len = 1; len <= max; len++)
        {
            if (a.EndsWith(b[^len..], StringComparison.Ordinal)) best = Math.Max(best, len);
            if (a.StartsWith(b[..len], StringComparison.Ordinal)) best = Math.Max(best, len);
            if (b.EndsWith(a[^len..], StringComparison.Ordinal)) best = Math.Max(best, len);
            if (b.StartsWith(a[..len], StringComparison.Ordinal)) best = Math.Max(best, len);
        }
        return best;
    }

    private static string ComputeSha256Hex(byte[] bytes)
    {
        var hash = SHA256.HashData(bytes);
        var sb = new StringBuilder(hash.Length * 2);
        foreach (var b in hash)
            sb.Append(b.ToString("x2"));
        return sb.ToString();
    }
}
