using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using ACUEDUCTO2.Data;
using ACUEDUCTO2.Share.Models;
using Microsoft.EntityFrameworkCore;

namespace ACUEDUCTO2.Services
{
    public sealed class ConsumoOcrAppService
    {
        private readonly IConsumoOcrService _ocr;
        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;

        public ConsumoOcrAppService(IConsumoOcrService ocr, IDbContextFactory<ApplicationDbContext> dbFactory)
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

            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);

            // NOTE: Minimal implementation for v2 port - do not modify DB in Scan.
            var (ocrText, lectura, numeroMedidor, confianza) = await _ocr.ReadAsync(new MemoryStream(bytes), cancellationToken);

            string? predioId = null;
            string? codigoUsuario = null;
            string? numeroMedidorEncontrado = null;

            if (!string.IsNullOrWhiteSpace(numeroMedidor))
            {
                var ocrDigits = Regex.Replace(numeroMedidor, "\\D+", string.Empty);
                const int minDigitsToMatch = 4;

                if (!string.IsNullOrWhiteSpace(ocrDigits) && ocrDigits.Length >= minDigitsToMatch)
                {
                    var candidates = await db.Predio.AsNoTracking()
                        .Where(p => p.NumeroMedidor != null && p.NumeroMedidor != "")
                        .Select(p => new { p.IdPredio, p.CodigoUsuario, p.NumeroMedidor })
                        .ToListAsync(cancellationToken);

                    var best = candidates
                        .Select(p => new
                        {
                            p.IdPredio,
                            p.CodigoUsuario,
                            p.NumeroMedidor,
                            Digits = Regex.Replace(p.NumeroMedidor ?? string.Empty, "\\D+", string.Empty),
                            Score = LongestCommonEdgeMatch(Regex.Replace(p.NumeroMedidor ?? string.Empty, "\\D+", string.Empty), ocrDigits)
                        })
                        .Where(x => x.Score >= minDigitsToMatch)
                        .OrderByDescending(x => x.Score)
                        .ThenByDescending(x => x.Digits.Length)
                        .FirstOrDefault();

                    if (best is not null)
                    {
                        predioId = best.IdPredio;
                        codigoUsuario = best.CodigoUsuario;
                        numeroMedidorEncontrado = best.NumeroMedidor;
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

            // Minimal storage: create OcrCorrections table entry if not exists
            var existing = await db.Set<OcrCorrection>().OrderByDescending(x => x.CreatedUtc).FirstOrDefaultAsync(x => x.ImageSha256 == hash, cancellationToken);

            if (existing is null)
            {
                var correction = new OcrCorrection
                {
                    ImageSha256 = hash,
                    NumeroMedidor = string.IsNullOrWhiteSpace(numeroMedidor) ? null : numeroMedidor.Trim(),
                    LecturaM3 = lecturaM3,
                    CreatedUtc = DateTime.UtcNow
                };
                db.Set<OcrCorrection>().Add(correction);
                await db.SaveChangesAsync(cancellationToken);
                return new SaveCorrectionResult("created", hash, correction.Id, new { updated = 0 });
            }

            existing.NumeroMedidor = string.IsNullOrWhiteSpace(numeroMedidor) ? null : numeroMedidor.Trim();
            existing.LecturaM3 = lecturaM3;
            existing.CreatedUtc = DateTime.UtcNow;
            await db.SaveChangesAsync(cancellationToken);
            return new SaveCorrectionResult("updated", hash, existing.Id, new { updated = 0 });
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
}
