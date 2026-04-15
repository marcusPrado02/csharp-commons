using MarcusPrado.Platform.Domain.Events;
using Microsoft.Extensions.Logging;

namespace MarcusPrado.Platform.EventRouting.Routing;

/// <summary>
/// Dispatches domain events post-<c>SaveChanges</c> by iterating the supplied
/// event collection and forwarding each one to the <see cref="DomainEventRouter"/>.
/// </summary>
public sealed class DomainEventDispatcher : IDomainEventDispatcher
{
    private readonly DomainEventRouter _router;
    private readonly ILogger<DomainEventDispatcher> _logger;

    /// <summary>
    /// Initialises a new <see cref="DomainEventDispatcher"/>.
    /// </summary>
    /// <param name="router">The router that delivers events to their handlers.</param>
    /// <param name="logger">Logger for diagnostic output.</param>
    public DomainEventDispatcher(DomainEventRouter router, ILogger<DomainEventDispatcher> logger)
    {
        ArgumentNullException.ThrowIfNull(router);
        ArgumentNullException.ThrowIfNull(logger);

        _router = router;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task DispatchAsync(IEnumerable<IDomainEvent> events, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(events);

        foreach (var domainEvent in events)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug(
                    "Dispatching domain event {EventType} ({EventId}) occurred at {OccurredOn}",
                    domainEvent.GetType().Name,
                    domainEvent.EventId,
                    domainEvent.OccurredOn
                );
            }

            await _router.RouteAsync(domainEvent, cancellationToken).ConfigureAwait(false);
        }
    }
}
