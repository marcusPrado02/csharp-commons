namespace MarcusPrado.Platform.Domain.Auditing;

/// <summary>
/// Marks an entity or aggregate root as having a full audit trail.
/// Infrastructure (e.g. EF Core interceptors) reads this interface to
/// auto-fill <see cref="Audit"/> before persisting changes.
/// </summary>
public interface IAuditable
{
    /// <summary>The audit trail for this entity (creation, updates, soft-delete).</summary>
    AuditRecord Audit { get; }
}
