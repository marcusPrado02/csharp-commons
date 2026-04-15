using System.Text.Json;

namespace MarcusPrado.Platform.EventSourcing;

/// <summary>
/// Thread-safe, in-memory implementation of <see cref="IEventStore"/> intended for testing and development.
/// </summary>
public sealed class InMemoryEventStore : IEventStore, IDisposable
{
    private readonly Dictionary<string, List<StoredEvent>> _streams = new();
    private readonly SemaphoreSlim _lock = new(1, 1);

    /// <inheritdoc />
    public async Task AppendAsync(string streamId, IEnumerable<IDomainEvent> events, long expectedVersion, CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            var currentVersion = _streams.TryGetValue(streamId, out var existing)
                ? (long)(existing.Count - 1)
                : -1L;

            if (currentVersion != expectedVersion)
                throw new OptimisticConcurrencyException(streamId, expectedVersion, currentVersion);

            if (!_streams.ContainsKey(streamId))
                _streams[streamId] = [];

            var stream = _streams[streamId];
            foreach (var evt in events)
            {
                stream.Add(new StoredEvent(
                    EventId: Guid.NewGuid(),
                    StreamId: streamId,
                    SequenceNumber: stream.Count,
                    EventType: evt.GetType().AssemblyQualifiedName ?? evt.GetType().Name,
                    Payload: JsonSerializer.Serialize(evt, evt.GetType()),
                    OccurredOn: DateTimeOffset.UtcNow));
            }
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<StoredEvent>> LoadAsync(string streamId, long fromSequence = 0, CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            if (!_streams.TryGetValue(streamId, out var stream))
                return [];
            return stream.Where(e => e.SequenceNumber >= fromSequence).ToList();
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <inheritdoc />
    public async Task<long> GetVersionAsync(string streamId, CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            return _streams.TryGetValue(streamId, out var stream) ? stream.Count - 1 : -1L;
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <summary>Releases the semaphore used for thread synchronization.</summary>
    public void Dispose() => _lock.Dispose();
}
