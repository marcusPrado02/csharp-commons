using MarcusPrado.Platform.EfCore.DbContext;
using MarcusPrado.Platform.OutboxInbox.Outbox;
using Microsoft.EntityFrameworkCore;

namespace MarcusPrado.Platform.EfCore.Outbox;

/// <summary>EF Core-backed implementation of <see cref="IOutboxStore"/>.</summary>
public sealed class EfOutboxStore : IOutboxStore
{
    private readonly AppDbContextBase _context;

    /// <summary>Initialises the store with the given db context.</summary>
    public EfOutboxStore(AppDbContextBase context)
    {
        ArgumentNullException.ThrowIfNull(context);
        _context = context;
    }

    /// <inheritdoc/>
    public async Task SaveAsync(OutboxMessage message, CancellationToken ct = default)
    {
        _context.OutboxMessages.Add(message);
        await _context.SaveChangesAsync(ct);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<OutboxMessage>> GetPendingAsync(int batchSize, CancellationToken ct = default)
    {
        return await _context
            .OutboxMessages.Where(m => m.State == OutboxState.Pending && m.ScheduledAt <= DateTimeOffset.UtcNow)
            .OrderBy(m => m.ScheduledAt)
            .Take(batchSize)
            .ToListAsync(ct);
    }

    /// <inheritdoc/>
    public async Task MarkPublishedAsync(Guid messageId, CancellationToken ct = default)
    {
        await _context
            .OutboxMessages.Where(m => m.Id == messageId)
            .ExecuteUpdateAsync(s => s.SetProperty(m => m.State, OutboxState.Published), ct);
    }

    /// <inheritdoc/>
    public async Task MarkFailedAsync(Guid messageId, string error, CancellationToken ct = default)
    {
        await _context
            .OutboxMessages.Where(m => m.Id == messageId)
            .ExecuteUpdateAsync(
                s => s.SetProperty(m => m.State, OutboxState.Failed).SetProperty(m => m.LastError, error),
                ct
            );
    }

    /// <inheritdoc/>
    public async Task IncrementRetryAsync(Guid messageId, DateTimeOffset nextAttempt, CancellationToken ct = default)
    {
        await _context
            .OutboxMessages.Where(m => m.Id == messageId)
            .ExecuteUpdateAsync(
                s =>
                    s.SetProperty(m => m.RetryCount, m => m.RetryCount + 1)
                        .SetProperty(m => m.ScheduledAt, nextAttempt),
                ct
            );
    }
}
