namespace MarcusPrado.Platform.Abstractions.Sms;

public sealed record SmsMessage(
    string To, string Body, string? From = null);

public sealed record SmsResult(
    bool   Success,
    string? MessageId = null,
    string? Error     = null);
