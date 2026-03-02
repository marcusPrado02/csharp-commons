namespace MarcusPrado.Platform.Governance.ADR;

/// <summary>Persists and retrieves Architecture Decision Records.</summary>
public interface IAdrStore
{
    /// <summary>Returns all stored ADR records.</summary>
    Task<IReadOnlyList<AdrRecord>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>Returns a single ADR by number, or <c>null</c> if not found.</summary>
    Task<AdrRecord?> GetByNumberAsync(int number, CancellationToken cancellationToken = default);

    /// <summary>Saves or overwrites an ADR record.</summary>
    Task SaveAsync(AdrRecord record, CancellationToken cancellationToken = default);
}
