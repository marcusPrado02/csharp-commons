using MarcusPrado.Platform.EventRouting.Bridge;
using MarcusPrado.Platform.EventRouting.Handling;
using MarcusPrado.Platform.EventRouting.Pipeline;
using MarcusPrado.Platform.EventRouting.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace MarcusPrado.Platform.EventRouting.Extensions;

/// <summary>Extension methods for registering domain event routing services.</summary>
public static class EventRoutingExtensions
{
    /// <summary>
    /// Registers the core domain event routing infrastructure:
    /// <see cref="EventHandlerPipeline"/>, <see cref="DomainEventRouter"/>,
    /// <see cref="DomainEventDispatcher"/> (as <see cref="IDomainEventDispatcher"/>),
    /// and <see cref="CrossBoundaryEventBridge"/>.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="configurePipeline">
    /// Optional delegate to configure the <see cref="EventHandlerPipeline"/> with middlewares.
    /// </param>
    /// <returns>The <paramref name="services"/> for fluent chaining.</returns>
    public static IServiceCollection AddDomainEventRouting(
        this IServiceCollection services,
        Action<EventHandlerPipeline>? configurePipeline = null
    )
    {
        ArgumentNullException.ThrowIfNull(services);

        var pipeline = new EventHandlerPipeline();
        configurePipeline?.Invoke(pipeline);

        services.AddSingleton(pipeline);
        services.AddScoped<DomainEventRouter>();
        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
        services.AddSingleton<CrossBoundaryEventBridge>();

        return services;
    }

    /// <summary>
    /// Registers a concrete <see cref="IDomainEventHandler{TEvent}"/> implementation.
    /// </summary>
    /// <typeparam name="TEvent">The domain event type.</typeparam>
    /// <typeparam name="THandler">The handler implementation.</typeparam>
    /// <param name="services">The service collection to configure.</param>
    /// <returns>The <paramref name="services"/> for fluent chaining.</returns>
    public static IServiceCollection AddDomainEventHandler<TEvent, THandler>(this IServiceCollection services)
        where THandler : class, IDomainEventHandler<TEvent>
        where TEvent : MarcusPrado.Platform.Domain.Events.IDomainEvent
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddScoped<IDomainEventHandler<TEvent>, THandler>();
        return services;
    }
}
