using System.Text.Json;

namespace MarcusPrado.Platform.EventSourcing;

public static class AggregateEventReplayer
{
    /// <summary>
    /// Replays stored events onto a state object by calling Apply(TEvent) methods
    /// found via reflection. Returns the mutated state.
    /// </summary>
    public static TState Replay<TState>(TState state, IEnumerable<StoredEvent> events)
        where TState : class
    {
        foreach (var storedEvent in events)
        {
            var eventType = Type.GetType(storedEvent.EventType);
            if (eventType is null) continue;

            var domainEvent = JsonSerializer.Deserialize(storedEvent.Payload, eventType);
            if (domainEvent is null) continue;

            var applyMethod = state.GetType().GetMethod("Apply", [eventType]);
            applyMethod?.Invoke(state, [domainEvent]);
        }

        return state;
    }
}
