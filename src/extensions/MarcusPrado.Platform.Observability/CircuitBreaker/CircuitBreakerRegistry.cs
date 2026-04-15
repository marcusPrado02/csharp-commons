using System.Collections.Concurrent;

namespace MarcusPrado.Platform.Observability.CircuitBreaker;

/// <summary>
/// Thread-safe registry that tracks the state and failure counts of named circuit breakers.
/// </summary>
public sealed class CircuitBreakerRegistry
{
    /// <summary>Default number of consecutive failures before a circuit is automatically opened.</summary>
    public const int DefaultFailureThreshold = 5;

    private readonly ConcurrentDictionary<string, CircuitBreakerEntry> _entries = new(StringComparer.Ordinal);
    private readonly int _failureThreshold;

    /// <summary>
    /// Initialises a new <see cref="CircuitBreakerRegistry"/> with an optional failure threshold.
    /// </summary>
    /// <param name="failureThreshold">
    /// Number of consecutive failures after which a circuit is automatically moved to
    /// <see cref="CircuitBreakerState.Open"/>.  Defaults to <see cref="DefaultFailureThreshold"/>.
    /// </param>
    public CircuitBreakerRegistry(int failureThreshold = DefaultFailureThreshold)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(failureThreshold, 1);
        _failureThreshold = failureThreshold;
    }

    /// <summary>Registers a new circuit breaker in <see cref="CircuitBreakerState.Closed"/> state.</summary>
    /// <param name="name">A unique, non-empty name for the circuit breaker.</param>
    public void Register(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        _entries.TryAdd(name, new CircuitBreakerEntry(name, CircuitBreakerState.Closed, 0, null));
    }

    /// <summary>
    /// Increments the failure count for the named circuit breaker.
    /// If the count reaches the threshold, the state transitions to <see cref="CircuitBreakerState.Open"/>.
    /// </summary>
    /// <param name="name">The name of the circuit breaker to update.</param>
    public void RecordFailure(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        _entries.AddOrUpdate(
            name,
            addValueFactory: static (n) => new CircuitBreakerEntry(n, CircuitBreakerState.Closed, 1, null),
            updateValueFactory: (_, existing) =>
            {
                var newFailures = existing.FailuresTotal + 1;
                var newState = newFailures >= _failureThreshold ? CircuitBreakerState.Open : existing.State;
                var stateChanged = newState != existing.State;
                return existing with
                {
                    FailuresTotal = newFailures,
                    State = newState,
                    LastStateChange = stateChanged ? DateTimeOffset.UtcNow : existing.LastStateChange,
                };
            }
        );
    }

    /// <summary>
    /// Records a successful operation, resetting the failure count to zero.
    /// The state is not automatically changed by this method.
    /// </summary>
    /// <param name="name">The name of the circuit breaker to update.</param>
    public void RecordSuccess(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        _entries.AddOrUpdate(
            name,
            addValueFactory: static (n) => new CircuitBreakerEntry(n, CircuitBreakerState.Closed, 0, null),
            updateValueFactory: static (_, existing) => existing with { FailuresTotal = 0 }
        );
    }

    /// <summary>Manually overrides the state of the named circuit breaker.</summary>
    /// <param name="name">The name of the circuit breaker.</param>
    /// <param name="state">The desired new state.</param>
    public void SetState(string name, CircuitBreakerState state)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        _entries.AddOrUpdate(
            name,
            addValueFactory: (n) => new CircuitBreakerEntry(n, state, 0, DateTimeOffset.UtcNow),
            updateValueFactory: (_, existing) =>
            {
                var stateChanged = existing.State != state;
                return existing with
                {
                    State = state,
                    LastStateChange = stateChanged ? DateTimeOffset.UtcNow : existing.LastStateChange,
                };
            }
        );
    }

    /// <summary>Returns a snapshot of all registered circuit breakers.</summary>
    /// <returns>An enumerable of <see cref="CircuitBreakerEntry"/> values.</returns>
    public IEnumerable<CircuitBreakerEntry> GetAll() => _entries.Values;

    /// <summary>Resets the named circuit breaker to <see cref="CircuitBreakerState.Closed"/> with zero failures.</summary>
    /// <param name="name">The name of the circuit breaker to reset.</param>
    public void Reset(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        _entries.AddOrUpdate(
            name,
            addValueFactory: static (n) =>
                new CircuitBreakerEntry(n, CircuitBreakerState.Closed, 0, DateTimeOffset.UtcNow),
            updateValueFactory: static (_, existing) =>
                existing with
                {
                    State = CircuitBreakerState.Closed,
                    FailuresTotal = 0,
                    LastStateChange = DateTimeOffset.UtcNow,
                }
        );
    }
}
