namespace MarcusPrado.Platform.Domain.Events;

/// <summary>
/// Convenient abstract base for domain events.
/// Auto-fills <see cref="EventId"/> and <see cref="OccurredOn"/> on construction;
/// deriving types only need to carry their business payload.
/// </summary>
/// <remarks>
/// Use <c>record</c> syntax for derived types to get structural equality and
/// <c>ToString</c> for free:
/// <code>
/// public sealed record OrderPlacedEvent(Guid OrderId, decimal Total) : DomainEvent;
/// </code>
/// </remarks>
public abstract record DomainEvent : IDomainEvent
{
    /// <inheritdoc/>
    public Guid EventId { get; } = Guid.NewGuid();

    /// <inheritdoc/>
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;

    /// <inheritdoc/>
    /// <remarks>
    /// Defaults to <c>GetType().Name</c>; override to supply a stable,
    /// version-agnostic name that survives class renames.
    /// </remarks>
    public virtual string EventType => GetType().Name;
}
