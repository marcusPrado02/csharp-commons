namespace MarcusPrado.Platform.Security.Audit;

/// <summary>Receives <see cref="SecurityAuditEvent"/> records and forwards them to an audit destination (e.g. log, database, SIEM).</summary>
public interface ISecurityAuditSink { }
