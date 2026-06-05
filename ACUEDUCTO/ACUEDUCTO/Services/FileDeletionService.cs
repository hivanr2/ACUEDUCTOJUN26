using Microsoft.AspNetCore.Hosting;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ACUEDUCTO.Services
{
    public class FileDeletionService
    {
        private readonly IWebHostEnvironment _env;

        public FileDeletionService(IWebHostEnvironment env)
        {
            _env = env;
        }

        public async Task<DeletionResult> DeleteSpecificPdfsAsync(string codigoCliente, string prefix)
        {
            var result = new DeletionResult { CodigoCliente = codigoCliente };

            string folderPath = Path.Combine(_env.WebRootPath, "imagenes", codigoCliente);

            if (!Directory.Exists(folderPath))
            {
                result.Message = $"Carpeta no existe: {folderPath}";
                return result;
            }

            var pdfFiles = Directory.GetFiles(folderPath, "*.pdf");

            foreach (var file in pdfFiles)
            {
                var fileName = Path.GetFileName(file);
                if (fileName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        File.Delete(file);
                        result.DeletedFiles.Add(fileName);
                    }
                    catch (Exception ex)
                    {
                        result.Errors.Add($"Error eliminando {fileName}: {ex.Message}");
                    }
                }
            }

            result.TotalFiles = pdfFiles.Length;
            result.DeletedCount = result.DeletedFiles.Count;

            return result;
        }

        public class DeletionResult
        {
            public string CodigoCliente { get; set; } = string.Empty;
            public int TotalFiles { get; set; }
            public int DeletedCount { get; set; }
            public List<string> DeletedFiles { get; set; } = new();
            public List<string> Errors { get; set; } = new();
            public string? Message { get; set; }
        }
    }
}