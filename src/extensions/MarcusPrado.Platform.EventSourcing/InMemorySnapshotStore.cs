namespace MarcusPrado.Platform.EventSourcing;

/// <summary>
/// In-memory implementation of <see cref="ISnapshotStore{TState}"/> intended for testing and development.
/// </summary>
public sealed class InMemorySnapshotStore<TState> : ISnapshotStore<TState>
{
    private readonly Dictionary<string, EventSnapshot<TState>> _snapshots = new();

    /// <inheritdoc />
    public Task SaveAsync(EventSnapshot<TState> snapshot, CancellationToken cancellationToken = default)
    {
        _snapshots[snapshot.StreamId] = snapshot;
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<EventSnapshot<TState>?> LoadLatestAsync(string streamId, CancellationToken cancellationToken = default)
    {
        _snapshots.TryGetValue(streamId, out var snapshot);
        return Task.FromResult(snapshot);
    }
}
