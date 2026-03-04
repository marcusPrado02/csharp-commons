namespace MarcusPrado.Platform.AuditLog;

/// <summary>An immutable audit log record capturing who did what and when.</summary>
public sealed record AuditEntry(
    Guid            Id,
    AuditAction     Action,
    string          Resource,
    string          ResourceId,
    string?         ActorId,
    string?         TenantId,
    DateTimeOffset  Timestamp,
    string?         Changes,
    string?         IpAddress,
    string?         UserAgent,
    IDictionary<string, string>? Metadata = null)
{
    /// <summary>Creates a new audit entry with auto-generated ID and current UTC timestamp.</summary>
    public static AuditEntry Create(
        AuditAction action,
        string      resource,
        string      resourceId,
        string?     actorId   = null,
        string?     tenantId  = null,
        string?     changes   = null,
        string?     ipAddress = null,
        string?     userAgent = null,
        IDictionary<string, string>? metadata = null)
            => new(
                Guid.NewGuid(), action, resource, resourceId,
                actorId, tenantId, DateTimeOffset.UtcNow,
                changes, ipAddress, userAgent, metadata);
}

/// <summary>The type of operation being audited.</summary>
public enum AuditAction
{
    Created,
    Updated,
    Deleted,
    Viewed,
    Exported,
    Imported,
    Approved,
    Rejected,
    Login,
    Logout,
    PermissionGranted,
    PermissionRevoked,
    Custom,
}
