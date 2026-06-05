using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;

namespace ACUEDUCTO2.Services.Email;

public sealed class EmailService
{
    private readonly SmtpEmailOptions _options;

    public EmailService(IOptions<SmtpEmailOptions> options)
    {
        _options = options.Value;
    }

    public async Task SendEmailAsync(string to, string subject, string htmlBody, byte[]? attachment = null, string? attachmentName = null)
    {
        using var smtpClient = new SmtpClient(_options.SmtpServer, _options.SmtpPort)
        {
            Credentials = new NetworkCredential(_options.SmtpUser, _options.SmtpPassword),
            EnableSsl = true
        };

        using var message = new MailMessage
        {
            From = new MailAddress(_options.SmtpUser),
            Subject = subject,
            Body = htmlBody,
            IsBodyHtml = true
        };
        message.To.Add(to);

        if (attachment is { Length: > 0 })
        {
            var stream = new MemoryStream(attachment);
            message.Attachments.Add(new Attachment(stream, attachmentName ?? "factura.pdf", "application/pdf"));
        }

        await smtpClient.SendMailAsync(message);
    }
}
