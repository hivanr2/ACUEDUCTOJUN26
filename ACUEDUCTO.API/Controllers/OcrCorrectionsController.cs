using System.Security.Cryptography;
using System.Text;
using ACUEDUCTO.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ACUEDUCTO.API.Controllers;

[ApiController]
[Route("api/ocr-corrections")]
[AllowAnonymous]
public sealed class OcrCorrectionsController : ControllerBase
{
    private readonly IDbContextFactory<AcueductoContext> _dbFactory;

    public OcrCorrectionsController(IDbContextFactory<AcueductoContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public sealed class CreateCorrectionRequest
    {
        public required string ImageBase64 { get; set; }
        public string? NumeroMedidor { get; set; }
        public double? LecturaM3 { get; set; }

        // Optional: apply correction directly to a planilla's Facturacion rows
        public string? IdPlanilla { get; set; }
    }

    [HttpPost]
    public async Task<ActionResult<object>> Create([FromBody] CreateCorrectionRequest req, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(req.ImageBase64))
            return BadRequest("ImageBase64 is required.");

        byte[] bytes;
        try
        {
            bytes = Convert.FromBase64String(req.ImageBase64);
        }
        catch
        {
            return BadRequest("ImageBase64 is not valid base64.");
        }

        var hash = ComputeSha256Hex(bytes);

        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);

        // Upsert by ImageSha256: prevent duplicates for the exact same image.
        var existing = await db.OcrCorrections
            .OrderByDescending(x => x.CreatedUtc)
            .FirstOrDefaultAsync(x => x.ImageSha256 == hash, cancellationToken);

        var serial = string.IsNullOrWhiteSpace(req.NumeroMedidor) ? null : req.NumeroMedidor.Trim();
        var lectura = req.LecturaM3;

        if (existing is null)
        {
            var correction = new OcrCorrection
            {
                ImageSha256 = hash,
                NumeroMedidor = serial,
                LecturaM3 = lectura,
                CreatedUtc = DateTime.UtcNow
            };

            db.OcrCorrections.Add(correction);
            await db.SaveChangesAsync(cancellationToken);

            var applied = await TryApplyToPlanillaAsync(db, req.IdPlanilla, serial, lectura, cancellationToken);
            return Ok(new { status = "created", imageSha256 = hash, correction.Id, applied });
        }

        existing.NumeroMedidor = serial;
        existing.LecturaM3 = lectura;
        existing.CreatedUtc = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);

        var applied2 = await TryApplyToPlanillaAsync(db, req.IdPlanilla, serial, lectura, cancellationToken);
        return Ok(new { status = "updated", imageSha256 = hash, existing.Id, applied = applied2 });
    }

    private static string DigitsOnly(string? s)
        => string.IsNullOrWhiteSpace(s) ? string.Empty : System.Text.RegularExpressions.Regex.Replace(s, "\\D+", string.Empty);

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

        // Load minimal candidate set for the planilla and match by normalized meter number.
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

    [HttpGet("{imageSha256}")]
    public async Task<ActionResult<OcrCorrection>> Get(string imageSha256, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(imageSha256))
            return BadRequest("imageSha256 is required");

        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        var corr = await db.OcrCorrections.AsNoTracking()
            .OrderByDescending(x => x.CreatedUtc)
            .FirstOrDefaultAsync(x => x.ImageSha256 == imageSha256, cancellationToken);

        if (corr is null)
            return NotFound();

        return Ok(corr);
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
