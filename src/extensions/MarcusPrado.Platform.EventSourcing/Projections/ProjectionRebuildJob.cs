namespace MarcusPrado.Platform.EventSourcing.Projections;

using System.Text.Json;

/// <summary>
/// Replays all events from an event store stream through the projection engine to rebuild read models.
/// </summary>
public sealed class ProjectionRebuildJob
{
    private readonly IEventStore _eventStore;
    private readonly ProjectionEngine _engine;

    /// <summary>
    /// Initializes the rebuild job with the event store and projection engine to use.
    /// </summary>
    /// <param name="eventStore">The event store from which events are loaded.</param>
    /// <param name="engine">The projection engine that processes each replayed event.</param>
    public ProjectionRebuildJob(IEventStore eventStore, ProjectionEngine engine)
    {
        _eventStore = eventStore;
        _engine = engine;
    }

    /// <summary>Replays all events from the specified stream through the projection engine.</summary>
    public async Task RebuildAsync(string streamId, CancellationToken cancellationToken = default)
    {
        var events = await _eventStore.LoadAsync(streamId, 0, cancellationToken);
        foreach (var storedEvent in events)
        {
            var eventType = Type.GetType(storedEvent.EventType);
            if (eventType is null)
                continue;

            if (JsonSerializer.Deserialize(storedEvent.Payload, eventType) is not IDomainEvent domainEvent)
                continue;

            await _engine.DispatchAsync(domainEvent, cancellationToken);
        }
    }
}
