using MarcusPrado.Platform.Domain.Events;

namespace MarcusPrado.Platform.EventRouting.Handling;

/// <summary>
/// Defines a strongly-typed handler for a specific domain event type.
/// Implement this interface for each event type your component needs to react to.
/// </summary>
/// <typeparam name="TEvent">The concrete <see cref="IDomainEvent"/> type handled.</typeparam>
public interface IDomainEventHandler<in TEvent>
    where TEvent : IDomainEvent
{
    /// <summary>
    /// Handles the given <paramref name="domainEvent"/> asynchronously.
    /// </summary>
    /// <param name="domainEvent">The domain event instance to handle.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    Task HandleAsync(TEvent domainEvent, CancellationToken cancellationToken = default);
}
