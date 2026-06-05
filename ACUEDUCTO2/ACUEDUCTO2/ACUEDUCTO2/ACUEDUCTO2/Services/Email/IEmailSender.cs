namespace ACUEDUCTO2.Services.Email;

public interface CustomEmailSender<TUser>
{
    Task SendEmailAsync(TUser user, string subject, string htmlBody);
}