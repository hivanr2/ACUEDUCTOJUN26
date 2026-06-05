using System;
using System.Collections.Generic;
using System.Linq;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace ACUEDUCTO.API.Services;

internal static class MeterWindowDetector
{
    // Heurística simple sin dependencias extras:
    // 1) Convertir a grises
    // 2) Buscar filas con alta proporción de pixeles claros (ventana blanca)
    // 3) Tomar el mejor segmento vertical y luego recortar columnas alrededor de la zona clara
    public static Rectangle? DetectReadingWindowRoi(Image<Rgba32> image)
    {
        if (image.Width < 50 || image.Height < 50)
            return null;

        using var gray = image.Clone(ctx => ctx.Grayscale());

        // Compute global brightness to adapt thresholds for dirty/gray windows
        double globalAvg;
        {
            long sum = 0;
            long count = 0;
            gray.ProcessPixelRows(accessor =>
            {
                for (int y = 0; y < accessor.Height; y++)
                {
                    var row = accessor.GetRowSpan(y);
                    for (int x = 0; x < row.Length; x++)
                    {
                        sum += row[x].R;
                        count++;
                    }
                }
            });
            globalAvg = count == 0 ? 128 : (double)sum / count;
        }

        // When the image is darker (mud/shadow), lower the "white" threshold.
        byte whiteCut = (byte)Math.Clamp(globalAvg + 30, 170, 215);

        var rowWhiteRatio = new double[gray.Height];
        gray.ProcessPixelRows(accessor =>
        {
            for (int y = 0; y < accessor.Height; y++)
            {
                var row = accessor.GetRowSpan(y);
                int white = 0;
                for (int x = 0; x < row.Length; x++)
                {
                    // R ya es intensidad en grises
                    if (row[x].R >= whiteCut) white++;
                }
                rowWhiteRatio[y] = (double)white / row.Length;
            }
        });

        // Focus search on a middle band (most meters place the window there)
        int ySearchStart = (int)(gray.Height * 0.2);
        int ySearchEnd = (int)(gray.Height * 0.75);
        var band = rowWhiteRatio.Skip(ySearchStart).Take(Math.Max(1, ySearchEnd - ySearchStart)).ToArray();

        // Threshold: window band should have a noticeable amount of light pixels, but may be dirty.
        double threshold = 0.16;
        var segments = FindSegments(band, threshold, minLen: Math.Max(10, gray.Height / 50))
            .Select(s => (Start: s.Start + ySearchStart, End: s.End + ySearchStart))
            .ToList();
        if (segments.Count == 0)
            return null;

        // Puntuar segmentos: preferir los que están en la mitad superior/media (no muy arriba ni muy abajo)
        var bestSeg = segments
            .Select(seg => new
            {
                seg.Start,
                seg.End,
                Len = seg.End - seg.Start + 1,
                Avg = rowWhiteRatio.Skip(seg.Start).Take(seg.End - seg.Start + 1).Average(),
                Center = (seg.Start + seg.End) / 2.0
            })
            .OrderByDescending(s => s.Avg * s.Len)
            .ThenBy(s => Math.Abs(s.Center - (gray.Height * 0.45)))
            .First();

        int y0 = Math.Max(0, bestSeg.Start - 6);
        int y1 = Math.Min(gray.Height - 1, bestSeg.End + 6);

        // Ahora columnas: en ese rango de filas, medir columnas con pixeles claros
        var colWhiteRatio = new double[gray.Width];
        gray.ProcessPixelRows(accessor =>
        {
            for (int x = 0; x < gray.Width; x++)
            {
                int white = 0;
                int total = 0;
                for (int y = y0; y <= y1; y++)
                {
                    var row = accessor.GetRowSpan(y);
                    total++;
                    if (row[x].R >= whiteCut) white++;
                }
                colWhiteRatio[x] = total == 0 ? 0 : (double)white / total;
            }
        });

        var colSegs = FindSegments(colWhiteRatio, threshold: 0.12, minLen: Math.Max(20, gray.Width / 25));
        if (colSegs.Count == 0)
            return null;

        var bestCol = colSegs
            .Select(seg => new
            {
                seg.Start,
                seg.End,
                Len = seg.End - seg.Start + 1,
                Avg = colWhiteRatio.Skip(seg.Start).Take(seg.End - seg.Start + 1).Average(),
                Center = (seg.Start + seg.End) / 2.0
            })
            .OrderByDescending(s => s.Avg * s.Len)
            .ThenBy(s => Math.Abs(s.Center - (gray.Width * 0.45)))
            .First();

        int x0 = Math.Max(0, bestCol.Start - 10);
        int x1 = Math.Min(gray.Width - 1, bestCol.End + 10);

        // Expandir un poco hacia abajo/arriba para incluir dígitos completos
        var roi = Rectangle.FromLTRB(x0, y0, x1 + 1, y1 + 1);

        // Validación final
        if (roi.Width < 40 || roi.Height < 20)
            return null;

        return roi;
    }

    private static List<(int Start, int End)> FindSegments(double[] values, double threshold, int minLen)
    {
        var segs = new List<(int, int)>();
        int start = -1;
        for (int i = 0; i < values.Length; i++)
        {
            if (values[i] >= threshold)
            {
                if (start < 0) start = i;
            }
            else
            {
                if (start >= 0)
                {
                    int end = i - 1;
                    if (end - start + 1 >= minLen)
                        segs.Add((start, end));
                    start = -1;
                }
            }
        }
        if (start >= 0)
        {
            int end = values.Length - 1;
            if (end - start + 1 >= minLen)
                segs.Add((start, end));
        }
        return segs;
    }
}
