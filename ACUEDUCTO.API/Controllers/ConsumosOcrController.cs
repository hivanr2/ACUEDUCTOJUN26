using ACUEDUCTO.API.Options;
using ACUEDUCTO.API.Services;
using ACUEDUCTO.COMPARTIR.DTO;
using ACUEDUCTO.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authorization;

namespace ACUEDUCTO.API.Controllers;

[ApiController]
[Route("api/consumos-ocr")]
[AllowAnonymous]
public sealed class ConsumosOcrController : ControllerBase
{
    private readonly IConsumoOcrService _ocr;
    private readonly IDbContextFactory<AcueductoContext> _dbFactory;
    private readonly ConsumoOcrWorkflow _workflow;
    private readonly IWebHostEnvironment _env;
    private readonly OcrOptions _ocrOptions;

    public ConsumosOcrController(
        IConsumoOcrService ocr,
        IDbContextFactory<AcueductoContext> dbFactory,
        ConsumoOcrWorkflow workflow,
        IWebHostEnvironment env,
        IOptions<OcrOptions> ocrOptions)
    {
        _ocr = ocr;
        _dbFactory = dbFactory;
        _workflow = workflow;
        _env = env;
        _ocrOptions = ocrOptions.Value;
    }

    [HttpPost("scan-debug")]
    [RequestSizeLimit(15_000_000)]
    public async Task<ActionResult<ConsumoOcrScanDebugResponse>> ScanDebug([FromForm] IFormFile image, CancellationToken cancellationToken)
    {
        if (image is null || image.Length == 0)
            return BadRequest("Imagen requerida");

        byte[] bytes;
        await using (var stream = image.OpenReadStream())
        {
            using var ms = new MemoryStream();
            await stream.CopyToAsync(ms, cancellationToken);
            bytes = ms.ToArray();
        }

        int w;
        int h;
        Rectangle? roi = null;
        string? roiPngBase64 = null;
        int? roiW = null;
        int? roiH = null;

        using (var img = Image.Load<Rgba32>(bytes))
        {
            w = img.Width;
            h = img.Height;

            roi = MeterWindowDetector.DetectReadingWindowRoi(img);
            if (roi is not null)
            {
                using var crop = img.Clone(ctx => ctx.Crop(roi.Value));
                roiW = crop.Width;
                roiH = crop.Height;

                using var outMs = new MemoryStream();
                crop.Save(outMs, new PngEncoder());
                roiPngBase64 = Convert.ToBase64String(outMs.ToArray());
            }
        }

        await using var stream2 = new MemoryStream(bytes);
        var (ocrText, lectura, numeroMedidor, confianza) = await _ocr.ReadAsync(stream2, cancellationToken);

        return Ok(new ConsumoOcrScanDebugResponse
        {
            Summary = new ConsumoOcrScanResponse
            {
                PredioId = null,
                CodigoUsuario = null,
                NumeroMedidor = numeroMedidor,
                LecturaActual = lectura,
                OcrText = ocrText,
                Confianza = confianza,
                FechaRecibidaUtc = DateTime.UtcNow
            },
            Debug = new OcrDebugInfo
            {
                ImageWidth = w,
                ImageHeight = h,
                DetectedRoi = roi is null ? null : new OcrRect { X = roi.Value.X, Y = roi.Value.Y, Width = roi.Value.Width, Height = roi.Value.Height },
                RoiCropWidth = roiW,
                RoiCropHeight = roiH,
                RoiPngBase64 = roiPngBase64,
                Note = roi is null
                    ? "Reading window ROI was NOT detected. The service will skip reading OCR and return lectura=null."
                    : "Reading window ROI was detected. If OCR is still empty, the crop may include glare/mud or digits are too small. Try closer, front-facing photo centered on the window."
            },
            Tips =
            [
                "If OcrText is empty, check /api/consumos-ocr/health and make sure tessdata + eng.traineddata exist.",
                "Take the photo closer, without glare, and with the reading window covering >40% of the image.",
                "If the meter is tilted, try to take the photo as front-facing as possible.",
                "If there are red digits (liters), try to exclude them or crop only the black-digit window."
            ]
        });
    }

