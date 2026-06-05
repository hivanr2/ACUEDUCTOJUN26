using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using PdfSharpCore.Pdf.IO;
using System.Globalization;

class Program
{
    static void Main(string[] args)
    {
        // Paths
        var inputPdf = @"C:\Users\Administrator\source\repos\ACUEDUCTO09\ACUEDUCTO\ACUEDUCTO\Components\Shared\Red_Acueduto.TobiaChica.Satelital2.pdf";
        var outputPdf = @"C:\Users\Administrator\source\repos\ACUEDUCTO09\ACUEDUCTO\ACUEDUCTO\Components\Shared\Share\Red_Acueduto.TobiaChica.Satelital2.modified.pdf";

        // Define new points: Label, lon, lat (WGS84). Modified labels to Galvis* as requested.
        var points = new[]
        {
            ("Galvis1", -75.12345, 5.67890),
            ("Galvis2", -75.12600, 5.67950),
            ("Galvis3", -75.12450, 5.68000)
        };

        // Open PDF
        using var input = PdfReader.Open(inputPdf, PdfDocumentOpenMode.Modify);

        // For simplicity, draw points on first page
        var page = input.Pages[0];
        var gfx = XGraphics.FromPdfPage(page, XGraphicsUnit.Point);

        // Simple transform: we'll map lon/lat to page coords using bounding box (manual values) - user must adapt for real georef
        // Bounding box of map on page (left lon, right lon, top lat, bottom lat)
        double leftLon = -75.13000, rightLon = -75.12000, topLat = 5.68500, bottomLat = 5.67500;

        double pageWidth = page.Width.Point;
        double pageHeight = page.Height.Point;

        var pen = new XPen(XColors.Red, 4);
        var font = new XFont("Arial", 10);

        foreach (var (label, lon, lat) in points)
        {
            double x = (lon - leftLon) / (rightLon - leftLon) * pageWidth;
            double y = (topLat - lat) / (topLat - bottomLat) * pageHeight;

            gfx.DrawEllipse(pen, x - 3, y - 3, 6, 6);
            gfx.DrawString(label, font, XBrushes.Black, x + 5, y - 5);
        }

        input.Save(outputPdf);

        System.Console.WriteLine("Saved modified PDF to: " + outputPdf);
    }
}
