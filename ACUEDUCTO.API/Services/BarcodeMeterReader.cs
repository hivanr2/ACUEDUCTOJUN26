using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using ZXing;
using ZXing.Common;

namespace ACUEDUCTO.API.Services;

internal static class BarcodeMeterReader
{
    public static string? TryReadBarcode(byte[] imageBytes)
    {
        try
        {
            using var image = Image.Load<Rgba32>(imageBytes);

            // Convert to RGB buffer for ZXing
            var pixels = new byte[image.Width * image.Height * 3];
            int i = 0;
            image.ProcessPixelRows(accessor =>
            {
                for (int y = 0; y < accessor.Height; y++)
                {
                    var row = accessor.GetRowSpan(y);
                    for (int x = 0; x < row.Length; x++)
                    {
                        var p = row[x];
                        pixels[i++] = p.R;
                        pixels[i++] = p.G;
                        pixels[i++] = p.B;
                    }
                }
            });

            var source = new RGBLuminanceSource(pixels, image.Width, image.Height);
            var binary = new BinaryBitmap(new HybridBinarizer(source));

            var reader = new MultiFormatReader();
            var hints = new Dictionary<DecodeHintType, object>
            {
                { DecodeHintType.TRY_HARDER, true },
                { DecodeHintType.POSSIBLE_FORMATS, new List<BarcodeFormat>
                    {
                        BarcodeFormat.CODE_128,
                        BarcodeFormat.CODE_39,
                        BarcodeFormat.CODE_93,
                        BarcodeFormat.EAN_13,
                        BarcodeFormat.EAN_8,
                        BarcodeFormat.ITF,
                        BarcodeFormat.CODABAR,
                        BarcodeFormat.QR_CODE,
                        BarcodeFormat.DATA_MATRIX,
                        BarcodeFormat.PDF_417
                    }
                }
            };
            reader.Hints = hints;

            var result = reader.decode(binary);
            return string.IsNullOrWhiteSpace(result?.Text) ? null : result.Text.Trim();
        }
        catch
        {
            return null;
        }
    }
}
