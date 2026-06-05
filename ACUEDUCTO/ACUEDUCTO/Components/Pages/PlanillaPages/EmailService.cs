using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Text.Encodings.Web;
using System.Collections.Generic;

public class EmailService
{
    private readonly string _smtpServer;
    private readonly int _smtpPort;
    private readonly string _smtpUser;
    private readonly string _smtpPassword;

    public EmailService(string smtpServer, int smtpPort, string smtpUser, string smtpPassword)
    {
        _smtpServer = smtpServer;
        _smtpPort = smtpPort;
        _smtpUser = smtpUser;
        _smtpPassword = smtpPassword;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string htmlBody, byte[] attachment = null, string attachmentName = "Factura.pdf")
    {
        await SendEmailAsync(toEmail, subject, htmlBody, inlineImageUrl: null, inlineImageContentId: null, attachment, attachmentName);
    }

    // New overload that supports a single inline image via CID
    public async Task SendEmailAsync(
        string toEmail,
        string subject,
        string htmlBody,
        string inlineImageUrl,
        string inlineImageContentId,
        byte[] attachment = null,
        string attachmentName = "Factura.pdf")
    {
        try
        {


            MailMessage message = new MailMessage();

            //if (!string.IsNullOrWhiteSpace(inlineImageUrl) && !string.IsNullOrWhiteSpace(inlineImageContentId))
            //{
            //    var htmlView = AlternateView.CreateAlternateViewFromString(htmlBody, null, MediaTypeNames.Text.Html);

            //    // Download image bytes
            //    using var http = new HttpClient();
            //    var imageBytes = await http.GetByteArrayAsync(inlineImageUrl);

            //    var mediaType = GetMediaTypeFromUrl(inlineImageUrl);
            //    var linkedResource = new LinkedResource(new MemoryStream(imageBytes), mediaType)
            //    {
            //        ContentId = inlineImageContentId,
            //        TransferEncoding = TransferEncoding.Base64,
            //        ContentType = { Name = inlineImageContentId }
            //    };

            //    // Some clients require this to be set to inline
            //    linkedResource.ContentLink = new Uri($"cid:{inlineImageContentId}");
            //    htmlView.LinkedResources.Add(linkedResource);

            //    message.AlternateViews.Add(htmlView);
            //}



            message.From = new MailAddress("facturacionacueductotobiachica@gmail.com");
            message.To.Add(new MailAddress(toEmail));
            message.Subject = subject;// "Test Email";
            message.Body = htmlBody;// "This is a test email sent from ASP.NET"; // Set up the SMTP client
            message.IsBodyHtml = true;
            //message.Attachments = ;

            if (attachment != null)
            {
                var memoryStream = new MemoryStream(attachment);
                var attachmentItem = new Attachment(memoryStream, attachmentName);
                message.Attachments.Add(attachmentItem);
            }

            SmtpClient smtp = new SmtpClient();
            smtp.Host = "smtp.gmail.com"; // your SMTP server
            smtp.Port = 587; // your SMTP port
            smtp.EnableSsl = true; // enable SSL if required
            smtp.Timeout = 30000; // 30 seconds timeout
            smtp.Credentials = new System.Net.NetworkCredential("facturacionacueductotobiachica@gmail.com", "hdtk ysbf gxyd cmaj"); // marzo 2026 your SMTP login credentials 
            // Send the email
            await smtp.SendMailAsync(message);


            //using var smtpClient = new SmtpClient(_smtpServer, _smtpPort)
            //{
            //    Credentials = new NetworkCredential(_smtpUser, _smtpPassword),
            //    EnableSsl = true
            //};

            //using var mailMessage = new MailMessage
            //{
            //    From = new MailAddress(_smtpUser),
            //    Subject = subject,
            //    Body = htmlBody,
            //    IsBodyHtml = true
            //};

            //mailMessage.To.Add(toEmail);

            // If an inline image is requested, add it as a LinkedResource with a CID
            //if (!string.IsNullOrWhiteSpace(inlineImageUrl) && !string.IsNullOrWhiteSpace(inlineImageContentId))
            //{
            //    var htmlView = AlternateView.CreateAlternateViewFromString(htmlBody, null, MediaTypeNames.Text.Html);

            //    // Download image bytes
            //    using var http = new HttpClient();
            //    var imageBytes = await http.GetByteArrayAsync(inlineImageUrl);

            //    var mediaType = GetMediaTypeFromUrl(inlineImageUrl);
            //    var linkedResource = new LinkedResource(new MemoryStream(imageBytes), mediaType)
            //    {
            //        ContentId = inlineImageContentId,
            //        TransferEncoding = TransferEncoding.Base64,
            //        ContentType = { Name = inlineImageContentId }
            //    };
            //
            //    // Some clients require this to be set to inline
            //    linkedResource.ContentLink = new Uri($"cid:{inlineImageContentId}");
            //    htmlView.LinkedResources.Add(linkedResource);

            //    mailMessage.AlternateViews.Add(htmlView);
            //}

            //if (attachment != null)
            //{
            //    var memoryStream = new MemoryStream(attachment);
            //    var attachmentItem = new Attachment(memoryStream, attachmentName);
            //    mailMessage.Attachments.Add(attachmentItem);
            //}

            //await smtpClient.SendMailAsync(mailMessage);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending email: {ex.Message}");
            throw;
        }
    }

    // New overload that supports multiple inline images via CID
    public async Task SendEmailAsync(string toEmail, string subject, string htmlBody, Dictionary<string, byte[]> inlineImages, byte[] attachment = null, string attachmentName = "Factura.pdf")
    {
        try
        {
            using (MailMessage message = new MailMessage())
            {
                message.From = new MailAddress("facturacionacueductotobiachica@gmail.com");
                message.To.Add(new MailAddress(toEmail));
                message.Subject = subject;

                var htmlView = AlternateView.CreateAlternateViewFromString(htmlBody, null, MediaTypeNames.Text.Html);
                if (inlineImages != null && inlineImages.Count > 0)
                {
                    foreach (var kvp in inlineImages)
                    {
                        var linkedResource = new LinkedResource(new MemoryStream(kvp.Value), "image/jpeg")
                        {
                            ContentId = kvp.Key,
                            TransferEncoding = TransferEncoding.Base64
                        };
                        htmlView.LinkedResources.Add(linkedResource);
                    }
                }
                message.AlternateViews.Add(htmlView);

                if (attachment != null)
                {
                    var memoryStream = new MemoryStream(attachment);
                    var attachmentItem = new Attachment(memoryStream, attachmentName);
                    message.Attachments.Add(attachmentItem);
                }

                using (SmtpClient smtp = new SmtpClient())
                {
                    smtp.Host = "smtp.gmail.com";
                    smtp.Port = 587;
                    smtp.EnableSsl = true;
                    smtp.Timeout = 30000; // 30 seconds timeout
                    smtp.Credentials = new System.Net.NetworkCredential("facturacionacueductotobiachica@gmail.com", "hbic brlb lhgj zmfp");
                    await smtp.SendMailAsync(message);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending email: {ex.Message}");
            throw;
        }
    }

    private static string GetMediaTypeFromUrl(string url)
    {
        if (url.EndsWith(".png", StringComparison.OrdinalIgnoreCase)) return "image/png";
        if (url.EndsWith(".gif", StringComparison.OrdinalIgnoreCase)) return "image/gif";
        if (url.EndsWith(".webp", StringComparison.OrdinalIgnoreCase)) return "image/webp";
        return "image/jpeg";
    }
}
