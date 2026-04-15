using MarcusPrado.Platform.Domain.Events;
using MarcusPrado.Platform.EventRouting.Handling;
using MarcusPrado.Platform.EventRouting.Pipeline;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MarcusPrado.Platform.EventRouting.Routing;

/// <summary>
/// Routes a domain event to all <see cref="IDomainEventHandler{TEvent}"/> instances
/// registered in the DI container for that specific event type.
/// Handler execution is wrapped by an optional <see cref="EventHandlerPipeline"/>.
/// </summary>
public sealed class DomainEventRouter
{
    private readonly IServiceProvider _serviceProvider;
    private readonly EventHandlerPipeline _pipeline;
    private readonly ILogger<DomainEventRouter> _logger;

    /// <summary>
    /// Initialises a new <see cref="DomainEventRouter"/>.
    /// </summary>
    /// <param name="serviceProvider">The DI container used to resolve handlers.</param>
    /// <param name="pipeline">The middleware pipeline applied around each dispatch.</param>
    /// <param name="logger">Logger for diagnostic output.</param>
    public DomainEventRouter(
        IServiceProvider serviceProvider,
        EventHandlerPipeline pipeline,
        ILogger<DomainEventRouter> logger
    )
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);
        ArgumentNullException.ThrowIfNull(pipeline);
        ArgumentNullException.ThrowIfNull(logger);

        _serviceProvider = serviceProvider;
        _pipeline = pipeline;
        _logger = logger;
    }

    /// <summary>
    /// Routes <paramref name="domainEvent"/> to every registered handler for its type.
    /// Each handler is invoked through the configured <see cref="EventHandlerPipeline"/>.
    /// </summary>
    /// <param name="domainEvent">The event to route.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async Task RouteAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);

        var eventType = domainEvent.GetType();
        var handlerType = typeof(IDomainEventHandler<>).MakeGenericType(eventType);
        var handlers = _serviceProvider.GetServices(handlerType).ToList();

        if (handlers.Count == 0)
        {
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.Log(
                    LogLevel.Debug,
                    "No handlers registered for domain event {EventType} ({EventId})",
                    eventType.Name,
                    domainEvent.EventId
                );
            }

            return;
        }

        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.Log(
                LogLevel.Debug,
                "Routing domain event {EventType} ({EventId}) to {HandlerCount} handler(s)",
                eventType.Name,
                domainEvent.EventId,
                handlers.Count
            );
        }

        var handleMethod = handlerType.GetMethod(nameof(IDomainEventHandler<IDomainEvent>.HandleAsync))!;

        foreach (var handler in handlers)
        {
            var localHandler = handler;
            var localCt = cancellationToken;

            await _pipeline
                .ExecuteAsync(
                    domainEvent,
                    () => (Task)handleMethod.Invoke(localHandler, [domainEvent, localCt])!,
                    cancellationToken
                )
                .ConfigureAwait(false);
        }
    }
}
