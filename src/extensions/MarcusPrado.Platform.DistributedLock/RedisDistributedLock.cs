using StackExchange.Redis;

namespace MarcusPrado.Platform.DistributedLock;

/// <summary>
/// Redis-backed distributed lock using the Redlock-inspired algorithm.
/// Uses atomic <c>SET key token NX PX expiry</c> with a monotonic fencing token
/// to prevent stale releases.
/// </summary>
public sealed class RedisDistributedLock : IDistributedLock
{
    /// <summary>
    /// Monotonic counter used to generate fencing tokens. Thread-safe via <see cref="Interlocked"/>.
    /// </summary>
    private static long _fencingCounter;

    private readonly IDatabase _db;

    /// <summary>
    /// Initialises a new instance using the provided <see cref="IDatabase"/>.
    /// </summary>
    /// <param name="database">The Redis database to use for lock operations.</param>
    public RedisDistributedLock(IDatabase database)
    {
        ArgumentNullException.ThrowIfNull(database);
        _db = database;
    }

    /// <inheritdoc/>
    public async Task<IAsyncDisposable?> AcquireAsync(
        string key,
        TimeSpan expiry,
        CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        ct.ThrowIfCancellationRequested();

        var token = Interlocked.Increment(ref _fencingCounter).ToString();
        var acquired = await _db.StringSetAsync(key, token, expiry, keepTtl: false, When.NotExists).ConfigureAwait(false);

        return acquired ? new RedisReleaseLock(_db, key, token) : null;
    }
}
