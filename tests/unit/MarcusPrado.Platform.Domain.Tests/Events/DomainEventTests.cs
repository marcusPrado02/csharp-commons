using MarcusPrado.Platform.Domain.Events;

namespace MarcusPrado.Platform.Domain.Tests.Events;

// ── IDomainEvent / DomainEvent tests ──────────────────────────────────────────

public sealed class DomainEventTests
{
    // ── Nested test doubles ───────────────────────────────────────────────────

    private sealed record ItemShipped(Guid OrderId, string TrackingNumber) : DomainEvent;

    private sealed record OrderCancelled(Guid OrderId, string Reason) : DomainEvent
    {
        public override string EventType => "order.cancelled";
    }
    [Fact]
    public void DomainEvent_AutoPopulates_EventId_OccurredOn()
    {
        var before = DateTimeOffset.UtcNow;
        var evt    = new ItemShipped(Guid.NewGuid(), "TRACK-001");
        var after  = DateTimeOffset.UtcNow;

        evt.EventId.Should().NotBe(Guid.Empty);
        evt.OccurredOn.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
    }

    [Fact]
    public void TwoEvents_HaveDifferentEventIds()
    {
        var a = new ItemShipped(Guid.NewGuid(), "T1");
        var b = new ItemShipped(Guid.NewGuid(), "T2");

        a.EventId.Should().NotBe(b.EventId);
    }

    [Fact]
    public void EventType_DefaultsToTypeName()
    {
        new ItemShipped(Guid.NewGuid(), "T").EventType.Should().Be(nameof(ItemShipped));
    }

    [Fact]
    public void EventType_CanBeOverridden_WithStableName()
    {
        new OrderCancelled(Guid.NewGuid(), "Customer requested").EventType
            .Should().Be("order.cancelled");
    }

    // ── DomainEventEnvelope ───────────────────────────────────────────────────

    [Fact]
    public void Envelope_Create_PopulatesAllFields()
    {
        var evt      = new ItemShipped(Guid.NewGuid(), "TRACK-001");
        var envelope = DomainEventEnvelope.Create(evt, Guid.NewGuid(), "Order", 3);

        envelope.Event.Should().Be(evt);
        envelope.AggregateType.Should().Be("Order");
        envelope.AggregateVersion.Should().Be(3);
        envelope.AggregateId.Should().NotBeNullOrWhiteSpace();
    }

    // ── IDomainEventRecorder contract ─────────────────────────────────────────

    [Fact]
    public void IDomainEventPublisher_CanBeImplemented()
    {
        // verify the interface is well-formed — compile-time check
        IDomainEventPublisher publisher = new NullPublisher();
        publisher.Should().NotBeNull();
    }

    private sealed class NullPublisher : IDomainEventPublisher
    {
        public Task PublishAsync<T>(T domainEvent, CancellationToken ct = default)
            where T : IDomainEvent => Task.CompletedTask;

        public Task PublishAllAsync(IEnumerable<IDomainEvent> events, CancellationToken ct = default)
            => Task.CompletedTask;
    }
}
