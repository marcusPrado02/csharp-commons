using MarcusPrado.Platform.RateLimiting.Quotas;
using StackExchange.Redis;

namespace MarcusPrado.Platform.Redis.RateLimit;

/// <summary>
/// Redis-backed <see cref="IQuotaStore"/> using atomic INCR + EXPIRE.
/// Uses a Lua script to atomically increment + set TTL on first use.
/// </summary>
public sealed class RedisQuotaStore : IQuotaStore
{
    private static readonly string LuaIncrExpire = @"
local current = redis.call('INCR', KEYS[1])
if current == 1 then
  redis.call('EXPIRE', KEYS[1], ARGV[1])
end
return current";

    private static readonly string LuaTryConsume = @"
local current = redis.call('GET', KEYS[1])
local count = tonumber(current) or 0
if count >= tonumber(ARGV[2]) then
  return 0
end
local newval = redis.call('INCR', KEYS[1])
if newval == 1 then
  redis.call('EXPIRE', KEYS[1], ARGV[1])
end
return 1";

    private readonly IDatabase _db;

    /// <summary>Initialises the store with the given Redis database connection.</summary>
    public RedisQuotaStore(IConnectionMultiplexer multiplexer)
    {
        ArgumentNullException.ThrowIfNull(multiplexer);
        _db = multiplexer.GetDatabase();
    }

    /// <inheritdoc/>
    public async Task<long> IncrementAsync(
        string key,
        long windowSeconds,
        CancellationToken ct = default)
    {
        var result = await _db.ScriptEvaluateAsync(
            LuaIncrExpire,
            keys: new RedisKey[] { key },
            values: new RedisValue[] { windowSeconds });
        return (long)result;
    }

    /// <inheritdoc/>
    public async Task<bool> TryConsumeAsync(
        string key,
        long limit,
        long windowSeconds,
        CancellationToken ct = default)
    {
        var result = await _db.ScriptEvaluateAsync(
            LuaTryConsume,
            keys: new RedisKey[] { key },
            values: new RedisValue[] { windowSeconds, limit });
        return (long)result == 1;
    }

    /// <inheritdoc/>
    public async Task ResetAsync(string key, CancellationToken ct = default)
    {
        await _db.KeyDeleteAsync(key);
    }
}
