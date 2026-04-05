using MarcusPrado.Platform.Domain.Events;

namespace MarcusPrado.Platform.EventRouting.Routing;

/// <summary>
/// Dispatches a collection of domain events, typically called after
/// a successful <c>SaveChanges</c> on the unit of work.
/// </summary>
public interface IDomainEventDispatcher
{
    /// <summary>
    /// Dispatches all <paramref name="events"/> asynchronously in order.
    /// </summary>
    /// <param name="events">The domain events to dispatch.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    Task DispatchAsync(IEnumerable<IDomainEvent> events, CancellationToken cancellationToken = default);
}
