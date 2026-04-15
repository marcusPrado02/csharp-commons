namespace MarcusPrado.Platform.Abstractions.Sms;

/// <summary>Abstraction for sending SMS messages via an underlying provider.</summary>
public interface ISmsService
{
    /// <summary>Sends an SMS message and returns the delivery result.</summary>
    /// <param name="message">The SMS message to send.</param>
    /// <param name="ct">A token to cancel the asynchronous operation.</param>
    /// <returns>A <see cref="SmsResult"/> indicating success or failure of the send operation.</returns>
    Task<SmsResult> SendAsync(SmsMessage message, CancellationToken ct = default);
}
