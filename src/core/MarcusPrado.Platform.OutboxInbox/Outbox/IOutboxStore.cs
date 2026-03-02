namespace MarcusPrado.Platform.OutboxInbox.Outbox;

/// <summary>Persistence contract for the outbox table.</summary>
public interface IOutboxStore
{
    /// <summary>Persists a new outbox message.</summary>
    Task SaveAsync(OutboxMessage message, CancellationToken ct = default);

    /// <summary>Returns the next batch of pending messages up to <paramref name="batchSize"/>.</summary>
    Task<IReadOnlyList<OutboxMessage>> GetPendingAsync(int batchSize, CancellationToken ct = default);

    /// <summary>Marks a message as successfully published.</summary>
    Task MarkPublishedAsync(Guid messageId, CancellationToken ct = default);

    /// <summary>Marks a message as failed and records the error.</summary>
    Task MarkFailedAsync(Guid messageId, string error, CancellationToken ct = default);

    /// <summary>Increments the retry counter and sets next scheduled time.</summary>
    Task IncrementRetryAsync(Guid messageId, DateTimeOffset nextAttempt, CancellationToken ct = default);
}
