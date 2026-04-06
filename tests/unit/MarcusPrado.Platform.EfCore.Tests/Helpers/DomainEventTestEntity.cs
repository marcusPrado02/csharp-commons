using MarcusPrado.Platform.Domain.Events;

namespace MarcusPrado.Platform.EfCore.Tests.Helpers;

/// <summary>
/// A simple domain event raised by <see cref="DomainEventTestEntity"/>.
/// </summary>
internal sealed record OrderCreatedEvent : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
    public string EventType => "order.created";
}

/// <summary>
/// Minimal entity that accumulates domain events so
/// <see cref="AppDbContextBase"/> can harvest and dispatch them.
/// </summary>
internal sealed class DomainEventTestEntity : IHasDomainEvents
{
    private readonly List<IDomainEvent> _events = [];

    public Guid Id { get; set; } = Guid.NewGuid();

    public string Name { get; set; } = string.Empty;

    /// <inheritdoc/>
    public IReadOnlyList<IDomainEvent> DomainEvents => _events.AsReadOnly();

    /// <inheritdoc/>
    public void ClearDomainEvents() => _events.Clear();

    /// <summary>Records an <see cref="OrderCreatedEvent"/> on this entity.</summary>
    public void RaiseOrderCreated() => _events.Add(new OrderCreatedEvent());
}
