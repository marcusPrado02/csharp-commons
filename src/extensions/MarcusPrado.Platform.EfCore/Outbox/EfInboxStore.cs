using MarcusPrado.Platform.EfCore.DbContext;
using MarcusPrado.Platform.OutboxInbox.Inbox;
using Microsoft.EntityFrameworkCore;

namespace MarcusPrado.Platform.EfCore.Outbox;

/// <summary>EF Core-backed implementation of <see cref="IInboxStore"/>.</summary>
public sealed class EfInboxStore : IInboxStore
{
    private readonly AppDbContextBase _context;

    /// <summary>Initialises the store with the given db context.</summary>
    public EfInboxStore(AppDbContextBase context)
    {
        ArgumentNullException.ThrowIfNull(context);
        _context = context;
    }

    /// <inheritdoc/>
    public async Task SaveAsync(InboxMessage message, CancellationToken ct = default)
    {
        _context.InboxMessages.Add(message);
        await _context.SaveChangesAsync(ct);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<InboxMessage>> GetPendingAsync(
        int batchSize,
        CancellationToken ct = default)
    {
        return await _context.InboxMessages
            .Where(m => m.State == InboxState.Pending)
            .OrderBy(m => m.ReceivedAt)
            .Take(batchSize)
            .ToListAsync(ct);
    }

    /// <inheritdoc/>
    public async Task MarkProcessedAsync(Guid id, CancellationToken ct = default)
    {
        await _context.InboxMessages
            .Where(m => m.Id == id)
            .ExecuteUpdateAsync(
                s => s.SetProperty(m => m.State, InboxState.Processed)
                       .SetProperty(m => m.ProcessedAt, DateTimeOffset.UtcNow),
                ct);
    }

    /// <inheritdoc/>
    public async Task MarkFailedAsync(Guid id, string error, CancellationToken ct = default)
    {
        await _context.InboxMessages
            .Where(m => m.Id == id)
            .ExecuteUpdateAsync(
                s => s.SetProperty(m => m.State, InboxState.Failed)
                       .SetProperty(m => m.LastError, error),
                ct);
    }

    /// <inheritdoc/>
    public async Task MarkDuplicateAsync(Guid id, CancellationToken ct = default)
    {
        await _context.InboxMessages
            .Where(m => m.Id == id)
            .ExecuteUpdateAsync(
                s => s.SetProperty(m => m.State, InboxState.Duplicate),
                ct);
    }

    /// <inheritdoc/>
    public async Task IncrementRetryAsync(
        Guid id,
        DateTimeOffset nextAttempt,
        CancellationToken ct = default)
    {
        await _context.InboxMessages
            .Where(m => m.Id == id)
            .ExecuteUpdateAsync(
                s => s.SetProperty(m => m.RetryCount, m => m.RetryCount + 1),
                ct);
    }
}
