namespace MarcusPrado.Platform.SendGrid.Options;

/// <summary>Configuration for the SendGrid email adapter.</summary>
public sealed class SendGridOptions
{
    /// <summary>Gets or sets the SendGrid API key.</summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>Gets or sets the default "From" address when none is specified on the message.</summary>
    public string DefaultFrom { get; set; } = "noreply@example.com";

    /// <summary>Gets or sets the default "From" display name.</summary>
    public string DefaultFromName { get; set; } = string.Empty;
}
