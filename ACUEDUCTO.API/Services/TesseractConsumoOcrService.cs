using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Tesseract;

namespace ACUEDUCTO.API.Services;

public sealed class TesseractConsumoOcrService : IConsumoOcrService
{
    private readonly IWebHostEnvironment _env;
    private readonly ACUEDUCTO.API.Options.OcrOptions _options;

    public TesseractConsumoOcrService(IWebHostEnvironment env, IOptions<ACUEDUCTO.API.Options.OcrOptions> options)
    {
        _env = env;
        _options = options.Value;
    }

    public async Task<(string ocrText, double? lectura, string? numeroMedidor, double? confianza)> ReadAsync(
        Stream imageStream,
        CancellationToken cancellationToken)
    {
        using var ms = new MemoryStream();
        await imageStream.CopyToAsync(ms, cancellationToken);
        var imageBytes = ms.ToArray();

        // Validación temprana de tamaño (evita errores de Tesseract con imágenes diminutas)
        using (var imgProbe = Image.Load<Rgba32>(imageBytes))
        {
            if (imgProbe.Width < _options.MinImageWidthPx || imgProbe.Height < _options.MinImageHeightPx)
                throw new InvalidOperationException($"Image too small for OCR ({imgProbe.Width}x{imgProbe.Height}).");
        }

        var tessDataPath = string.IsNullOrWhiteSpace(_options.TessdataPath)
            ? Path.Combine(_env.ContentRootPath, "tessdata")
            : _options.TessdataPath!;

        if (!Directory.Exists(tessDataPath))
            throw new DirectoryNotFoundException($"No existe carpeta tessdata en: {tessDataPath}");

        var lang = string.IsNullOrWhiteSpace(_options.Language) ? "spa" : _options.Language.Trim();
        var trainedFile = Path.Combine(tessDataPath, $"{lang}.traineddata");
        if (!System.IO.File.Exists(trainedFile))
            throw new FileNotFoundException($"No se encontró el archivo de entrenamiento: {trainedFile}. Descárgalo desde https://github.com/tesseract-ocr/tessdata y cópialo a tessdata.");

        var psmCandidates = new[]
        {
            PageSegMode.SingleLine,
            PageSegMode.SingleWord,
            PageSegMode.SingleBlock,
            PageSegMode.Auto
        };

        using var engine = new TesseractEngine(tessDataPath, lang, EngineMode.Default);
        if (_options.NumericOnly)
        {
            engine.SetVariable("tessedit_char_whitelist", "0123456789");
            engine.SetVariable("classify_bln_numeric_mode", "1");
        }

        // 1) OCR completo (used mainly for meter serial; reading will be taken from detected window)
        var best = RunOcr(engine, imageBytes, psmCandidates);

        // 2) Reading window OCR. We only attempt reading OCR on a detected window ROI.
        // If the window can't be detected, we return a controlled "not detected" (lectura remains null).
        byte[]? readingCrop = null;
        using (var img = Image.Load<Rgba32>(imageBytes))
        {
            var roi = MeterWindowDetector.DetectReadingWindowRoi(img);
            if (roi is not null)
            {
                using var roiImg = img.Clone(ctx => ctx.Crop(roi.Value));
                using var roiMs = new MemoryStream();
                roiImg.Save(roiMs, new PngEncoder());
                readingCrop = roiMs.ToArray();
            }
        }

        // If ROI cannot be detected, skip reading OCR to avoid noisy false positives.
        var cropForReading = readingCrop;

        (string ocrText, double? confianza) bestBottom = (string.Empty, null);
        if (cropForReading is not null)
        {
            using (var bottomProbe = Image.Load<Rgba32>(cropForReading))
            {
                if (bottomProbe.Width >= _options.MinImageWidthPx && bottomProbe.Height >= _options.MinImageHeightPx)
                {
                    bestBottom = RunOcr(engine, cropForReading, new[] { PageSegMode.SingleLine, PageSegMode.SingleBlock, PageSegMode.Auto });
                }
            }
        }

        // 3) Preprocesados si la confianza es baja
        if (best.confianza is null || best.confianza < _options.MinConfidenceToSkipPreprocess || string.IsNullOrWhiteSpace(best.ocrText))
        {
            var pre = PreprocessToPng(imageBytes, scale: _options.PreprocessScale, contrast: _options.Contrast, sharpenSigma: _options.SharpenSigma);
            var best2 = RunOcr(engine, pre, psmCandidates);
            if ((best2.confianza ?? 0) > (best.confianza ?? 0))
                best = best2;
        }

        if (cropForReading is not null &&
            (bestBottom.confianza is null || bestBottom.confianza < _options.MinConfidenceToSkipPreprocess || string.IsNullOrWhiteSpace(bestBottom.ocrText))
            && cropForReading.Length > 0)
        {
            // Solo preprocesar si el buffer es válido y no es diminuto
            using var p = Image.Load<Rgba32>(cropForReading);
            if (p.Width >= _options.MinImageWidthPx && p.Height >= _options.MinImageHeightPx)
            {
                var preBottom = PreprocessToPng(cropForReading, scale: _options.PreprocessScale, contrast: _options.Contrast, sharpenSigma: _options.SharpenSigma);
                var bestBottom2 = RunOcr(engine, preBottom, new[] { PageSegMode.SingleLine, PageSegMode.SingleBlock, PageSegMode.Auto });
                if ((bestBottom2.confianza ?? 0) > (bestBottom.confianza ?? 0))
                    bestBottom = bestBottom2;
            }
        }

        // Combinar textos (arriba: serial, abajo: lectura)
        // Si el OCR de abajo detecta algo, lo ponemos al final para que el extractor lo considere "lectura abajo".
        // Combine in a stable way: best (full) first, then reading window (if any)
        var combinedText = string.IsNullOrWhiteSpace(bestBottom.ocrText)
            ? best.ocrText
            : string.Join("\n", new[] { best.ocrText, bestBottom.ocrText }.Where(s => !string.IsNullOrWhiteSpace(s)));

        var extracted = Extract(combinedText, _options);

        // Fallback: try barcode/QR detection for meter serial if OCR didn't find it.
        if (string.IsNullOrWhiteSpace(extracted.numeroMedidor))
        {
            var barcode = BarcodeMeterReader.TryReadBarcode(imageBytes);
            if (!string.IsNullOrWhiteSpace(barcode))
                extracted = (extracted.lectura, barcode);
        }

        // Confianza: reportar la mejor entre ambas
        var conf = new[] { best.confianza, bestBottom.confianza }.Max();
        return (combinedText, extracted.lectura, extracted.numeroMedidor, conf);
    }

