using StackExchange.Redis;

namespace MarcusPrado.Platform.Redis.Lock;

/// <summary>
/// Internal lock handle that releases via a Lua CAS-DEL script to avoid
/// releasing a lock acquired by a different holder.
/// </summary>
internal sealed class RedisLockHandle : ILockHandle
{
    // Lua: DEL only when GET matches token (atomic compare-and-delete)
    private const string ReleaseLua =
        "if redis.call(\"GET\", KEYS[1]) == ARGV[1] then\n" +
        "    return redis.call(\"DEL\", KEYS[1])\n" +
        "else\n" +
        "    return 0\n" +
        "end";

    private readonly IDatabase _db;
    private int _released;

    internal RedisLockHandle(IDatabase db, string key, string token)
    {
        _db   = db;
        Key   = key;
        Token = token;
    }

    /// <inheritdoc/>
    public string Key { get; }

    /// <inheritdoc/>
    public string Token { get; }

    /// <inheritdoc/>
    public bool IsHeld => _released == 0;

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        if (System.Threading.Interlocked.Exchange(ref _released, 1) == 0)
        {
            await _db.ScriptEvaluateAsync(ReleaseLua, [(RedisKey)Key], [(RedisValue)Token]);
        }
    }
}
