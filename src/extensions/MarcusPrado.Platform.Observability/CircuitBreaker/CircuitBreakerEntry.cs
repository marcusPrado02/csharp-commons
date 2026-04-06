namespace MarcusPrado.Platform.Observability.CircuitBreaker;

/// <summary>Represents a snapshot of a single circuit breaker's status.</summary>
/// <param name="Name">The unique name identifying the circuit breaker.</param>
/// <param name="State">The current state of the circuit breaker.</param>
/// <param name="FailuresTotal">The total number of failures recorded since the last reset.</param>
/// <param name="LastStateChange">The timestamp of the most recent state transition, or <c>null</c> if never changed.</param>
public sealed record CircuitBreakerEntry(
    string Name,
    CircuitBreakerState State,
    int FailuresTotal,
    DateTimeOffset? LastStateChange);
