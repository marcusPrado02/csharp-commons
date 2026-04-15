namespace MarcusPrado.Platform.Abstractions.Email;

/// <summary>An outgoing email message.</summary>
public sealed record EmailMessage(
    string To,
    string Subject,
    string Body,
    bool IsHtml = true,
    string? From = null,
    IReadOnlyList<string>? Cc = null,
    IReadOnlyList<string>? Bcc = null,
    IReadOnlyList<EmailAttachment>? Attachments = null
);

/// <summary>A binary attachment to include in an email.</summary>
public sealed record EmailAttachment(string FileName, byte[] Content, string ContentType = "application/octet-stream");

/// <summary>The result of an email send operation.</summary>
public sealed record EmailResult(bool Success, string? MessageId = null, string? Error = null);
