namespace MarcusPrado.Platform.EventSourcing;

public interface IEventStore
{
    /// <summary>Appends events to the stream. Throws OptimisticConcurrencyException if expectedVersion doesn't match.</summary>
    Task AppendAsync(string streamId, IEnumerable<IDomainEvent> events, long expectedVersion, CancellationToken cancellationToken = default);

    /// <summary>Loads all events for the stream starting from the given sequence number.</summary>
    Task<IReadOnlyList<StoredEvent>> LoadAsync(string streamId, long fromSequence = 0, CancellationToken cancellationToken = default);

    /// <summary>Returns the current version (latest sequence number) of the stream, or -1 if not found.</summary>
    Task<long> GetVersionAsync(string streamId, CancellationToken cancellationToken = default);
}
