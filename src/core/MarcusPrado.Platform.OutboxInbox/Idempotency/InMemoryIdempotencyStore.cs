using System.Collections.Concurrent;

namespace MarcusPrado.Platform.OutboxInbox.Idempotency;

/// <summary>Thread-safe in-memory implementation of <see cref="IIdempotencyStore"/>.</summary>
public sealed class InMemoryIdempotencyStore : IIdempotencyStore
{
    private readonly ConcurrentDictionary<string, IdempotencyRecord> _store = new();

    /// <inheritdoc/>
    public Task<IdempotencyRecord?> GetAsync(IdempotencyKey key, CancellationToken ct = default)
    {
        _store.TryGetValue(key.Value, out var record);
        if (record is not null && !record.IsValid(DateTimeOffset.UtcNow))
        {
            _store.TryRemove(key.Value, out _);
            return Task.FromResult<IdempotencyRecord?>(null);
        }

        return Task.FromResult(record);
    }

    /// <inheritdoc/>
    public Task SetAsync(IdempotencyRecord record, CancellationToken ct = default)
    {
        _store[record.Key] = record;
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public async Task<bool> ExistsAsync(IdempotencyKey key, CancellationToken ct = default)
    {
        var record = await GetAsync(key, ct).ConfigureAwait(false);
        return record is not null;
    }

    /// <inheritdoc/>
    public Task RemoveAsync(IdempotencyKey key, CancellationToken ct = default)
    {
        _store.TryRemove(key.Value, out _);
        return Task.CompletedTask;
    }
}
