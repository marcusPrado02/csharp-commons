namespace MarcusPrado.Platform.EventSourcing;

/// <summary>An event as persisted in the event store.</summary>
public sealed record StoredEvent(
    Guid EventId,
    string StreamId,
    long SequenceNumber,
    string EventType,
    string Payload, // JSON-serialized event
    DateTimeOffset OccurredOn
);
