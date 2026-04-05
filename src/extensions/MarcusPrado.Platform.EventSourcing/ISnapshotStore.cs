namespace MarcusPrado.Platform.EventSourcing;

public interface ISnapshotStore<TState>
{
    Task SaveAsync(EventSnapshot<TState> snapshot, CancellationToken cancellationToken = default);

    Task<EventSnapshot<TState>?> LoadLatestAsync(string streamId, CancellationToken cancellationToken = default);
}
