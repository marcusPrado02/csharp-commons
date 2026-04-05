using StackExchange.Redis;

namespace MarcusPrado.Platform.DistributedLock;

/// <summary>
/// An <see cref="IAsyncDisposable"/> returned by <see cref="RedisDistributedLock"/> that
/// releases the Redis lock on disposal using a Lua compare-and-delete script.
/// </summary>
internal sealed class RedisReleaseLock : IAsyncDisposable
{
    // Lua: DEL only when GET matches token (atomic compare-and-delete)
    private const string ReleaseLua =
        "if redis.call(\"GET\", KEYS[1]) == ARGV[1] then\n" +
        "    return redis.call(\"DEL\", KEYS[1])\n" +
        "else\n" +
        "    return 0\n" +
        "end";

    private readonly IDatabase _db;
    private readonly string _key;
    private readonly string _token;
    private int _released;

    /// <summary>
    /// Initialises a new <see cref="RedisReleaseLock"/>.
    /// </summary>
    /// <param name="db">The Redis database.</param>
    /// <param name="key">The lock key.</param>
    /// <param name="token">The fencing token that was stored when the lock was acquired.</param>
    internal RedisReleaseLock(IDatabase db, string key, string token)
    {
        _db = db;
        _key = key;
        _token = token;
    }

    /// <summary>
    /// Gets the lock key.
    /// </summary>
    public string Key => _key;

    /// <summary>
    /// Gets the fencing token associated with this lock acquisition.
    /// </summary>
    public string Token => _token;

    /// <summary>
    /// Gets a value indicating whether the lock has not yet been released.
    /// </summary>
    public bool IsHeld => _released == 0;

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        if (Interlocked.Exchange(ref _released, 1) == 0)
        {
            await _db.ScriptEvaluateAsync(
                ReleaseLua,
                [(RedisKey)_key],
                [(RedisValue)_token]).ConfigureAwait(false);
        }
    }
}
