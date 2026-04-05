namespace MarcusPrado.Platform.EventSourcing;

public sealed class InMemorySnapshotStore<TState> : ISnapshotStore<TState>
{
    private readonly Dictionary<string, EventSnapshot<TState>> _snapshots = new();

    public Task SaveAsync(EventSnapshot<TState> snapshot, CancellationToken cancellationToken = default)
    {
        _snapshots[snapshot.StreamId] = snapshot;
        return Task.CompletedTask;
    }

    public Task<EventSnapshot<TState>?> LoadLatestAsync(string streamId, CancellationToken cancellationToken = default)
    {
        _snapshots.TryGetValue(streamId, out var snapshot);
        return Task.FromResult(snapshot);
    }
}
