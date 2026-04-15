namespace MarcusPrado.Platform.AuditLog;

/// <summary>An immutable audit log record capturing who did what and when.</summary>
/// <param name="Id">Unique identifier for this audit entry.</param>
/// <param name="Action">The type of operation that was performed.</param>
/// <param name="Resource">The name of the resource type that was acted upon.</param>
/// <param name="ResourceId">The identifier of the specific resource instance.</param>
/// <param name="ActorId">The identifier of the user or service that performed the action, if known.</param>
/// <param name="TenantId">The tenant context in which the action occurred, if applicable.</param>
/// <param name="Timestamp">The UTC date and time when the action was performed.</param>
/// <param name="Changes">A serialized representation of what changed, if applicable.</param>
/// <param name="IpAddress">The IP address of the client that initiated the action, if available.</param>
/// <param name="UserAgent">The user-agent string of the client, if available.</param>
/// <param name="Metadata">Additional arbitrary key-value metadata associated with the entry.</param>
public sealed record AuditEntry(
    Guid Id,
    AuditAction Action,
    string Resource,
    string ResourceId,
    string? ActorId,
    string? TenantId,
    DateTimeOffset Timestamp,
    string? Changes,
    string? IpAddress,
    string? UserAgent,
    IDictionary<string, string>? Metadata = null)
{
    /// <summary>Creates a new audit entry with auto-generated ID and current UTC timestamp.</summary>
    /// <param name="action">The type of operation being recorded.</param>
    /// <param name="resource">The name of the resource type that was acted upon.</param>
    /// <param name="resourceId">The identifier of the specific resource instance.</param>
    /// <param name="actorId">The identifier of the actor that performed the action.</param>
    /// <param name="tenantId">The tenant context in which the action occurred.</param>
    /// <param name="changes">A serialized representation of what changed.</param>
    /// <param name="ipAddress">The IP address of the client that initiated the action.</param>
    /// <param name="userAgent">The user-agent string of the client.</param>
    /// <param name="metadata">Additional arbitrary key-value metadata.</param>
    /// <returns>A new <see cref="AuditEntry"/> with a generated <see cref="Guid"/> and the current UTC timestamp.</returns>
    public static AuditEntry Create(
        AuditAction action,
        string resource,
        string resourceId,
        string? actorId = null,
        string? tenantId = null,
        string? changes = null,
        string? ipAddress = null,
        string? userAgent = null,
        IDictionary<string, string>? metadata = null)
            => new(
                Guid.NewGuid(), action, resource, resourceId,
                actorId, tenantId, DateTimeOffset.UtcNow,
                changes, ipAddress, userAgent, metadata);
}

/// <summary>The type of operation being audited.</summary>
public enum AuditAction
{
    /// <summary>A new resource was created.</summary>
    Created,

    /// <summary>An existing resource was modified.</summary>
    Updated,

    /// <summary>A resource was deleted.</summary>
    Deleted,

    /// <summary>A resource was viewed or read.</summary>
    Viewed,

    /// <summary>Data was exported from the system.</summary>
    Exported,

    /// <summary>Data was imported into the system.</summary>
    Imported,

    /// <summary>A resource or action was approved.</summary>
    Approved,

    /// <summary>A resource or action was rejected.</summary>
    Rejected,

    /// <summary>A user successfully authenticated.</summary>
    Login,

    /// <summary>A user ended their authenticated session.</summary>
    Logout,

    /// <summary>A permission was granted to a principal.</summary>
    PermissionGranted,

    /// <summary>A previously granted permission was revoked.</summary>
    PermissionRevoked,

    /// <summary>A custom or application-specific action was performed.</summary>
    Custom,
}
