namespace MarcusPrado.Platform.Abstractions.Email;

/// <summary>Sends transactional email messages.</summary>
public interface IEmailSender
{
    /// <summary>Sends a single email message.</summary>
    Task<EmailResult> SendAsync(EmailMessage message, CancellationToken ct = default);

    /// <summary>Sends multiple email messages in a single batch.</summary>
    Task<EmailResult> SendBulkAsync(IReadOnlyList<EmailMessage> messages, CancellationToken ct = default);
}

/// <summary>Renders email bodies from named templates.</summary>
public interface IEmailTemplateRenderer
{
    /// <summary>Renders the named template using the given model and returns the HTML/text body.</summary>
    Task<string> RenderAsync(string templateName, object model, CancellationToken ct = default);
}
