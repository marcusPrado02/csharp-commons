namespace MarcusPrado.Platform.MailKit.Options;

/// <summary>Configuration for the MailKit SMTP email sender.</summary>
public sealed class MailKitOptions
{
    /// <summary>Gets or sets the SMTP host name.</summary>
    public string SmtpHost { get; set; } = "localhost";

    /// <summary>Gets or sets the SMTP port number.</summary>
    public int SmtpPort { get; set; } = 587;

    /// <summary>Gets or sets a value indicating whether to use SSL.</summary>
    public bool UseSsl { get; set; }

    /// <summary>Gets or sets the SMTP username.</summary>
    public string? Username { get; set; }

    /// <summary>Gets or sets the SMTP password.</summary>
    public string? Password { get; set; }

    /// <summary>Gets or sets the default sender address used when <see cref="Abstractions.Email.EmailMessage.From"/> is null.</summary>
    public string DefaultFrom { get; set; } = "no-reply@platform.local";

    /// <summary>Gets or sets the directory that contains HTML template files.</summary>
    public string TemplateDirectory { get; set; } = "Templates";
}
