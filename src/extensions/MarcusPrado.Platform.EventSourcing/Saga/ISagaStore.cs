namespace MarcusPrado.Platform.EventSourcing.Saga;

/// <summary>
/// Persistence abstraction for saga instances.
/// </summary>
/// <typeparam name="TState">The state type of the saga.</typeparam>
public interface ISagaStore<TState>
{
    /// <summary>
    /// Persists or updates a saga instance.
    /// </summary>
    /// <param name="saga">The saga to save.</param>
    /// <param name="ct">Cancellation token.</param>
    Task SaveAsync(ISaga<TState> saga, CancellationToken ct = default);

    /// <summary>
    /// Loads a saga instance by its identifier.
    /// </summary>
    /// <param name="sagaId">The saga identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The saga if found; otherwise <see langword="null"/>.</returns>
    Task<ISaga<TState>?> LoadAsync(string sagaId, CancellationToken ct = default);
}
