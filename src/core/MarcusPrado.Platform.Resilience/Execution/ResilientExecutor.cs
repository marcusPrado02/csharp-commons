using MarcusPrado.Platform.Resilience.Policies;

namespace MarcusPrado.Platform.Resilience.Execution;

/// <summary>
/// Fluent builder that composes resilience policies and executes an action
/// through them in the recommended order:
/// <c>Timeout → Retry → CircuitBreaker → Bulkhead → Execute</c>.
/// </summary>
public sealed class ResilientExecutor : IDisposable
{
    private RetryPolicy? _retry;
    private CircuitBreakerPolicy? _circuitBreaker;
    private BulkheadPolicy? _bulkhead;
    private TimeSpan? _timeout;

    /// <summary>Adds a retry policy.</summary>
    public ResilientExecutor WithRetry(RetryOptions options)
    {
        _retry = new RetryPolicy(options);
        return this;
    }

    /// <summary>Adds a circuit breaker.</summary>
    public ResilientExecutor WithCircuitBreaker(CircuitBreakerOptions options)
    {
        _circuitBreaker = new CircuitBreakerPolicy(options);
        return this;
    }

    /// <summary>Adds a bulkhead with the given max parallelism.</summary>
    public ResilientExecutor WithBulkhead(int maxParallelism)
    {
        _bulkhead = new BulkheadPolicy(maxParallelism);
        return this;
    }

    /// <summary>Adds a per-call timeout.</summary>
    public ResilientExecutor WithTimeout(TimeSpan timeout)
    {
        _timeout = timeout;
        return this;
    }

    /// <summary>Executes <paramref name="action"/> through the composed pipeline.</summary>
    public async Task<T> ExecuteAsync<T>(
        Func<CancellationToken, Task<T>> action,
        CancellationToken cancellationToken = default
    )
    {
        Func<CancellationToken, Task<T>> pipeline = action;

        if (_bulkhead is not null)
        {
            var bh = _bulkhead;
            var inner = pipeline;
            pipeline = ct => bh.ExecuteAsync(inner, ct);
        }

        if (_circuitBreaker is not null)
        {
            var cb = _circuitBreaker;
            var inner = pipeline;
            pipeline = ct => cb.ExecuteAsync(inner, ct);
        }

        if (_retry is not null)
        {
            var retry = _retry;
            var inner = pipeline;
            pipeline = ct => retry.ExecuteAsync(inner, ct);
        }

        if (_timeout.HasValue)
        {
            var timeoutPolicy = new TimeoutPolicy(_timeout.Value);
            return await timeoutPolicy.ExecuteAsync(pipeline, cancellationToken).ConfigureAwait(false);
        }

        return await pipeline(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public void Dispose() => _bulkhead?.Dispose();
}
