using System.Collections.Concurrent;

namespace MarcusPrado.Platform.AuditLog;

/// <summary>
/// In-memory <see cref="IAuditLogger"/> sink — useful in unit tests.
/// </summary>
public sealed class InMemoryAuditSink : IAuditLogger
{
    private readonly ConcurrentQueue<AuditEntry> _log = new();

    /// <summary>All audit entries recorded so far (in insertion order).</summary>
    public IReadOnlyList<AuditEntry> Entries => [.. _log];

    /// <inheritdoc />
    public Task LogAsync(AuditEntry entry, CancellationToken ct = default)
    {
        _log.Enqueue(entry);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<AuditEntry>> QueryAsync(
        string resource,
        string? resourceId = null,
        DateTimeOffset? from = null,
        DateTimeOffset? to = null,
        CancellationToken ct = default)
    {
        var q = _log.Where(e => e.Resource == resource);

        if (resourceId is not null)
            q = q.Where(e => e.ResourceId == resourceId);
        if (from.HasValue)
            q = q.Where(e => e.Timestamp >= from.Value);
        if (to.HasValue)
            q = q.Where(e => e.Timestamp <= to.Value);

        return Task.FromResult<IReadOnlyList<AuditEntry>>(q.ToList());
    }

    /// <summary>Clears all recorded entries — useful between test cases.</summary>
    public void Clear() => _log.Clear();
}
