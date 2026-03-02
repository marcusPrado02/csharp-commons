namespace MarcusPrado.Platform.Application.Idempotency;

/// <summary>
/// Marks a command as idempotent.
/// The <see cref="MarcusPrado.Platform.Application.Pipeline.IdempotencyBehavior{TRequest,TResponse}"/>
/// will cache the response in <see cref="IIdempotencyStore"/> for <see cref="TimeToLiveSeconds"/>.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class IdempotentAttribute : Attribute
{
    /// <summary>How long the cached response survives (seconds). Default: 1 hour.</summary>
    public int TimeToLiveSeconds { get; }

    /// <summary>Creates an <see cref="IdempotentAttribute"/> with the given TTL.</summary>
    /// <param name="timeToLiveSeconds">Cache duration in seconds (default 3600).</param>
    public IdempotentAttribute(int timeToLiveSeconds = 3600)
    {
        TimeToLiveSeconds = timeToLiveSeconds;
    }
}
