using System.Text.Json;
using MarcusPrado.Platform.OutboxInbox.Idempotency;
using StackExchange.Redis;

namespace MarcusPrado.Platform.Redis.Idempotency;

/// <summary>
/// Redis-backed <see cref="IIdempotencyStore"/> with TTL-aware storage.
/// Key format: <c>{tenantId}:{operationName}:{idempotencyKey}</c>.
/// </summary>
public sealed class RedisIdempotencyStore : IIdempotencyStore
{
    private readonly IDatabase _db;
    private readonly TimeSpan _defaultTtl;

    /// <summary>Initialises the store with the given multiplexer and default TTL.</summary>
    public RedisIdempotencyStore(
        IConnectionMultiplexer multiplexer,
        TimeSpan? defaultTtl = null)
    {
        ArgumentNullException.ThrowIfNull(multiplexer);
        _db = multiplexer.GetDatabase();
        _defaultTtl = defaultTtl ?? TimeSpan.FromDays(1);
    }

    /// <inheritdoc/>
    public async Task<IdempotencyRecord?> GetAsync(
        IdempotencyKey key,
        CancellationToken ct = default)
    {
        var value = await _db.StringGetAsync(RedisKey(key));
        if (!value.HasValue)
        {
            return null;
        }

        var record = JsonSerializer.Deserialize<IdempotencyRecord>((string)value!);
        if (record is { } r && !r.IsValid(DateTimeOffset.UtcNow))
        {
            return null;
        }

        return record;
    }

    /// <inheritdoc/>
    public async Task SetAsync(IdempotencyRecord record, CancellationToken ct = default)
    {
        var ttl = record.ExpiresAt.HasValue
            ? record.ExpiresAt.Value - DateTimeOffset.UtcNow
            : _defaultTtl;

        if (ttl <= TimeSpan.Zero)
        {
            return;
        }

        var json = JsonSerializer.Serialize(record);
        await _db.StringSetAsync(record.Key, json, ttl);
    }

    /// <inheritdoc/>
    public async Task<bool> ExistsAsync(IdempotencyKey key, CancellationToken ct = default)
    {
        return await _db.KeyExistsAsync(RedisKey(key));
    }

    /// <inheritdoc/>
    public async Task RemoveAsync(IdempotencyKey key, CancellationToken ct = default)
    {
        await _db.KeyDeleteAsync(RedisKey(key));
    }

    private static string RedisKey(IdempotencyKey key) => key.Value;
}
