namespace MarcusPrado.Platform.Security.Signatures;

public sealed record SignedPayloadEnvelope<T>(T Payload, string Signature, string Nonce, DateTimeOffset Timestamp)
{
    /// <summary>Returns true if the envelope is within the allowed replay window.</summary>
    public bool IsWithinWindow(TimeSpan window) => DateTimeOffset.UtcNow - Timestamp <= window;
}
