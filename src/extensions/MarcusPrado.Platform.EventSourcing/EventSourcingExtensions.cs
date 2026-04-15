namespace MarcusPrado.Platform.EventSourcing;

/// <summary>
/// Extension methods for registering event sourcing services in a dependency injection container.
/// </summary>
public static class EventSourcingExtensions
{
    /// <summary>
    /// Registers an in-memory <see cref="IEventStore"/> implementation as a singleton service.
    /// </summary>
    /// <param name="services">The service collection to add the event store to.</param>
    /// <returns>The same <see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddInMemoryEventSourcing(this IServiceCollection services)
    {
        services.AddSingleton<IEventStore, InMemoryEventStore>();
        return services;
    }
}
