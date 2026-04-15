using MarcusPrado.Platform.Abstractions.Email;
using MarcusPrado.Platform.SendGrid.Options;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace MarcusPrado.Platform.SendGrid.Email;

/// <summary>Implements <see cref="IEmailSender"/> via the SendGrid v3 API.</summary>
public sealed class SendGridEmailSender : IEmailSender
{
    private readonly ISendGridClient _client;
    private readonly SendGridOptions _options;

    /// <summary>Initializes a new instance of <see cref="SendGridEmailSender"/>.</summary>
    public SendGridEmailSender(ISendGridClient client, SendGridOptions options)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(options);
        _client = client;
        _options = options;
    }

    /// <inheritdoc />
    public async Task<EmailResult> SendAsync(EmailMessage message, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        var msg = BuildSendGridMessage(message);

        try
        {
            var response = await _client.SendEmailAsync(msg, ct).ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
            {
                var messageId = response.Headers.TryGetValues("X-Message-Id", out var ids)
                    ? ids.FirstOrDefault()
                    : null;
                return new EmailResult(true, messageId);
            }

            var body = await response.Body.ReadAsStringAsync(ct).ConfigureAwait(false);
            return new EmailResult(false, null, $"SendGrid error {(int)response.StatusCode}: {body}");
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

        string? lastMessageId = null;
        foreach (var message in messages)
        {
            var result = await SendAsync(message, ct).ConfigureAwait(false);
            if (!result.Success)
                return result;
            lastMessageId = result.MessageId;
        }

        return new EmailResult(true, lastMessageId);
    }

    private SendGridMessage BuildSendGridMessage(EmailMessage message)
    {
        var from = message.From is not null
            ? new EmailAddress(message.From)
            : new EmailAddress(_options.DefaultFrom, _options.DefaultFromName);

        var msg = new SendGridMessage
        {
            From = from,
            Subject = message.Subject,
        };

        msg.AddTo(new EmailAddress(message.To));

        if (message.Cc is not null)
        {
            foreach (var cc in message.Cc)
                msg.AddCc(new EmailAddress(cc));
        }

        if (message.Bcc is not null)
        {
            foreach (var bcc in message.Bcc)
                msg.AddBcc(new EmailAddress(bcc));
        }

        if (message.IsHtml)
            msg.HtmlContent = message.Body;
        else
            msg.PlainTextContent = message.Body;

        if (message.Attachments is not null)
        {
            foreach (var att in message.Attachments)
            {
                msg.AddAttachment(
                    att.FileName,
                    Convert.ToBase64String(att.Content),
                    att.ContentType);
            }
        }

        return msg;
    }
}
