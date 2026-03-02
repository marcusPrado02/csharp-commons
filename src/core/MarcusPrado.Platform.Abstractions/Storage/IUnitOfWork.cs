namespace MarcusPrado.Platform.Abstractions.Storage;

/// <summary>
/// Represents a unit-of-work that groups a set of operations into an
/// atomic, consistent, isolated transaction.
/// </summary>
public interface IUnitOfWork
{
    /// <summary>Opens a new database transaction.</summary>
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>Persists all changes and commits the active transaction.</summary>
    Task CommitAsync(CancellationToken cancellationToken = default);

    /// <summary>Discards all changes and rolls back the active transaction.</summary>
    Task RollbackAsync(CancellationToken cancellationToken = default);

    /// <summary>Persists all tracked changes without committing a transaction.</summary>
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
