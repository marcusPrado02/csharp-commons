namespace MarcusPrado.Platform.EventSourcing;

public static class EventSourcingExtensions
{
    public static IServiceCollection AddInMemoryEventSourcing(this IServiceCollection services)
    {
        services.AddSingleton<IEventStore, InMemoryEventStore>();
        return services;
    }
}
