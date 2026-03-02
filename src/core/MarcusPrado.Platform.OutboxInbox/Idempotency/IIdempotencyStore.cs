namespace MarcusPrado.Platform.OutboxInbox.Idempotency;

/// <summary>Persistence contract for idempotency records.</summary>
public interface IIdempotencyStore
{
    /// <summary>Returns an existing record for <paramref name="key"/>, or null if none exists.</summary>
    Task<IdempotencyRecord?> GetAsync(IdempotencyKey key, CancellationToken ct = default);

    /// <summary>Stores <paramref name="record"/> so the same operation is not repeated.</summary>
    Task SetAsync(IdempotencyRecord record, CancellationToken ct = default);

    /// <summary>Returns true if a valid (non-expired) record exists for <paramref name="key"/>.</summary>
    Task<bool> ExistsAsync(IdempotencyKey key, CancellationToken ct = default);

    /// <summary>Removes the record for <paramref name="key"/>, if present.</summary>
    Task RemoveAsync(IdempotencyKey key, CancellationToken ct = default);
}
