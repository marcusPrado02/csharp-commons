namespace MarcusPrado.Platform.EventSourcing.Projections;

/// <summary>
/// Abstraction for a key-addressable store that persists and retrieves read model instances.
/// </summary>
public interface IReadModelStore<T>
    where T : class
{
    /// <summary>Retrieves the read model with the given identifier, or <c>null</c> if it does not exist.</summary>
    /// <param name="id">The unique identifier of the read model.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>The read model, or <c>null</c> if not found.</returns>
    Task<T?> GetAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>Persists the read model under the given identifier, replacing any existing entry.</summary>
    /// <param name="id">The unique identifier to store the read model under.</param>
    /// <param name="readModel">The read model instance to save.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    Task SaveAsync(string id, T readModel, CancellationToken cancellationToken = default);

    /// <summary>Removes the read model with the given identifier if it exists.</summary>
    /// <param name="id">The unique identifier of the read model to delete.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    Task DeleteAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>Returns all read models currently held in the store.</summary>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>A read-only list of all stored read model instances.</returns>
    Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default);
}
