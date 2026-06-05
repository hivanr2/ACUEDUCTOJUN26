using ACUEDUCTO2.Data;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Identity;
using MimeKit;

namespace ACUEDUCTO2.Components.Account;

internal sealed class SmtpEmailSender : IEmailSender<ApplicationUser>
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<SmtpEmailSender> _logger;

    public SmtpEmailSender(IConfiguration configuration, ILogger<SmtpEmailSender> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public Task SendConfirmationLinkAsync(ApplicationUser user, string email, string confirmationLink) =>
        SendEmailAsync(email, "Confirma tu cuenta – ACUEDUCTO", BuildConfirmationEmail(confirmationLink));

    public Task SendPasswordResetLinkAsync(ApplicationUser user, string email, string resetLink) =>
        SendEmailAsync(email, "Restablecer contraseña – ACUEDUCTO", BuildPasswordResetEmail(resetLink));

    public Task SendPasswordResetCodeAsync(ApplicationUser user, string email, string resetCode) =>
        SendEmailAsync(email, "Código para restablecer contraseña – ACUEDUCTO", BuildPasswordResetCodeEmail(resetCode));

    private async Task SendEmailAsync(string toEmail, string subject, string htmlBody)
    {
        var smtpServer = _configuration["EmailSettings:SmtpServer"]!;
        var smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"]!);
        var smtpUser = _configuration["EmailSettings:SmtpUser"]!;
        var smtpPassword = _configuration["EmailSettings:SmtpPassword"]!;

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("ACUEDUCTO", smtpUser));
        message.To.Add(MailboxAddress.Parse(toEmail));
        message.Subject = subject;
        message.Body = new TextPart("html") { Text = htmlBody };

        using var client = new SmtpClient();

        try
        {
            await client.ConnectAsync(smtpServer, smtpPort, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(smtpUser, smtpPassword);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
            _logger.LogInformation("Email sent to {Email}", toEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Email}", toEmail);
            throw;
        }
    }

    // ──────────────── Branded HTML templates ────────────────

    private static string BuildConfirmationEmail(string confirmationLink) => $"""
        <!DOCTYPE html>
        <html lang="es">
        <head><meta charset="UTF-8"><meta name="viewport" content="width=device-width,initial-scale=1"></head>
        <body style="margin:0;padding:0;background:#f8fafc;font-family:system-ui,-apple-system,Segoe UI,Roboto,Helvetica,Arial,sans-serif;">
          <table role="presentation" width="100%" cellpadding="0" cellspacing="0" style="background:radial-gradient(900px 600px at 15% 10%,rgba(29,155,240,0.10),transparent 55%),linear-gradient(135deg,#f8fafc,#eef2ff);">
            <tr><td align="center" style="padding:40px 16px;">
              <table role="presentation" width="560" cellpadding="0" cellspacing="0" style="max-width:560px;width:100%;background:rgba(255,255,255,0.96);border-radius:12px;box-shadow:0 10px 30px rgba(2,6,23,0.08);overflow:hidden;">
                <!-- Header -->
                <tr>
                  <td style="background:linear-gradient(90deg,#072a5e,#0b3a82);padding:28px 32px;text-align:center;">
                    <h1 style="margin:0;color:#fff;font-size:1.6rem;font-weight:800;letter-spacing:0.4px;">💧 ACUEDUCTO</h1>
                  </td>
                </tr>
                <!-- Body -->
                <tr>
                  <td style="padding:36px 32px 16px;">
                    <h2 style="margin:0 0 12px;color:#0f172a;font-size:1.25rem;">¡Bienvenido!</h2>
                    <p style="margin:0 0 24px;color:#475569;font-size:0.95rem;line-height:1.6;">
                      Gracias por registrarte en <strong>ACUEDUCTO</strong>. Para activar tu cuenta, confirma tu dirección de correo electrónico haciendo clic en el siguiente botón:
                    </p>
                    <table role="presentation" cellpadding="0" cellspacing="0" style="margin:0 auto 28px;">
                      <tr>
                        <td style="border-radius:10px;background-color:#0b3a82;background:linear-gradient(135deg,#0b3a82,#1d9bf0);box-shadow:0 8px 18px rgba(11,58,130,0.22);">
                          <a href="{confirmationLink}" target="_blank"
                             style="display:inline-block;padding:14px 36px;background-color:#0b3a82;color:#ffffff;font-size:1rem;font-weight:600;text-decoration:none;border-radius:10px;">
                            Confirmar mi cuenta
                          </a>
                        </td>
                      </tr>
                    </table>
                    <p style="margin:0 0 8px;color:#475569;font-size:0.85rem;line-height:1.5;">
                      Si el botón no funciona, copia y pega el siguiente enlace en tu navegador:
                    </p>
                    <p style="margin:0 0 24px;word-break:break-all;color:#0b3a82;font-size:0.82rem;">{confirmationLink}</p>
                  </td>
                </tr>
                <!-- Footer -->
                <tr>
                  <td style="padding:20px 32px;border-top:1px solid rgba(15,23,42,0.08);text-align:center;">
                    <p style="margin:0;color:#94a3b8;font-size:0.78rem;">
                      Si no creaste esta cuenta, puedes ignorar este mensaje.
                    </p>
                    <p style="margin:8px 0 0;color:#cbd5e1;font-size:0.72rem;">© ACUEDUCTO — Todos los derechos reservados</p>
                  </td>
                </tr>
              </table>
            </td></tr>
          </table>
        </body>
        </html>
        """;

    private static string BuildPasswordResetEmail(string resetLink) => $"""
        <!DOCTYPE html>
        <html lang="es">
        <head><meta charset="UTF-8"><meta name="viewport" content="width=device-width,initial-scale=1"></head>
        <body style="margin:0;padding:0;background:#f8fafc;font-family:system-ui,-apple-system,Segoe UI,Roboto,Helvetica,Arial,sans-serif;">
          <table role="presentation" width="100%" cellpadding="0" cellspacing="0" style="background:radial-gradient(900px 600px at 15% 10%,rgba(29,155,240,0.10),transparent 55%),linear-gradient(135deg,#f8fafc,#eef2ff);">
            <tr><td align="center" style="padding:40px 16px;">
              <table role="presentation" width="560" cellpadding="0" cellspacing="0" style="max-width:560px;width:100%;background:rgba(255,255,255,0.96);border-radius:12px;box-shadow:0 10px 30px rgba(2,6,23,0.08);overflow:hidden;">
                <tr>
                  <td style="background:linear-gradient(90deg,#072a5e,#0b3a82);padding:28px 32px;text-align:center;">
                    <h1 style="margin:0;color:#fff;font-size:1.6rem;font-weight:800;letter-spacing:0.4px;">💧 ACUEDUCTO</h1>
                  </td>
                </tr>
                <tr>
                  <td style="padding:36px 32px 16px;">
                    <h2 style="margin:0 0 12px;color:#0f172a;font-size:1.25rem;">Restablecer contraseña</h2>
                    <p style="margin:0 0 24px;color:#475569;font-size:0.95rem;line-height:1.6;">
                      Recibimos una solicitud para restablecer tu contraseña. Haz clic en el siguiente botón para continuar:
                    </p>
                    <table role="presentation" cellpadding="0" cellspacing="0" style="margin:0 auto 28px;">
                      <tr>
                        <td style="border-radius:10px;background-color:#0b3a82;background:linear-gradient(135deg,#0b3a82,#1d9bf0);box-shadow:0 8px 18px rgba(11,58,130,0.22);">
                          <a href="{resetLink}" target="_blank"
                             style="display:inline-block;padding:14px 36px;background-color:#0b3a82;color:#ffffff;font-size:1rem;font-weight:600;text-decoration:none;border-radius:10px;">
                            Restablecer contraseña
                          </a>
                        </td>
                      </tr>
                    </table>
                    <p style="margin:0 0 8px;color:#475569;font-size:0.85rem;line-height:1.5;">
                      Si no solicitaste este cambio, puedes ignorar este mensaje.
                    </p>
                  </td>
                </tr>
                <tr>
                  <td style="padding:20px 32px;border-top:1px solid rgba(15,23,42,0.08);text-align:center;">
                    <p style="margin:0;color:#cbd5e1;font-size:0.72rem;">© ACUEDUCTO — Todos los derechos reservados</p>
                  </td>
                </tr>
              </table>
            </td></tr>
          </table>
        </body>
        </html>
        """;

    private static string BuildPasswordResetCodeEmail(string resetCode) => $"""
        <!DOCTYPE html>
        <html lang="es">
        <head><meta charset="UTF-8"><meta name="viewport" content="width=device-width,initial-scale=1"></head>
        <body style="margin:0;padding:0;background:#f8fafc;font-family:system-ui,-apple-system,Segoe UI,Roboto,Helvetica,Arial,sans-serif;">
          <table role="presentation" width="100%" cellpadding="0" cellspacing="0" style="background:radial-gradient(900px 600px at 15% 10%,rgba(29,155,240,0.10),transparent 55%),linear-gradient(135deg,#f8fafc,#eef2ff);">
            <tr><td align="center" style="padding:40px 16px;">
              <table role="presentation" width="560" cellpadding="0" cellspacing="0" style="max-width:560px;width:100%;background:rgba(255,255,255,0.96);border-radius:12px;box-shadow:0 10px 30px rgba(2,6,23,0.08);overflow:hidden;">
                <tr>
                  <td style="background:linear-gradient(90deg,#072a5e,#0b3a82);padding:28px 32px;text-align:center;">
                    <h1 style="margin:0;color:#fff;font-size:1.6rem;font-weight:800;letter-spacing:0.4px;">💧 ACUEDUCTO</h1>
                  </td>
                </tr>
                <tr>
                  <td style="padding:36px 32px 16px;">
                    <h2 style="margin:0 0 12px;color:#0f172a;font-size:1.25rem;">Código de restablecimiento</h2>
                    <p style="margin:0 0 24px;color:#475569;font-size:0.95rem;line-height:1.6;">
                      Usa el siguiente código para restablecer tu contraseña:
                    </p>
                    <table role="presentation" cellpadding="0" cellspacing="0" style="margin:0 auto 28px;">
                      <tr>
                        <td style="padding:16px 40px;border-radius:10px;background:#eef2ff;border:2px solid rgba(11,58,130,0.15);text-align:center;">
                          <span style="font-size:1.6rem;font-weight:800;letter-spacing:4px;color:#0b3a82;">{resetCode}</span>
                        </td>
                      </tr>
                    </table>
                    <p style="margin:0 0 8px;color:#475569;font-size:0.85rem;line-height:1.5;">
                      Si no solicitaste este cambio, puedes ignorar este mensaje.
                    </p>
                  </td>
                </tr>
                <tr>
                  <td style="padding:20px 32px;border-top:1px solid rgba(15,23,42,0.08);text-align:center;">
                    <p style="margin:0;color:#cbd5e1;font-size:0.72rem;">© ACUEDUCTO — Todos los derechos reservados</p>
                  </td>
                </tr>
              </table>
            </td></tr>
          </table>
        </body>
        </html>
        """;
}
