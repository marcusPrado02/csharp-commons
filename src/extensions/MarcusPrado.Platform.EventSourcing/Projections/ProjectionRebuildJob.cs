namespace MarcusPrado.Platform.EventSourcing.Projections;

using System.Text.Json;

public sealed class ProjectionRebuildJob
{
    private readonly IEventStore _eventStore;
    private readonly ProjectionEngine _engine;

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
            if (eventType is null) continue;

            if (JsonSerializer.Deserialize(storedEvent.Payload, eventType) is not IDomainEvent domainEvent) continue;

            await _engine.DispatchAsync(domainEvent, cancellationToken);
        }
    }
}
