using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ACUEDUCTO2.Services
{
    public interface IConsumoOcrService
    {
        Task<(string? ocrText, double? lectura, string? numeroMedidor, double? confianza)> ReadAsync(Stream imageStream, CancellationToken cancellationToken = default);
    }
}