    private static byte[] CropBottomRegion(byte[] inputImageBytes, double heightRatio, int minCropHeightPx)
    {
        using var image = Image.Load<Rgba32>(inputImageBytes);

        // Si la imagen es demasiado pequeña, no recortar.
        if (image.Width < 10 || image.Height < minCropHeightPx)
        {
            using var outSmall = new MemoryStream();
            image.Save(outSmall, new PngEncoder());
            return outSmall.ToArray();
        }

        var ratio = Math.Clamp(heightRatio, 0.2, 1.0);
        var cropH = (int)Math.Round(image.Height * ratio);
        cropH = Math.Clamp(cropH, 1, image.Height);

        if (cropH < minCropHeightPx)
            cropH = Math.Min(image.Height, minCropHeightPx);

        var rect = new Rectangle(0, image.Height - cropH, image.Width, cropH);
        image.Mutate(x => x.Crop(rect));

        using var outMs = new MemoryStream();
        image.Save(outMs, new PngEncoder());
        return outMs.ToArray();
    }

    private static (string ocrText, double? confianza) RunOcr(TesseractEngine engine, byte[] imageBytes, PageSegMode[] psmCandidates)
    {
        string bestText = string.Empty;
        double bestScore = -1;
        double? bestConf = null;

        using var pix = Pix.LoadFromMemory(imageBytes);

        foreach (var psm in psmCandidates)
        {
            using var page = engine.Process(pix, psm);
            var text = (page.GetText() ?? string.Empty).Trim();
            var conf = SafeMeanConfidence(page);

            var digitCount = Regex.Matches(text, @"\d").Count;
            var score = (conf ?? 0) + Math.Min(30, digitCount * 2);

            if (score > bestScore)
            {
                bestScore = score;
                bestText = text;
                bestConf = conf;
            }
        }

        return (bestText, bestConf);
    }

