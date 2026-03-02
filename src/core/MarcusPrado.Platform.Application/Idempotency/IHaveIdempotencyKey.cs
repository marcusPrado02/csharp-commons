namespace MarcusPrado.Platform.Application.Idempotency;

/// <summary>
/// Commands that provide their own idempotency key implement this interface.
/// When absent, the behavior falls back to a hash of the serialized command.
/// </summary>
public interface IHaveIdempotencyKey
{
    /// <summary>A stable, client-supplied key that uniquely identifies this request.</summary>
    string IdempotencyKey { get; }
}
