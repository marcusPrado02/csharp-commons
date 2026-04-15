using System.Threading;

namespace MarcusPrado.Platform.Resilience.Policies;

/// <summary>
/// Fixed-window rate limiter.  Allows at most <c>limit</c> calls per <c>window</c>.
/// </summary>
public sealed class RateLimitPolicy
{
    private readonly int _limit;
    private readonly TimeSpan _window;
    private readonly object _sync = new();
    private int _count;
    private DateTime _windowStart = DateTime.UtcNow;

    /// <summary>Creates the policy with the given limit and window.</summary>
    public RateLimitPolicy(int limit, TimeSpan window)
    {
        _limit = limit;
        _window = window;
    }

    /// <summary>Executes <paramref name="action"/> respecting the rate limit.</summary>
    /// <exception cref="RateLimitExceededException">Thrown when the limit is exceeded.</exception>
    public async Task<T> ExecuteAsync<T>(
        Func<CancellationToken, Task<T>> action,
        CancellationToken cancellationToken = default
    )
    {
        lock (_sync)
        {
            var now = DateTime.UtcNow;
            if (now - _windowStart >= _window)
            {
                _count = 0;
                _windowStart = now;
            }

            if (_count >= _limit)
            {
                throw new RateLimitExceededException($"Rate limit ({_limit} calls per {_window}) exceeded.");
            }

            _count++;
        }

        return await action(cancellationToken).ConfigureAwait(false);
    }
}

/// <summary>Thrown when the rate limit is exceeded.</summary>
public sealed class RateLimitExceededException : Exception
{
    /// <inheritdoc />
    public RateLimitExceededException(string message)
        : base(message) { }
}