    [HttpPost("scan")]
    [RequestSizeLimit(15_000_000)]
    public async Task<ActionResult<ConsumoOcrScanResponse>> Scan([FromForm] IFormFile image, CancellationToken cancellationToken)
    {
        if (image is null || image.Length == 0)
            return BadRequest("Imagen requerida");

        byte[] bytes;
        await using (var stream0 = image.OpenReadStream())
        {
            using var ms = new MemoryStream();
            await stream0.CopyToAsync(ms, cancellationToken);
            bytes = ms.ToArray();
        }

        var imageSha = ComputeSha256Hex(bytes);

        // If we already have a user correction saved for this image, use it.
        await using (var db0 = await _dbFactory.CreateDbContextAsync(cancellationToken))
        {
            var corr = await db0.OcrCorrections.AsNoTracking()
                .OrderByDescending(x => x.CreatedUtc)
                .FirstOrDefaultAsync(x => x.ImageSha256 == imageSha, cancellationToken);

            if (corr is not null)
            {
                return Ok(new ConsumoOcrScanResponse
                {
                    PredioId = null,
                    CodigoUsuario = null,
                    NumeroMedidor = corr.NumeroMedidor,
                    LecturaActual = corr.LecturaM3,
                    OcrText = "user-correction",
                    Confianza = 100,
                    FechaRecibidaUtc = DateTime.UtcNow
                });
            }
        }

        await using var stream = new MemoryStream(bytes);
        var (ocrText, lectura, numeroMedidor, confianza) = await _ocr.ReadAsync(stream, cancellationToken);

        // Si se detecta un número de medidor, intentar mapear a predio.
        string? predioId = null;
        string? codigoUsuario = null;
        string? numeroMedidorEncontrado = null;

        if (!string.IsNullOrWhiteSpace(numeroMedidor))
        {
            var ocrDigits = Regex.Replace(numeroMedidor, "\\D+", string.Empty);
            const int minDigitsToMatch = 4;

            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            Predio? predio = null;

            // Matching robusto: comparar SIEMPRE por solo dígitos, porque la BD puede tener guiones/espacios/letras
            // y el OCR puede devolver solo un fragmento.
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

            // Si se encontró el predio y hay lectura, actualizar automáticamente la nueva medición (UltimaMedicion).
            if (predioId is not null && lectura is not null)
            {
                // Reusar el mismo DbContext ya abierto.
                var toUpdate = await db.Predios.FirstOrDefaultAsync(p => p.Idpredio == predioId, cancellationToken);
                if (toUpdate is not null)
                {
                    toUpdate.UltimaMedicion = lectura;
                    await db.SaveChangesAsync(cancellationToken);
                }
            }
        }
        return Ok(new ConsumoOcrScanResponse
        {
            PredioId = predioId,
            CodigoUsuario = codigoUsuario,
            NumeroMedidor = numeroMedidorEncontrado ?? numeroMedidor,
            LecturaActual = lectura,
            OcrText = ocrText,
            Confianza = confianza,
            FechaRecibidaUtc = DateTime.UtcNow
        });
    }

    private static string ComputeSha256Hex(byte[] bytes)
    {
        var hash = SHA256.HashData(bytes);
        var sb = new StringBuilder(hash.Length * 2);
        foreach (var b in hash)
            sb.Append(b.ToString("x2"));
        return sb.ToString();
    }

    [HttpPost("autorizar")]
    public async Task<ActionResult<ConsumoOcrAutorizarResponse>> Autorizar([FromBody] ConsumoOcrAutorizarRequest req, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(req.PredioId))
            return BadRequest("PredioId requerido");

        if (req.LecturaActual < 0)
            return BadRequest("LecturaActual inválida");

        try
        {
            var resp = await _workflow.AutorizarAsync(req, cancellationToken);
            return Ok(resp);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }

    }

    private static int LongestCommonEdgeMatch(string a, string b)
    {
        // Score por coincidencia al inicio o al final (útil cuando OCR detecta solo un fragmento)
        // Ej: a="12345678", b="345678" => score alto por sufijo.
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

    [HttpGet("health")]
    public ActionResult<object> Health()
    {
        var tessDataPath = string.IsNullOrWhiteSpace(_ocrOptions.TessdataPath)
            ? Path.Combine(_env.ContentRootPath, "tessdata")
            : _ocrOptions.TessdataPath!;

        var lang = string.IsNullOrWhiteSpace(_ocrOptions.Language) ? "spa" : _ocrOptions.Language.Trim();
        var trainedFile = Path.Combine(tessDataPath, $"{lang}.traineddata");

        var ok = Directory.Exists(tessDataPath) && System.IO.File.Exists(trainedFile);

        if (!ok)
        {
            return Problem(
                detail: $"OCR no configurado. tessdata: '{tessDataPath}' (exists={Directory.Exists(tessDataPath)}), traineddata: '{trainedFile}' (exists={System.IO.File.Exists(trainedFile)}).",
                statusCode: StatusCodes.Status503ServiceUnavailable);
        }

        return Ok(new
        {
            status = "ok",
            tessdataPath = tessDataPath,
            language = lang,
            traineddataFile = trainedFile
        });
    }
}

public sealed class ConsumoOcrScanDebugResponse
{
    public required ConsumoOcrScanResponse Summary { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public OcrDebugInfo? Debug { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string[]? Tips { get; set; }
}

public sealed class OcrDebugInfo
{
    public int ImageWidth { get; set; }
    public int ImageHeight { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public OcrRect? DetectedRoi { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? RoiCropWidth { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? RoiCropHeight { get; set; }

    // For debugging only (visualize what the detector is cropping).
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? RoiPngBase64 { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Note { get; set; }
}

public sealed class OcrRect
{
    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
}
