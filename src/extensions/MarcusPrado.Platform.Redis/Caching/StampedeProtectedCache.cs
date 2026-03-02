using MarcusPrado.Platform.Redis.Lock;

namespace MarcusPrado.Platform.Redis.Caching;

/// <summary>
/// Decorator over <see cref="ICache"/> that prevents cache-stampede by
/// serialising concurrent misses for the same key behind a distributed lock.
/// Use <see cref="GetOrAddAsync{T}"/> for stampede-safe population.
/// </summary>
public sealed class StampedeProtectedCache : ICache
{
    private readonly ICache _inner;
    private readonly IDistributedLock _lock;
    private readonly TimeSpan _lockTtl;

    /// <summary>
    /// Initialises with the underlying cache and a distributed lock provider.
    /// </summary>
    /// <param name="inner">Underlying cache.</param>
    /// <param name="distributedLock">Lock provider.</param>
    /// <param name="lockTtl">TTL for stampede locks (default: 5 s).</param>
    public StampedeProtectedCache(
        ICache inner,
        IDistributedLock distributedLock,
        TimeSpan? lockTtl = null)
    {
        ArgumentNullException.ThrowIfNull(inner);
        ArgumentNullException.ThrowIfNull(distributedLock);
        _inner   = inner;
        _lock    = distributedLock;
        _lockTtl = lockTtl ?? TimeSpan.FromSeconds(5);
    }

    /// <inheritdoc/>
    public Task<T?> GetAsync<T>(string key, CancellationToken ct = default)
        where T : class
        => _inner.GetAsync<T>(key, ct);

    /// <inheritdoc/>
    public Task SetAsync<T>(
        string key,
        T value,
        TimeSpan? expiry = null,
        CancellationToken ct = default)
        where T : class
        => _inner.SetAsync(key, value, expiry, ct);

    /// <inheritdoc/>
    public Task RemoveAsync(string key, CancellationToken ct = default)
        => _inner.RemoveAsync(key, ct);

    /// <inheritdoc/>
    public Task<bool> ExistsAsync(string key, CancellationToken ct = default)
        => _inner.ExistsAsync(key, ct);

    /// <summary>
    /// Returns the cached value for <paramref name="key"/>, or computes it via
    /// <paramref name="factory"/> under a distributed lock to prevent stampede.
    /// The result is stored in the cache when non-null.
    /// </summary>
    public async Task<T?> GetOrAddAsync<T>(
        string key,
        Func<CancellationToken, Task<T?>> factory,
        TimeSpan? expiry = null,
        CancellationToken ct = default)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(factory);

        // Fast path — cache hit avoids the lock entirely
        var cached = await _inner.GetAsync<T>(key, ct);
        if (cached is not null)
        {
            return cached;
        }

        // Slow path — serialise callers behind a per-key lock
        await using var handle = await _lock.AcquireAsync(
            $"__stampede:{key}",
            _lockTtl,
            ct: ct);

        // Double-check after acquiring the lock
        var afterLock = await _inner.GetAsync<T>(key, ct);
        if (afterLock is not null)
        {
            return afterLock;
        }

        var value = await factory(ct);
        if (value is not null)
        {
            await _inner.SetAsync(key, value, expiry, ct);
        }

        return value;
    }
}