    private static double? SafeMeanConfidence(Page page)
    {
        try
        {
            var c = page.GetMeanConfidence();
            return Math.Round(c * 100, 2);
        }
        catch
        {
            return null;
        }
    }

    private static (double? lectura, string? numeroMedidor) Extract(string ocrText, ACUEDUCTO.API.Options.OcrOptions options)
    {
        // Caso esperado por el usuario: 2 líneas (serial arriba, lectura abajo).
        // Evitar perder esta información por normalizar demasiado.
        var lines = (ocrText ?? string.Empty)
            .Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(l => !string.IsNullOrWhiteSpace(l))
            .ToList();

        // Extraer dígitos por línea
        var perLine = lines
            .Select(l => Regex.Matches(l, @"\d{3,12}").Select(m => m.Value).ToList())
            .ToList();

        // Regla: los medidores pueden mostrar lectura completa con dígitos de litros (en rojo) al final.
        // Para registrar consumo en m3, se toman solo los dígitos de m3 (parte izquierda).
        static string? ToM3Digits(string? raw, ACUEDUCTO.API.Options.OcrOptions opt)
        {
            if (string.IsNullOrWhiteSpace(raw)) return null;
            var s = raw.Trim();

            // Si viene muy largo (ej. 8-10), probablemente incluye litros al final.
            // Asumimos que los últimos 2-3 dígitos son litros y se ignoran.
            if (s.Length >= opt.LitersSuffixMinTotalDigits)
            {
                // Quitar 3 por defecto, si queda muy corto quitar 2
                var m3Len = s.Length - 3;
                if (m3Len < opt.MinM3Digits) m3Len = s.Length - 2;
                if (m3Len < opt.MinM3Digits) m3Len = opt.MinM3Digits;

                m3Len = Math.Clamp(m3Len, opt.MinM3Digits, opt.MaxM3Digits);
                return s.Substring(0, m3Len);
            }

            // Si es 4-6 dígitos, se asume ya está en m3
            if (s.Length is >= 4 and <= 6) return s;

            // Si es 3 dígitos, aceptar como fallback
            if (s.Length == 3) return s;

            return s;
        }

        // 1) Lectura: preferir ÚLTIMA línea con 4-7 dígitos (medidor mecánico)
        string? lecturaStr = null;
        for (int i = perLine.Count - 1; i >= 0; i--)
        {
            var candidates = perLine[i].Where(g => g.Length is >= 4 and <= 10).ToList();
            if (candidates.Count == 0)
                candidates = perLine[i].Where(g => g.Length is >= 3 and <= 10).ToList();

            var raw = candidates
                .OrderByDescending(g => g.Length)
                .FirstOrDefault();

            lecturaStr = ToM3Digits(raw, options);

            if (!string.IsNullOrWhiteSpace(lecturaStr))
                break;
        }

        double? lectura = null;
        if (!string.IsNullOrWhiteSpace(lecturaStr) && double.TryParse(lecturaStr, NumberStyles.Integer, CultureInfo.InvariantCulture, out var val))
            lectura = val;

        // 2) Serial / número medidor: preferir PRIMERA línea con 6+ dígitos
        string? numeroMedidor = null;
        for (int i = 0; i < perLine.Count; i++)
        {
            numeroMedidor = perLine[i]
                .Where(g => g.Length >= 6)
                .OrderByDescending(g => g.Length)
                .FirstOrDefault();

            if (!string.IsNullOrWhiteSpace(numeroMedidor))
                break;
        }

        // Fallback global si no se encontró por líneas
        if (lectura is null || string.IsNullOrWhiteSpace(numeroMedidor))
        {
            var normalized = NormalizeDigits(ocrText);
            var groups = Regex.Matches(normalized, @"\d{3,12}")
                .Select(m => m.Value)
                .Distinct()
                .ToList();

            if (lectura is null)
            {
                var candidates = groups.Where(g => g.Length is >= 4 and <= 10).ToList();
                if (candidates.Count == 0)
                    candidates = groups.Where(g => g.Length is >= 3 and <= 10).ToList();

                var raw = candidates
                    .OrderByDescending(g => g.Length)
                    .ThenByDescending(g => LastIndexOf(normalized, g))
                    .FirstOrDefault();

                var lStr = ToM3Digits(raw, options);
                if (!string.IsNullOrWhiteSpace(lStr) && double.TryParse(lStr, NumberStyles.Integer, CultureInfo.InvariantCulture, out var v2))
                    lectura = v2;
            }

            if (string.IsNullOrWhiteSpace(numeroMedidor))
            {
                numeroMedidor = groups
                    .Where(g => g.Length >= 6)
                    .OrderByDescending(g => g.Length)
                    .ThenByDescending(g => LastIndexOf(normalized, g))
                    .FirstOrDefault();
            }
        }

        if (!string.IsNullOrWhiteSpace(numeroMedidor) && lecturaStr is not null && numeroMedidor == lecturaStr)
            numeroMedidor = null;

        return (lectura, numeroMedidor);
    }

