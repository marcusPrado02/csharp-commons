namespace MarcusPrado.Platform.EventSourcing;

/// <summary>A point-in-time snapshot of an aggregate's state.</summary>
public sealed record EventSnapshot<TState>(
    string StreamId,
    long SequenceNumber,
    TState State,
    DateTimeOffset CreatedAt
);
