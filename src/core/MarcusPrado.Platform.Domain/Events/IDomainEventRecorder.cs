namespace MarcusPrado.Platform.Domain.Events;

/// <summary>
/// Exposes the domain events accumulated by an aggregate root so that
/// infrastructure (unit of work, outbox publisher) can harvest and dispatch
/// them at commit time.
/// </summary>
public interface IDomainEventRecorder
{
    /// <summary>Domain events recorded since the last <see cref="ClearDomainEvents"/> call.</summary>
    IReadOnlyCollection<IDomainEvent> DomainEvents { get; }

    /// <summary>
    /// Removes all recorded events.  Called by infrastructure after the events
    /// have been written to the outbox or dispatched in-process.
    /// </summary>
    void ClearDomainEvents();
}
