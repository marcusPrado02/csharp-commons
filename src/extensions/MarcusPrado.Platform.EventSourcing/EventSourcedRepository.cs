namespace MarcusPrado.Platform.EventSourcing;

/// <summary>
/// Repository for event-sourced aggregates that automatically handles
/// snapshots every N events.
/// </summary>
public sealed class EventSourcedRepository<TState>
    where TState : class, new()
{
    private readonly IEventStore _eventStore;
    private readonly ISnapshotStore<TState> _snapshotStore;
    private readonly int _snapshotEvery;

    public EventSourcedRepository(
        IEventStore eventStore,
        ISnapshotStore<TState> snapshotStore,
        int snapshotEvery = 50)
    {
        _eventStore = eventStore;
        _snapshotStore = snapshotStore;
        _snapshotEvery = snapshotEvery;
    }

    public async Task<(TState State, long Version)> LoadAsync(string id, CancellationToken cancellationToken = default)
    {
        var snapshot = await _snapshotStore.LoadLatestAsync(id, cancellationToken);
        var fromSeq = snapshot?.SequenceNumber + 1 ?? 0;
        var state = snapshot?.State ?? new TState();

        var events = await _eventStore.LoadAsync(id, fromSeq, cancellationToken);
        state = AggregateEventReplayer.Replay(state, events);
        var version = await _eventStore.GetVersionAsync(id, cancellationToken);

        return (state, version);
    }

    public async Task SaveAsync(string id, IEnumerable<IDomainEvent> newEvents, long expectedVersion, CancellationToken cancellationToken = default)
    {
        await _eventStore.AppendAsync(id, newEvents, expectedVersion, cancellationToken);
        var version = await _eventStore.GetVersionAsync(id, cancellationToken);

        if ((version + 1) % _snapshotEvery == 0)
        {
            var (state, _) = await LoadAsync(id, cancellationToken);
            await _snapshotStore.SaveAsync(new EventSnapshot<TState>(id, version, state, DateTimeOffset.UtcNow), cancellationToken);
        }
    }
}
