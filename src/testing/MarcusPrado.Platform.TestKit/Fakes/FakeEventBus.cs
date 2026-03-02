using MarcusPrado.Platform.Abstractions.Execution;

namespace MarcusPrado.Platform.TestKit.Fakes;

/// <summary>
/// In-memory <see cref="IEventBus"/> that captures published events for assertion in tests.
/// </summary>
public sealed class FakeEventBus : IEventBus
{
    private readonly List<object> _events = new();

    /// <summary>All events published via <see cref="PublishAsync{TEvent}"/>, in order.</summary>
    public IReadOnlyList<object> PublishedEvents => _events;

    /// <summary>Returns all published events of type <typeparamref name="TEvent"/>.</summary>
    public IEnumerable<TEvent> EventsOf<TEvent>()
        where TEvent : class
        => _events.OfType<TEvent>();

    /// <summary>Returns the number of published events.</summary>
    public int Count => _events.Count;

    /// <summary>Clears the captured events list.</summary>
    public void Reset() => _events.Clear();

    /// <inheritdoc/>
    public Task PublishAsync<TEvent>(TEvent @event, CancellationToken ct = default)
        where TEvent : class
    {
        ArgumentNullException.ThrowIfNull(@event);
        _events.Add(@event);
        return Task.CompletedTask;
    }
}
