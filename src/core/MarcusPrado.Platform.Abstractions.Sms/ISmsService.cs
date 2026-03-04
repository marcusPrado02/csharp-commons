namespace MarcusPrado.Platform.Abstractions.Sms;

public interface ISmsService
{
    Task<SmsResult> SendAsync(SmsMessage message, CancellationToken ct = default);
}
