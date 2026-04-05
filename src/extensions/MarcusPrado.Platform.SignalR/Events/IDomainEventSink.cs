namespace MarcusPrado.Platform.SignalR.Events;

/// <summary>
/// Handles domain events by dispatching them to an external sink (e.g. SignalR).
/// </summary>
public interface IDomainEventSink
{
    /// <summary>Processes the given domain event.</summary>
    Task HandleAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default);
}
