using System.Text.Json;

namespace MarcusPrado.Platform.EventSourcing;

/// <summary>
/// Replays a sequence of stored events onto an aggregate state object using reflection.
/// </summary>
public static class AggregateEventReplayer
{
    /// <summary>
    /// Replays stored events onto a state object by calling Apply(TEvent) methods
    /// found via reflection. Returns the mutated state.
    /// </summary>
    /// <param name="state">The initial state object to apply events to.</param>
    /// <param name="events">The ordered sequence of stored events to replay.</param>
    /// <typeparam name="TState">The type of the aggregate state.</typeparam>
    /// <returns>The state object after all applicable events have been applied.</returns>
    public static TState Replay<TState>(TState state, IEnumerable<StoredEvent> events)
        where TState : class
    {
        foreach (var storedEvent in events)
        {
            var eventType = Type.GetType(storedEvent.EventType);
            if (eventType is null)
                continue;

            var domainEvent = JsonSerializer.Deserialize(storedEvent.Payload, eventType);
            if (domainEvent is null)
                continue;

            var applyMethod = state.GetType().GetMethod("Apply", [eventType]);
            applyMethod?.Invoke(state, [domainEvent]);
        }

        return state;
    }
}
