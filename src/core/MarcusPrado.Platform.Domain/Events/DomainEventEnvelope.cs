namespace MarcusPrado.Platform.Domain.Events;

/// <summary>
/// Wraps an <see cref="IDomainEvent"/> with the identity and version of the
/// aggregate that raised it.  Used by infrastructure (outbox, message bus) to
/// route and persist events without coupling to concrete event types.
/// </summary>
/// <param name="Event">The domain event payload.</param>
/// <param name="AggregateId">String representation of the aggregate identifier.</param>
/// <param name="AggregateType">
/// Simple type name of the aggregate root (e.g. <c>"Order"</c>).
/// </param>
/// <param name="AggregateVersion">Version of the aggregate at the time the event was raised.</param>
public sealed record DomainEventEnvelope(
    IDomainEvent Event,
    string AggregateId,
    string AggregateType,
    int AggregateVersion
)
{
    /// <summary>
    /// Convenience factory that infers the <paramref name="aggregateType"/> from
    /// the runtime type of the aggregate.
    /// </summary>
    public static DomainEventEnvelope Create<TId>(
        IDomainEvent domainEvent,
        TId aggregateId,
        string aggregateType,
        int aggregateVersion
    )
        where TId : notnull =>
        new(
            Event: domainEvent,
            AggregateId: aggregateId.ToString()!,
            AggregateType: aggregateType,
            AggregateVersion: aggregateVersion
        );
}
