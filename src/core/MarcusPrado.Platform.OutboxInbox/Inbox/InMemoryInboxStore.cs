using System.Collections.Concurrent;

namespace MarcusPrado.Platform.OutboxInbox.Inbox;

/// <summary>Thread-safe in-memory implementation of <see cref="IInboxStore"/> for testing.</summary>
public sealed class InMemoryInboxStore : IInboxStore
{
    private readonly ConcurrentDictionary<Guid, InboxMessage> _messages = new();

    /// <summary>Gets all stored messages (for test assertions).</summary>
#pragma warning disable S2365
    public IReadOnlyCollection<InboxMessage> Messages => _messages.Values.ToList();
#pragma warning restore S2365

    /// <inheritdoc/>
    public Task SaveAsync(InboxMessage message, CancellationToken ct = default)
    {
        _messages[message.Id] = message;
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task<IReadOnlyList<InboxMessage>> GetPendingAsync(int batchSize, CancellationToken ct = default)
    {
        var pending = _messages
            .Values.Where(m => m.State == InboxState.Pending)
            .OrderBy(m => m.ReceivedAt)
            .Take(batchSize)
            .ToList();

        return Task.FromResult<IReadOnlyList<InboxMessage>>(pending);
    }

    /// <inheritdoc/>
    public Task MarkProcessedAsync(Guid id, CancellationToken ct = default)
    {
        if (_messages.TryGetValue(id, out var msg))
        {
            msg.State = InboxState.Processed;
            msg.ProcessedAt = DateTimeOffset.UtcNow;
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task MarkFailedAsync(Guid id, string error, CancellationToken ct = default)
    {
        if (_messages.TryGetValue(id, out var msg))
        {
            msg.State = InboxState.Failed;
            msg.LastError = error;
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task MarkDuplicateAsync(Guid id, CancellationToken ct = default)
    {
        if (_messages.TryGetValue(id, out var msg))
        {
            msg.State = InboxState.Duplicate;
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task IncrementRetryAsync(Guid id, DateTimeOffset nextAttempt, CancellationToken ct = default)
    {
        if (_messages.TryGetValue(id, out var msg))
        {
            msg.RetryCount++;
        }

        return Task.CompletedTask;
    }
}
