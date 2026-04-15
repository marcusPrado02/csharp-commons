namespace MarcusPrado.Platform.EventSourcing;

/// <summary>
/// Abstraction for an append-only event store that persists and retrieves domain event streams.
/// </summary>
public interface IEventStore
{
    /// <summary>Appends events to the stream. Throws OptimisticConcurrencyException if expectedVersion doesn't match.</summary>
    /// <param name="streamId">The identifier of the event stream.</param>
    /// <param name="events">The domain events to append.</param>
    /// <param name="expectedVersion">The version the stream must be at for the write to succeed.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    Task AppendAsync(
        string streamId,
        IEnumerable<IDomainEvent> events,
        long expectedVersion,
        CancellationToken cancellationToken = default
    );

    /// <summary>Loads all events for the stream starting from the given sequence number.</summary>
    /// <param name="streamId">The identifier of the event stream.</param>
    /// <param name="fromSequence">The inclusive sequence number to start loading from; defaults to 0.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>An ordered, read-only list of stored events.</returns>
    Task<IReadOnlyList<StoredEvent>> LoadAsync(
        string streamId,
        long fromSequence = 0,
        CancellationToken cancellationToken = default
    );

    /// <summary>Returns the current version (latest sequence number) of the stream, or -1 if not found.</summary>
    /// <param name="streamId">The identifier of the event stream.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>The latest sequence number, or <c>-1</c> if the stream does not exist.</returns>
    Task<long> GetVersionAsync(string streamId, CancellationToken cancellationToken = default);
}
