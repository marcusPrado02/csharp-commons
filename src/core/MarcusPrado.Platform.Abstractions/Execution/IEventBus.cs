namespace MarcusPrado.Platform.Abstractions.Execution;

/// <summary>Publishes domain or integration events to interested subscribers.</summary>
public interface IEventBus
{
    /// <summary>Publishes <paramref name="event"/> to all registered handlers.</summary>
    Task PublishAsync<TEvent>(TEvent @event, CancellationToken ct = default)
        where TEvent : class;
}
