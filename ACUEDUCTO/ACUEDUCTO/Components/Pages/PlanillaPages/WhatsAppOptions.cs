namespace ACUEDUCTO.Services;

public sealed class WhatsAppOptions
{
    public string AccessToken { get; set; } = string.Empty;    // Permanent or long-lived token
    public string PhoneNumberId { get; set; } = string.Empty;  // From WhatsApp Business phone
    public string ApiVersion { get; set; } = "v20.0";          // Graph API version
}