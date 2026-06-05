using System.Threading;
using System.Threading.Tasks;

namespace ACUEDUCTO.Services;

public interface IWhatsAppService
{
    Task SendMessageAsync(string toE164, string message, CancellationToken cancellationToken = default);
    Task SendDocumentAsync(string toE164, byte[] documentBytes, string fileName, string caption = null, CancellationToken cancellationToken = default);
}