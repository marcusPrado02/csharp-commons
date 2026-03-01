namespace MarcusPrado.Platform.Domain.Auditing;

/// <summary>
/// Immutable value object that carries full audit trail metadata for an entity
/// or aggregate root.  Store it as a single embedded column group rather than
/// duplicating individual fields across tables.
/// </summary>
/// <remarks>
/// Support for soft-delete is provided by <see cref="DeletedAt"/> /<see cref="DeletedBy"/>.
/// A non-null <see cref="DeletedAt"/> means the record is logically deleted.
/// </remarks>
public sealed record AuditRecord
{
    /// <summary>Timestamp when the entity was first persisted.</summary>
    public DateTimeOffset CreatedAt { get; init; }

    /// <summary>Identifier (user-id, service account, etc.) that created the entity.</summary>
    public string CreatedBy { get; init; }

    /// <summary>Timestamp of the most recent modification, or <c>null</c> if never updated.</summary>
    public DateTimeOffset? UpdatedAt { get; init; }

    /// <summary>Identifier that performed the most recent modification.</summary>
    public string? UpdatedBy { get; init; }

    /// <summary>Timestamp when the entity was soft-deleted, or <c>null</c> if still active.</summary>
    public DateTimeOffset? DeletedAt { get; init; }

    /// <summary>Identifier that soft-deleted the entity.</summary>
    public string? DeletedBy { get; init; }

    /// <summary>
    /// <c>true</c> when <see cref="DeletedAt"/> is set (the entity is logically deleted).
    /// </summary>
    public bool IsDeleted => DeletedAt.HasValue;

    private AuditRecord(string createdBy, DateTimeOffset createdAt)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(createdBy, nameof(createdBy));
        CreatedBy = createdBy;
        CreatedAt = createdAt;
    }

    // ── Factories ─────────────────────────────────────────────────────────

    /// <summary>Creates an audit record for a newly-created entity.</summary>
    /// <param name="createdBy">Identifier of the creator (user id, service name, etc.).</param>
    /// <param name="at">Creation timestamp; defaults to <see cref="DateTimeOffset.UtcNow"/> when <c>null</c>.</param>
    public static AuditRecord Create(string createdBy, DateTimeOffset? at = null) =>
        new(createdBy, at ?? DateTimeOffset.UtcNow);

    // ── Mutations (returns a new record — immutable) ──────────────────────────

    /// <summary>Returns a new record stamped with an update from <paramref name="updatedBy"/>.</summary>
    public AuditRecord Update(string updatedBy, DateTimeOffset? at = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(updatedBy, nameof(updatedBy));
        return this with { UpdatedBy = updatedBy, UpdatedAt = at ?? DateTimeOffset.UtcNow };
    }

    /// <summary>Returns a new record marked as soft-deleted.</summary>
    public AuditRecord Delete(string deletedBy, DateTimeOffset? at = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(deletedBy, nameof(deletedBy));
        return this with { DeletedBy = deletedBy, DeletedAt = at ?? DateTimeOffset.UtcNow };
    }

    /// <summary>Returns a new record with <see cref="DeletedAt"/> and <see cref="DeletedBy"/> cleared (undelete).</summary>
    public AuditRecord Restore() => this with { DeletedAt = null, DeletedBy = null };
}
