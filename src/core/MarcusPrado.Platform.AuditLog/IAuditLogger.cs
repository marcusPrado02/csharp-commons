namespace MarcusPrado.Platform.AuditLog;

/// <summary>Writes audit entries to a persistent sink.</summary>
public interface IAuditLogger
{
    /// <summary>Records an audit entry asynchronously.</summary>
    Task LogAsync(AuditEntry entry, CancellationToken ct = default);

    /// <summary>Queries audit entries for a specific resource.</summary>
    Task<IReadOnlyList<AuditEntry>> QueryAsync(
        string resource,
        string? resourceId = null,
        DateTimeOffset? from = null,
        DateTimeOffset? to = null,
        CancellationToken ct = default);
}
