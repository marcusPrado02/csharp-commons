namespace MarcusPrado.Platform.EventSourcing.Projections;

/// <summary>
/// Dispatches domain events to registered projections, updating the corresponding read models.
/// </summary>
public sealed class ProjectionEngine
{
    // Key: event type, Value: list of handlers (async delegates)
    private readonly Dictionary<Type, List<Func<IDomainEvent, CancellationToken, Task>>> _handlers = new();

    /// <summary>Registers a projection for a specific event + read model combination.</summary>
    public void Register<TEvent, TReadModel>(
        IProjection<TEvent, TReadModel> projection,
        IReadModelStore<TReadModel> store,
        Func<TEvent, string> idSelector)
        where TEvent : IDomainEvent
        where TReadModel : class, new()
    {
        var eventType = typeof(TEvent);
        if (!_handlers.ContainsKey(eventType))
            _handlers[eventType] = [];

        _handlers[eventType].Add(async (evt, ct) =>
        {
            var typed = (TEvent)evt;
            var id = idSelector(typed);
            var readModel = await store.GetAsync(id, ct) ?? new TReadModel();
            await projection.ApplyAsync(typed, readModel, ct);
            await store.SaveAsync(id, readModel, ct);
        });
    }

    /// <summary>Dispatches an event to all registered projections.</summary>
    public async Task DispatchAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        var eventType = domainEvent.GetType();
        if (!_handlers.TryGetValue(eventType, out var handlers)) return;

        foreach (var handler in handlers)
            await handler(domainEvent, cancellationToken);
    }
}
