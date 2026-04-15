using Microsoft.Extensions.Logging;

namespace MarcusPrado.Platform.AuditLog;

/// <summary>
/// <see cref="IAuditLogger"/> sink that writes audit entries via
/// <see cref="ILogger"/> (stdout-friendly, works with Serilog / OTel).
/// </summary>
public sealed partial class LoggingAuditSink : IAuditLogger
{
    private readonly ILogger<LoggingAuditSink> _logger;
    private readonly InMemoryAuditSink _store = new();

    /// <summary>Initialises the sink with the given logger.</summary>
    public LoggingAuditSink(ILogger<LoggingAuditSink> logger) => _logger = logger;

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "AUDIT {Action} {Resource}/{ResourceId} by {ActorId} at {Timestamp}"
    )]
    private static partial void LogAudit(
        ILogger logger,
        AuditAction action,
        string resource,
        string resourceId,
        string? actorId,
        DateTimeOffset timestamp
    );

    /// <inheritdoc />
    public Task LogAsync(AuditEntry entry, CancellationToken ct = default)
    {
        LogAudit(_logger, entry.Action, entry.Resource, entry.ResourceId, entry.ActorId, entry.Timestamp);
        return _store.LogAsync(entry, ct);
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<AuditEntry>> QueryAsync(
        string resource,
        string? resourceId = null,
        DateTimeOffset? from = null,
        DateTimeOffset? to = null,
        CancellationToken ct = default
    ) => _store.QueryAsync(resource, resourceId, from, to, ct);
}
