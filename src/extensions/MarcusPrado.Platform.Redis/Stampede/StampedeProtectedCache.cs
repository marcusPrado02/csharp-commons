using System.Collections.Concurrent;
using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;

namespace MarcusPrado.Platform.Redis.Stampede;

/// <summary>
/// Wraps <see cref="IDistributedCache"/> and uses a <see cref="SemaphoreSlim"/> per key
/// to prevent cache stampedes. When a cache miss occurs only a single caller invokes the
/// factory; all other concurrent callers wait and then read the freshly populated entry.
/// </summary>
public sealed class StampedeProtectedCache : IAsyncDisposable
{
    private readonly IDistributedCache _inner;
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _semaphores = new();

    /// <summary>Initialises the stampede-protected cache with the underlying distributed cache.</summary>
    /// <param name="inner">The underlying <see cref="IDistributedCache"/> implementation.</param>
    public StampedeProtectedCache(IDistributedCache inner)
    {
        ArgumentNullException.ThrowIfNull(inner);
        _inner = inner;
    }

    /// <summary>
    /// Returns the cached value for <paramref name="key"/>, or computes it via
    /// <paramref name="factory"/> under a per-key semaphore to prevent stampede.
    /// The result is stored in the cache when non-null.
    /// </summary>
    /// <typeparam name="T">The type of the cached value.</typeparam>
    /// <param name="key">The cache key.</param>
    /// <param name="factory">Async factory invoked on cache miss to produce the value.</param>
    /// <param name="expiry">Optional TTL for the cache entry.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The cached or freshly produced value, or <c>null</c> if the factory returned <c>null</c>.</returns>
    public async Task<T?> GetOrSetAsync<T>(
        string key,
        Func<CancellationToken, Task<T>> factory,
        TimeSpan expiry,
        CancellationToken ct = default)
        where T : class
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
        ArgumentNullException.ThrowIfNull(factory);

        // Fast path — cache hit avoids the semaphore entirely
        var bytes = await _inner.GetAsync(key, ct).ConfigureAwait(false);
        if (bytes is not null)
        {
            return JsonSerializer.Deserialize<T>(bytes);
        }

        var semaphore = _semaphores.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));

        await semaphore.WaitAsync(ct).ConfigureAwait(false);
        try
        {
            // Double-check after acquiring the semaphore
            bytes = await _inner.GetAsync(key, ct).ConfigureAwait(false);
            if (bytes is not null)
            {
                return JsonSerializer.Deserialize<T>(bytes);
            }

            var value = await factory(ct).ConfigureAwait(false);
            if (value is not null)
            {
                var entryBytes = JsonSerializer.SerializeToUtf8Bytes(value);
                var options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = expiry
                };
                await _inner.SetAsync(key, entryBytes, options, ct).ConfigureAwait(false);
            }

            return value;
        }
        finally
        {
            semaphore.Release();
        }
    }

    /// <summary>Removes the cache entry for <paramref name="key"/>.</summary>
    /// <param name="key">The cache key to remove.</param>
    /// <param name="ct">Cancellation token.</param>
    public Task RemoveAsync(string key, CancellationToken ct = default)
        => _inner.RemoveAsync(key, ct);

    /// <inheritdoc/>
    public ValueTask DisposeAsync()
    {
        foreach (var semaphore in _semaphores.Values)
        {
            semaphore.Dispose();
        }
        _semaphores.Clear();
        return ValueTask.CompletedTask;
    }
}
