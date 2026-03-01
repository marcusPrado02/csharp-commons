namespace MarcusPrado.Platform.Domain.Events;

/// <summary>
/// Dispatches domain events to all registered handlers.
/// Implementations may dispatch in-process (e.g. via MediatR / custom pipeline)
/// or via a message broker, depending on the deployment topology.
/// </summary>
public interface IDomainEventPublisher
{
    /// <summary>
    /// Publishes a single domain event asynchronously.
    /// </summary>
    /// <typeparam name="T">Concrete event type.</typeparam>
    /// <param name="domainEvent">The event to publish.</param>
    /// <param name="cancellationToken">Propagates cancellation.</param>
    Task PublishAsync<T>(T domainEvent, CancellationToken cancellationToken = default)
        where T : IDomainEvent;

    /// <summary>
    /// Publishes a batch of domain events, preserving order.
    /// </summary>
    /// <param name="domainEvents">Events to dispatch.</param>
    /// <param name="cancellationToken">Propagates cancellation.</param>
    Task PublishAllAsync(
        IEnumerable<IDomainEvent> domainEvents,
        CancellationToken cancellationToken = default);
}
