namespace MarcusPrado.Platform.EventSourcing;

/// <summary>
/// Abstraction for a store that persists and retrieves aggregate state snapshots.
/// </summary>
public interface ISnapshotStore<TState>
{
    /// <summary>Persists the given snapshot, replacing any previously stored snapshot for the same stream.</summary>
    /// <param name="snapshot">The snapshot to save.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    Task SaveAsync(EventSnapshot<TState> snapshot, CancellationToken cancellationToken = default);

    /// <summary>Retrieves the most recent snapshot for the specified stream, or <c>null</c> if none exists.</summary>
    /// <param name="streamId">The identifier of the event stream.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>The latest <see cref="EventSnapshot{TState}"/>, or <c>null</c> if no snapshot is stored.</returns>
    Task<EventSnapshot<TState>?> LoadLatestAsync(string streamId, CancellationToken cancellationToken = default);
}
