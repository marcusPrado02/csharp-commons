using System.Text.Json;
using StackExchange.Redis;

namespace MarcusPrado.Platform.Redis.Caching;

/// <summary>
/// Redis-backed implementation of <see cref="ICache"/> using
/// <see cref="StackExchange.Redis"/>.
/// </summary>
public sealed class RedisCache : ICache
{
    private readonly IDatabase _db;
    private readonly RedisCacheOptions _options;

    /// <summary>Initialises the cache with the given Redis database connection.</summary>
    public RedisCache(IConnectionMultiplexer multiplexer, RedisCacheOptions options)
    {
        ArgumentNullException.ThrowIfNull(multiplexer);
        ArgumentNullException.ThrowIfNull(options);
        _db = multiplexer.GetDatabase();
        _options = options;
    }

    /// <inheritdoc/>
    public async Task<T?> GetAsync<T>(string key, CancellationToken ct = default)
        where T : class
    {
        var redisKey = BuildKey(key);
        var value = await _db.StringGetAsync(redisKey);
        if (!value.HasValue)
        {
            return null;
        }

        return JsonSerializer.Deserialize<T>((string)value!);
    }

    /// <inheritdoc/>
    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken ct = default)
        where T : class
    {
        var redisKey = BuildKey(key);
        var serialized = JsonSerializer.Serialize(value);
        await _db.StringSetAsync(redisKey, serialized, expiry ?? _options.DefaultExpiry);
    }

    /// <inheritdoc/>
    public async Task RemoveAsync(string key, CancellationToken ct = default)
    {
        await _db.KeyDeleteAsync(BuildKey(key));
    }

    /// <inheritdoc/>
    public async Task<bool> ExistsAsync(string key, CancellationToken ct = default)
    {
        return await _db.KeyExistsAsync(BuildKey(key));
    }

    private string BuildKey(string key) =>
        string.IsNullOrEmpty(_options.KeyPrefix) ? key : $"{_options.KeyPrefix}{key}";
}