    private static int LastIndexOf(string haystack, string needle)
        => haystack.LastIndexOf(needle, StringComparison.Ordinal);

    private static string NormalizeDigits(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return string.Empty;
        var sb = new StringBuilder(input.Length);
        foreach (var ch in input)
        {
            if (char.IsDigit(ch)) sb.Append(ch);
            else if (char.IsWhiteSpace(ch) || ch == '-' || ch == '_' || ch == '.') sb.Append(' ');
        }
        return Regex.Replace(sb.ToString(), @"\s+", " ").Trim();
    }

    private static byte[] PreprocessToPng(byte[] inputImageBytes, int scale, float contrast, float sharpenSigma)
    {
        using var image = Image.Load<Rgba32>(inputImageBytes);

        if (scale > 1)
        {
            image.Mutate(x => x.Resize(image.Width * scale, image.Height * scale));
        }

        image.Mutate(x => x.Grayscale());

        if (contrast > 0 && Math.Abs(contrast - 1f) > 0.001f)
            image.Mutate(x => x.Contrast(contrast));

        if (sharpenSigma > 0)
            image.Mutate(x => x.GaussianSharpen(sharpenSigma));

        var threshold = OtsuThreshold(image);
        image.Mutate(x => x.BinaryThreshold(threshold / 255f));

        using var outMs = new MemoryStream();
        image.Save(outMs, new PngEncoder());
        return outMs.ToArray();
    }

    private static int OtsuThreshold(Image<Rgba32> gray)
    {
        var hist = new int[256];
        long total = (long)gray.Width * gray.Height;

        gray.ProcessPixelRows(accessor =>
        {
            for (int y = 0; y < accessor.Height; y++)
            {
                var row = accessor.GetRowSpan(y);
                for (int x = 0; x < row.Length; x++)
                {
                    hist[row[x].R]++;
                }
            }
        });

        double sum = 0;
        for (int t = 0; t < 256; t++) sum += t * hist[t];

        double sumB = 0;
        long wB = 0;
        long wF;

        double varMax = 0;
        int threshold = 128;

        for (int t = 0; t < 256; t++)
        {
            wB += hist[t];
            if (wB == 0) continue;
            wF = total - wB;
            if (wF == 0) break;

            sumB += t * hist[t];

            var mB = sumB / wB;
            var mF = (sum - sumB) / wF;

            var varBetween = (double)wB * wF * (mB - mF) * (mB - mF);
            if (varBetween > varMax)
            {
                varMax = varBetween;
                threshold = t;
            }
        }

        return threshold;
    }
}
