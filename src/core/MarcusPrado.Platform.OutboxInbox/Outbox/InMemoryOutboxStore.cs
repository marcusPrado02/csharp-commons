using System.Collections.Concurrent;

namespace MarcusPrado.Platform.OutboxInbox.Outbox;

/// <summary>Thread-safe in-memory implementation of <see cref="IOutboxStore"/> for testing.</summary>
public sealed class InMemoryOutboxStore : IOutboxStore
{
    private readonly ConcurrentDictionary<Guid, OutboxMessage> _messages = new();

    /// <summary>Gets all stored messages (for test assertions).</summary>
#pragma warning disable S2365
    public IReadOnlyCollection<OutboxMessage> Messages => _messages.Values.ToList();
#pragma warning restore S2365

    /// <inheritdoc/>
    public Task SaveAsync(OutboxMessage message, CancellationToken ct = default)
    {
        _messages[message.Id] = message;
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task<IReadOnlyList<OutboxMessage>> GetPendingAsync(int batchSize, CancellationToken ct = default)
    {
        var pending = _messages
            .Values.Where(m => m.State == OutboxState.Pending && m.ScheduledAt <= DateTimeOffset.UtcNow)
            .OrderBy(m => m.ScheduledAt)
            .Take(batchSize)
            .ToList();

        return Task.FromResult<IReadOnlyList<OutboxMessage>>(pending);
    }

    /// <inheritdoc/>
    public Task MarkPublishedAsync(Guid messageId, CancellationToken ct = default)
    {
        if (_messages.TryGetValue(messageId, out var msg))
        {
            msg.State = OutboxState.Published;
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task MarkFailedAsync(Guid messageId, string error, CancellationToken ct = default)
    {
        if (_messages.TryGetValue(messageId, out var msg))
        {
            msg.State = OutboxState.Failed;
            msg.LastError = error;
            msg.LastAttemptAt = DateTimeOffset.UtcNow;
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task IncrementRetryAsync(Guid messageId, DateTimeOffset nextAttempt, CancellationToken ct = default)
    {
        if (_messages.TryGetValue(messageId, out var msg))
        {
            msg.RetryCount++;
            msg.ScheduledAt = nextAttempt;
            msg.LastAttemptAt = DateTimeOffset.UtcNow;
        }

        return Task.CompletedTask;
    }
}
