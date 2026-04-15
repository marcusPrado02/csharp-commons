using MarcusPrado.Platform.Abstractions.Sms;
using MarcusPrado.Platform.Twilio.Options;
using Twilio;
using Twilio.Clients;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace MarcusPrado.Platform.Twilio.Sms;

/// <summary>Sends SMS messages via the Twilio REST API.</summary>
public sealed class TwilioSmsService : ISmsService
{
    private readonly TwilioOptions _options;
    private readonly ITwilioRestClient? _client;

    /// <summary>
    /// Initializes a new instance of <see cref="TwilioSmsService"/>.
    /// When <paramref name="client"/> is <see langword="null"/> the global
    /// <see cref="TwilioClient"/> is initialised with the account credentials.
    /// </summary>
    public TwilioSmsService(TwilioOptions options, ITwilioRestClient? client = null)
    {
        ArgumentNullException.ThrowIfNull(options);
        _options = options;
        _client = client;

        if (client is null)
            TwilioClient.Init(options.AccountSid, options.AuthToken);
    }

    /// <inheritdoc />
    public async Task<SmsResult> SendAsync(SmsMessage message, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        try
        {
            var from = new PhoneNumber(message.From ?? _options.DefaultFrom);
            var to = new PhoneNumber(message.To);

            MessageResource resource = _client is not null
                ? await MessageResource
                    .CreateAsync(to, from: from, body: message.Body, client: _client)
                    .ConfigureAwait(false)
                : await MessageResource.CreateAsync(to, from: from, body: message.Body).ConfigureAwait(false);

            var success = resource.ErrorCode is null;
            return new SmsResult(
                success,
                resource.Sid,
                success ? null : $"[{resource.ErrorCode}] {resource.ErrorMessage}"
            );
        }
#pragma warning disable CA1031
        catch (Exception ex)
#pragma warning restore CA1031
        {
            return new SmsResult(false, null, ex.Message);
        }
    }
}
