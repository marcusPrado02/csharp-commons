using System.Threading;

namespace MarcusPrado.Platform.Resilience.Overload;

/// <summary>
/// AIMD (Additive Increase / Multiplicative Decrease) adaptive concurrency limiter.
/// Increases the limit by 1 on success; halves it on failure/overload.
/// </summary>
public sealed class AdaptiveConcurrencyLimiter
{
    private readonly int _minLimit;
    private readonly int _maxLimit;
    private readonly BackpressureSignal _backpressure;
    private volatile int _limit;
    private volatile int _inflight;

    /// <summary>Creates the limiter with the specified bounds.</summary>
    /// <param name="initialLimit">Starting concurrency limit.</param>
    /// <param name="minLimit">Minimum limit (never decreased below this).</param>
    /// <param name="maxLimit">Maximum limit (never increased above this).</param>
    /// <param name="backpressure">Optional back-pressure signal to set on overload.</param>
    public AdaptiveConcurrencyLimiter(
        int initialLimit = 100,
        int minLimit = 1,
        int maxLimit = 1000,
        BackpressureSignal? backpressure = null
    )
    {
        _limit = initialLimit;
        _minLimit = minLimit;
        _maxLimit = maxLimit;
        _backpressure = backpressure ?? new BackpressureSignal();
    }

    /// <summary>Gets the current concurrency limit.</summary>
    public int CurrentLimit => _limit;

    /// <summary>Executes <paramref name="action"/> respecting the adaptive limit.</summary>
    /// <exception cref="OverloadException">Thrown when the limit is exceeded.</exception>
    public async Task<T> ExecuteAsync<T>(
        Func<CancellationToken, Task<T>> action,
        CancellationToken cancellationToken = default
    )
    {
        if (Interlocked.Increment(ref _inflight) > _limit)
        {
            Interlocked.Decrement(ref _inflight);
            _backpressure.Activate();
            throw new OverloadException($"Adaptive concurrency limit ({_limit}) exceeded.");
        }

        try
        {
            var result = await action(cancellationToken).ConfigureAwait(false);
            // Additive increase on success
            Interlocked.Exchange(ref _limit, Math.Min(_limit + 1, _maxLimit));
            _backpressure.Clear();
            return result;
        }
        catch
        {
            // Multiplicative decrease on failure
            Interlocked.Exchange(ref _limit, Math.Max(_limit / 2, _minLimit));
            throw;
        }
        finally
        {
            Interlocked.Decrement(ref _inflight);
        }
    }
}
