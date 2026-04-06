namespace MarcusPrado.Platform.Observability.CircuitBreaker;

/// <summary>Represents the state of a circuit breaker.</summary>
public enum CircuitBreakerState
{
    /// <summary>The circuit is closed and requests flow normally.</summary>
    Closed = 0,

    /// <summary>The circuit is open and requests are blocked.</summary>
    Open = 1,

    /// <summary>The circuit is half-open and a limited number of requests are allowed through.</summary>
    HalfOpen = 2,
}
