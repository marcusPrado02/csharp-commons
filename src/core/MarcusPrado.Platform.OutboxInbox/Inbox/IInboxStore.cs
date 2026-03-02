namespace MarcusPrado.Platform.OutboxInbox.Inbox;

/// <summary>Persistence contract for the inbox table.</summary>
public interface IInboxStore
{
    /// <summary>Persists a newly-received inbox message.</summary>
    Task SaveAsync(InboxMessage message, CancellationToken ct = default);

    /// <summary>Returns pending messages up to <paramref name="batchSize"/>.</summary>
    Task<IReadOnlyList<InboxMessage>> GetPendingAsync(int batchSize, CancellationToken ct = default);

    /// <summary>Marks a message as successfully processed.</summary>
    Task MarkProcessedAsync(Guid id, CancellationToken ct = default);

    /// <summary>Marks a message as failed and records the error.</summary>
    Task MarkFailedAsync(Guid id, string error, CancellationToken ct = default);

    /// <summary>Marks a message as duplicate (already processed).</summary>
    Task MarkDuplicateAsync(Guid id, CancellationToken ct = default);

    /// <summary>Increments the retry counter and sets next scheduled time.</summary>
    Task IncrementRetryAsync(Guid id, DateTimeOffset nextAttempt, CancellationToken ct = default);
}
