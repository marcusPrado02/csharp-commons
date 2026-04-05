namespace MarcusPrado.Platform.Security.Signatures;

public sealed class WebhookSignatureOptions
{
    public byte[] Secret { get; set; } = [];

    public string HeaderName { get; set; } = "X-Webhook-Signature";

    /// <summary>Tolerance window for timestamp validation. Default: 5 minutes.</summary>
    public TimeSpan TimestampWindow { get; set; } = TimeSpan.FromMinutes(5);

    /// <summary>Header carrying the request timestamp (Unix seconds). Default: X-Webhook-Timestamp.</summary>
    public string TimestampHeader { get; set; } = "X-Webhook-Timestamp";
}
