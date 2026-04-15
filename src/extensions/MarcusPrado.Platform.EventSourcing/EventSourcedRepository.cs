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

    /// <summary>
    /// Initializes the repository with the given event store, snapshot store, and snapshot frequency.
    /// </summary>
    /// <param name="eventStore">The event store used to persist and load domain events.</param>
    /// <param name="snapshotStore">The snapshot store used to persist and load state snapshots.</param>
    /// <param name="snapshotEvery">Number of events between automatic snapshots; defaults to 50.</param>
    public EventSourcedRepository(
        IEventStore eventStore,
        ISnapshotStore<TState> snapshotStore,
        int snapshotEvery = 50)
    {
        _eventStore = eventStore;
        _snapshotStore = snapshotStore;
        _snapshotEvery = snapshotEvery;
    }

    /// <summary>
    /// Loads the current state and version of an aggregate by replaying events from the latest snapshot.
    /// </summary>
    /// <param name="id">The stream identifier of the aggregate.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>A tuple containing the reconstructed state and the current event stream version.</returns>
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

    /// <summary>
    /// Appends new domain events to the stream and takes a snapshot when the threshold is reached.
    /// </summary>
    /// <param name="id">The stream identifier of the aggregate.</param>
    /// <param name="newEvents">The new domain events to append.</param>
    /// <param name="expectedVersion">The version the stream is expected to be at for optimistic concurrency.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
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
