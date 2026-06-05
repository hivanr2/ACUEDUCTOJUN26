using System.Net.Mail;
using System.Net;
using System.Threading.Tasks;
using ACUEDUCTO2.Share.Models;
using ACUEDUCTO2.Data; // Added correct namespace for ApplicationUser
using Microsoft.Extensions.Options;

namespace ACUEDUCTO2.Services.Email;

public class SmtpEmailSender : CustomEmailSender<ApplicationUser>
{
    private readonly string _smtpServer;
    private readonly int _smtpPort;
    private readonly string _smtpUser;
    private readonly string _smtpPassword;

    public SmtpEmailSender(IOptions<SmtpEmailOptions> options)
    {
        var smtpOptions = options.Value;
        _smtpServer = smtpOptions.SmtpServer;
        _smtpPort = smtpOptions.SmtpPort;
        _smtpUser = smtpOptions.SmtpUser;
        _smtpPassword = smtpOptions.SmtpPassword;
    }

    public async Task SendEmailAsync(ApplicationUser user, string subject, string htmlBody)
    {
        using var smtpClient = new SmtpClient(_smtpServer, _smtpPort)
        {
            Credentials = new NetworkCredential(_smtpUser, _smtpPassword),
            EnableSsl = true
        };

        using var mailMessage = new MailMessage
        {
            From = new MailAddress(_smtpUser),
            Subject = subject,
            Body = htmlBody,
            IsBodyHtml = true
        };

        mailMessage.To.Add(user.Email);

        await smtpClient.SendMailAsync(mailMessage);
    }
}

public class SmtpEmailOptions
{
    public string SmtpServer { get; set; } = string.Empty;
    public int SmtpPort { get; set; } = 587;
    public string SmtpUser { get; set; } = string.Empty;
    public string SmtpPassword { get; set; } = string.Empty;
}