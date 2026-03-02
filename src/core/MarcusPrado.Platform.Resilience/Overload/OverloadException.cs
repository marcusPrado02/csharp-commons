namespace MarcusPrado.Platform.Resilience.Overload;

/// <summary>
/// Thrown by <see cref="AdaptiveConcurrencyLimiter"/> when the adaptive limit
/// is exceeded and the request should be rejected upstream.
/// </summary>
public sealed class OverloadException : Exception
{
    /// <inheritdoc />
    public OverloadException(string message) : base(message) { }

    /// <inheritdoc />
    public OverloadException(string message, Exception inner) : base(message, inner) { }
}
