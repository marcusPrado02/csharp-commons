namespace MarcusPrado.Platform.Domain.Events;

/// <summary>
/// Marker contract for all domain events produced by aggregate roots.
/// Domain events are immutable facts that describe something that happened
/// inside the domain.  They are raised during a command and dispatched after
/// the unit of work commits.
/// </summary>
public interface IDomainEvent
{
    /// <summary>Unique identifier for this particular event occurrence.</summary>
    Guid EventId { get; }

    /// <summary>Wall-clock timestamp at which the event was raised.</summary>
    DateTimeOffset OccurredOn { get; }

    /// <summary>
    /// Logical event type name used for routing, schema-registry look-up and
    /// serialisation (e.g. <c>"order.item.added"</c>).
    /// </summary>
    string EventType { get; }
}
