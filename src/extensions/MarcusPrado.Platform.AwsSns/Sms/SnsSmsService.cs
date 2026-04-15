using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using MarcusPrado.Platform.Abstractions.Sms;
using MarcusPrado.Platform.AwsSns.Options;

namespace MarcusPrado.Platform.AwsSns.Sms;

/// <summary>Implements <see cref="ISmsService"/> via AWS Simple Notification Service.</summary>
public sealed class SnsSmsService : ISmsService
{
    private readonly IAmazonSimpleNotificationService _sns;
    private readonly AwsSnsOptions _options;

    /// <summary>Initializes a new instance of <see cref="SnsSmsService"/>.</summary>
    public SnsSmsService(IAmazonSimpleNotificationService sns, AwsSnsOptions options)
    {
        ArgumentNullException.ThrowIfNull(sns);
        ArgumentNullException.ThrowIfNull(options);
        _sns = sns;
        _options = options;
    }

    /// <inheritdoc />
    public async Task<SmsResult> SendAsync(SmsMessage message, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        var request = new PublishRequest
        {
            PhoneNumber = message.To,
            Message = message.Body,
            MessageAttributes = BuildMessageAttributes(message.From),
        };

        try
        {
            var response = await _sns.PublishAsync(request, ct).ConfigureAwait(false);
            return new SmsResult(true, response.MessageId);
        }
#pragma warning disable CA1031
        catch (Exception ex)
#pragma warning restore CA1031
        {
            return new SmsResult(false, null, ex.Message);
        }
    }

    private Dictionary<string, MessageAttributeValue> BuildMessageAttributes(string? from)
    {
        var attrs = new Dictionary<string, MessageAttributeValue>
        {
            ["AWS.SNS.SMS.SMSType"] = new MessageAttributeValue
            {
                DataType = "String",
                StringValue = _options.SmsType,
            },
        };

        var senderId = from ?? _options.SenderId;
        if (!string.IsNullOrWhiteSpace(senderId))
        {
            attrs["AWS.SNS.SMS.SenderID"] = new MessageAttributeValue
            {
                DataType = "String",
                StringValue = senderId,
            };
        }

        return attrs;
    }
}
