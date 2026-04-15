namespace MarcusPrado.Platform.Abstractions.Sms;

/// <summary>Represents an SMS message to be sent to a recipient.</summary>
/// <param name="To">The destination phone number in E.164 format.</param>
/// <param name="Body">The text content of the SMS message.</param>
/// <param name="From">Optional sender phone number or alphanumeric sender ID.</param>
public sealed record SmsMessage(
    string To, string Body, string? From = null);

/// <summary>Represents the outcome of an SMS send operation.</summary>
/// <param name="Success">Indicates whether the SMS was accepted by the provider.</param>
/// <param name="MessageId">Optional provider-assigned identifier for the sent message.</param>
/// <param name="Error">Optional error description when <paramref name="Success"/> is <see langword="false"/>.</param>
public sealed record SmsResult(
    bool   Success,
    string? MessageId = null,
    string? Error     = null);
