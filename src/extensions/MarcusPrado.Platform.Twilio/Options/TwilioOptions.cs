namespace MarcusPrado.Platform.Twilio.Options;

/// <summary>Configuration for the Twilio SMS adapter.</summary>
public sealed class TwilioOptions
{
    /// <summary>Gets or sets the Twilio Account SID.</summary>
    public string AccountSid { get; set; } = string.Empty;

    /// <summary>Gets or sets the Twilio Auth Token.</summary>
    public string AuthToken { get; set; } = string.Empty;

    /// <summary>Gets or sets the default sender phone number (E.164 format).</summary>
    public string DefaultFrom { get; set; } = string.Empty;
}
