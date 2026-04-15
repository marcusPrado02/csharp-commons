using StackExchange.Redis;

namespace MarcusPrado.Platform.Redis.Lock;

/// <summary>
/// Redis-backed distributed lock using atomic <c>SET NX PX</c> with a
/// per-acquisition fencing token.
/// </summary>
public sealed class RedisDistributedLock : IDistributedLock
{
    private readonly IDatabase _db;

    /// <summary>Initialises using the multiplexer's default database.</summary>
    public RedisDistributedLock(IConnectionMultiplexer multiplexer)
    {
        ArgumentNullException.ThrowIfNull(multiplexer);
        _db = multiplexer.GetDatabase();
    }

    /// <inheritdoc/>
    public async Task<ILockHandle> AcquireAsync(
        string key,
        TimeSpan ttl,
        int retryCount = 3,
        TimeSpan? retryDelay = null,
        CancellationToken ct = default)
    {
        var delay = retryDelay ?? TimeSpan.FromMilliseconds(100);

        for (var attempt = 0; attempt <= retryCount; attempt++)
        {
            var handle = await TryAcquireAsync(key, ttl, ct);
            if (handle is not null)
            {
                return handle;
            }

            if (attempt < retryCount)
            {
                await Task.Delay(delay, ct);
            }
        }

        throw new InvalidOperationException(
            $"Unable to acquire distributed lock for key '{key}' after {retryCount + 1} attempt(s).");
    }

    /// <inheritdoc/>
    public async Task<ILockHandle?> TryAcquireAsync(
        string key,
        TimeSpan ttl,
        CancellationToken ct = default)
    {
        var token = Guid.NewGuid().ToString("N");
        var acquired = await _db.StringSetAsync(key, token, ttl, When.NotExists);
        return acquired ? new RedisLockHandle(_db, key, token) : null;
    }
}
