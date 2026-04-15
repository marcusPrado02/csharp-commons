using MarcusPrado.Platform.SignalR.Publishers;

namespace MarcusPrado.Platform.SignalR.Events;

/// <summary>
/// Translates domain events into SignalR broadcasts.
/// Topic name is derived from the event type name (snake_case).
/// </summary>
public sealed class SignalRDomainEventSink : IDomainEventSink
{
    private readonly IRealtimePublisher _publisher;

    /// <summary>Initializes a new instance of <see cref="SignalRDomainEventSink"/>.</summary>
    public SignalRDomainEventSink(IRealtimePublisher publisher) => _publisher = publisher;

    /// <inheritdoc />
    public Task HandleAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        var topic = ToSnakeCase(domainEvent.GetType().Name);
        return _publisher.PublishAsync(topic, domainEvent, cancellationToken);
    }

    public static string ToSnakeCase(string name)
    {
        // "OrderPlaced" → "order_placed"
        return string.Concat(
            name.Select(
                (c, i) =>
                    i > 0 && char.IsUpper(c) ? $"_{char.ToLowerInvariant(c)}" : char.ToLowerInvariant(c).ToString()
            )
        );
    }
}
