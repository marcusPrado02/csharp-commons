using MailKit.Net.Smtp;
using MailKit.Security;
using MarcusPrado.Platform.Abstractions.Email;
using MarcusPrado.Platform.MailKit.Options;
using MimeKit;

namespace MarcusPrado.Platform.MailKit.Email;

/// <summary>Sends emails via SMTP using MailKit.</summary>
public sealed class MailKitEmailSender : IEmailSender
{
    private readonly MailKitOptions _options;
    private readonly ISmtpClient _smtp;

    /// <summary>Initializes a new instance of <see cref="MailKitEmailSender"/>.</summary>
    public MailKitEmailSender(MailKitOptions options, ISmtpClient smtp)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(smtp);
        _options = options;
        _smtp = smtp;
    }

    /// <inheritdoc />
    public async Task<EmailResult> SendAsync(EmailMessage message, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        try
        {
            var mime = BuildMimeMessage(message);
            await ConnectAndSendAsync(mime, ct).ConfigureAwait(false);
            return new EmailResult(true, mime.MessageId);
        }
#pragma warning disable CA1031
        catch (Exception ex)
#pragma warning restore CA1031
        {
            return new EmailResult(false, null, ex.Message);
        }
    }

    /// <inheritdoc />
    public async Task<EmailResult> SendBulkAsync(
        IReadOnlyList<EmailMessage> messages, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(messages);

        if (messages.Count == 0)
            return new EmailResult(true);

        try
        {
            var secureOptions = _options.UseSsl
                ? SecureSocketOptions.SslOnConnect
                : SecureSocketOptions.StartTlsWhenAvailable;

            await _smtp.ConnectAsync(_options.SmtpHost, _options.SmtpPort, secureOptions, ct)
                .ConfigureAwait(false);

            if (!string.IsNullOrEmpty(_options.Username))
            {
                await _smtp.AuthenticateAsync(_options.Username, _options.Password, ct)
                    .ConfigureAwait(false);
            }

            string? lastMessageId = null;
            foreach (var msg in messages)
            {
                var mime = BuildMimeMessage(msg);
                await _smtp.SendAsync(mime, ct).ConfigureAwait(false);
                lastMessageId = mime.MessageId;
            }

            await _smtp.DisconnectAsync(true, ct).ConfigureAwait(false);
            return new EmailResult(true, lastMessageId);
        }
#pragma warning disable CA1031
        catch (Exception ex)
#pragma warning restore CA1031
        {
            return new EmailResult(false, null, ex.Message);
        }
    }

    private async Task ConnectAndSendAsync(MimeMessage mime, CancellationToken ct)
    {
        var secureOptions = _options.UseSsl
            ? SecureSocketOptions.SslOnConnect
            : SecureSocketOptions.StartTlsWhenAvailable;

        await _smtp.ConnectAsync(_options.SmtpHost, _options.SmtpPort, secureOptions, ct)
            .ConfigureAwait(false);

        if (!string.IsNullOrEmpty(_options.Username))
        {
            await _smtp.AuthenticateAsync(_options.Username, _options.Password, ct)
                .ConfigureAwait(false);
        }

        await _smtp.SendAsync(mime, ct).ConfigureAwait(false);
        await _smtp.DisconnectAsync(true, ct).ConfigureAwait(false);
    }

    private MimeMessage BuildMimeMessage(EmailMessage message)
    {
        var mime = new MimeMessage();
        mime.From.Add(MailboxAddress.Parse(message.From ?? _options.DefaultFrom));
        mime.To.Add(MailboxAddress.Parse(message.To));

        if (message.Cc is not null)
        {
            foreach (var cc in message.Cc)
            {
                mime.Cc.Add(MailboxAddress.Parse(cc));
            }
        }

        if (message.Bcc is not null)
        {
            foreach (var bcc in message.Bcc)
            {
                mime.Bcc.Add(MailboxAddress.Parse(bcc));
            }
        }

        mime.Subject = message.Subject;

        var builder = new BodyBuilder();
        if (message.IsHtml)
            builder.HtmlBody = message.Body;
        else
            builder.TextBody = message.Body;

        if (message.Attachments is not null)
        {
            foreach (var att in message.Attachments)
            {
                builder.Attachments.Add(
                    att.FileName,
                    att.Content,
                    global::MimeKit.ContentType.Parse(att.ContentType));
            }
        }

        mime.Body = builder.ToMessageBody();
        return mime;
    }
}
