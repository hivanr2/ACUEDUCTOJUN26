using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using System.Text.RegularExpressions;

namespace ACUEDUCTO.Services
{
    public class PdfProcessorService
    {
        private readonly IWebHostEnvironment _env;

        public PdfProcessorService(IWebHostEnvironment env)
        {
            _env = env;
        }

        public async Task<ProcessResult> ProcessFacturasPdfs()
        {
            string facturasPath = Path.Combine(_env.WebRootPath, "facturas");
            string imagenesPath = Path.Combine(_env.WebRootPath, "imagenes");

            var result = new ProcessResult();

            if (!Directory.Exists(facturasPath))
            {
                result.Error = $"La carpeta {facturasPath} no existe.";
                return result;
            }

            var pdfFiles = Directory.GetFiles(facturasPath, "*.pdf");
            result.TotalFiles = pdfFiles.Length;

            foreach (var pdfFile in pdfFiles)
            {
                try
                {
                    var (codigo, factura, text) = ExtractCodesFromPdf(pdfFile);

                    if (!string.IsNullOrEmpty(codigo) && !string.IsNullOrEmpty(factura))
                    {
                        // divide "-" el codigo y toma la primera parte y la segunda parte  incluido el guion
                        var codigoParts = codigo.Split('-');
                        if (codigoParts.Length >= 2)
                        {
                            string codigoParte1 = codigoParts[0];
                            string codigoParte2 = codigoParts[1];
                            codigo = $"{codigoParte1}-{codigoParte2}";
                        }

                        string targetDir = Path.Combine(imagenesPath, codigo);
                        Directory.CreateDirectory(targetDir);

                        // divide "-" factura y toma la primera parte "06" y el resto dejalo igual
                        var facturaParts = factura.Split('-');
                        if (facturaParts.Length >= 2)
                        {
                            string facturaParte1 = facturaParts[0];
                            string facturaParte2 = facturaParts[1];
                            string facturaParte3 = facturaParts[2];
                            factura = $"{facturaParte1}-{facturaParte2}-{facturaParte3}";
                            //factura = $"06-{facturaParte2}-{facturaParte3}";
                        }

                        string targetFile = Path.Combine(targetDir, $"{factura}.pdf");

                        File.Copy(pdfFile, targetFile, true); // Overwrite if exists
                        result.ProcessedFiles++;
                    }
                    else
                    {
                        result.Errors.Add($"No se pudieron extraer códigos de {Path.GetFileName(pdfFile)}. Texto extraído (primeros 500 chars): {text.Substring(0, Math.Min(500, text.Length))}");
                    }
                }
                catch (Exception ex)
                {
                    result.Errors.Add($"Error procesando {Path.GetFileName(pdfFile)}: {ex.Message}");
                }
            }

            return result;
        }

        private (string codigo, string factura, string text) ExtractCodesFromPdf(string pdfPath)
        {
            using var pdfReader = new PdfReader(pdfPath);
            using var pdfDoc = new PdfDocument(pdfReader);

            var strategy = new LocationTextExtractionStrategy();
            string text = "";

            for (int i = 1; i <= pdfDoc.GetNumberOfPages(); i++)
            {
                var page = pdfDoc.GetPage(i);
                text += PdfTextExtractor.GetTextFromPage(page, strategy);
            }

            // Extract Código
            var codigoMatch = Regex.Match(text, @"CODIGO N°[:\s]*([^\s]+-[^\s]+)", RegexOptions.IgnoreCase);
            string codigo = codigoMatch.Success ? codigoMatch.Groups[1].Value.Trim() : "";

            // Extract Factura
            var facturaMatch = Regex.Match(text, @"FACTURA NO\.[:\s]*([^\s]+)", RegexOptions.IgnoreCase);
            string factura = facturaMatch.Success ? facturaMatch.Groups[1].Value.Trim() : "";

            return (codigo, factura, text);
        }

        public class ProcessResult
        {
            public int TotalFiles { get; set; }
            public int ProcessedFiles { get; set; }
            public List<string> Errors { get; set; } = new();
            public string? Error { get; set; }
        }
    }
}