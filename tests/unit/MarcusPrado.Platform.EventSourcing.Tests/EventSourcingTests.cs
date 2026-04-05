using FluentAssertions;
using MarcusPrado.Platform.Domain.Events;
using MarcusPrado.Platform.EventSourcing;
using Xunit;

namespace MarcusPrado.Platform.EventSourcing.Tests;

// ── Test domain model ────────────────────────────────────────────────────────

internal sealed class OrderCreatedEvent : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();

    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;

    public string EventType => nameof(OrderCreatedEvent);

    public string OrderId { get; init; } = string.Empty;
}

internal sealed class ItemAddedEvent : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();

    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;

    public string EventType => nameof(ItemAddedEvent);
}

internal sealed class OrderState
{
    public string OrderId { get; set; } = string.Empty;

    public int ItemCount { get; set; }

    public void Apply(OrderCreatedEvent evt) => OrderId = evt.OrderId;

    public void Apply(ItemAddedEvent evt) => ItemCount++;
}

// ── Tests ────────────────────────────────────────────────────────────────────

public sealed class InMemoryEventStoreTests
{
    [Fact]
    public async Task AppendAndLoad_ReturnsEventsInOrder()
    {
        var store = new InMemoryEventStore();
        var events = new IDomainEvent[]
        {
            new OrderCreatedEvent { OrderId = "order-1" },
            new ItemAddedEvent(),
        };

        await store.AppendAsync("stream-1", events, expectedVersion: -1);
        var loaded = await store.LoadAsync("stream-1");

        loaded.Should().HaveCount(2);
        loaded[0].SequenceNumber.Should().Be(0);
        loaded[1].SequenceNumber.Should().Be(1);
    }

    [Fact]
    public async Task GetVersion_UnknownStream_ReturnsMinusOne()
    {
        var store = new InMemoryEventStore();
        var version = await store.GetVersionAsync("nonexistent");
        version.Should().Be(-1L);
    }

    [Fact]
    public async Task Version_IncrementsAfterEachAppend()
    {
        var store = new InMemoryEventStore();

        await store.AppendAsync("s", new[] { new ItemAddedEvent() }, expectedVersion: -1);
        var v1 = await store.GetVersionAsync("s");

        await store.AppendAsync("s", new[] { new ItemAddedEvent() }, expectedVersion: 0);
        var v2 = await store.GetVersionAsync("s");

        v1.Should().Be(0L);
        v2.Should().Be(1L);
    }

    [Fact]
    public async Task AppendAsync_WrongExpectedVersion_ThrowsOptimisticConcurrencyException()
    {
        var store = new InMemoryEventStore();
        await store.AppendAsync("s", new[] { new ItemAddedEvent() }, expectedVersion: -1);

        var act = () => store.AppendAsync("s", new[] { new ItemAddedEvent() }, expectedVersion: 99);

        await act.Should().ThrowAsync<OptimisticConcurrencyException>()
            .Where(ex => ex.StreamId == "s" && ex.ExpectedVersion == 99 && ex.ActualVersion == 0);
    }

    [Fact]
    public async Task LoadAsync_WithFromSequence_FiltersCorrectly()
    {
        var store = new InMemoryEventStore();
        await store.AppendAsync("s", new IDomainEvent[]
        {
            new ItemAddedEvent(),
            new ItemAddedEvent(),
            new ItemAddedEvent(),
        }, expectedVersion: -1);

        var loaded = await store.LoadAsync("s", fromSequence: 1);

        loaded.Should().HaveCount(2);
        loaded[0].SequenceNumber.Should().Be(1);
        loaded[1].SequenceNumber.Should().Be(2);
    }

    [Fact]
    public async Task LoadAsync_EmptyStream_ReturnsEmpty()
    {
        var store = new InMemoryEventStore();
        var result = await store.LoadAsync("no-stream");
        result.Should().BeEmpty();
    }
}

public sealed class AggregateEventReplayerTests
{
    [Fact]
    public async Task Replay_AppliesEventsInOrder()
    {
        var store = new InMemoryEventStore();
        await store.AppendAsync("order-42", new IDomainEvent[]
        {
            new OrderCreatedEvent { OrderId = "order-42" },
            new ItemAddedEvent(),
            new ItemAddedEvent(),
        }, expectedVersion: -1);

        var storedEvents = await store.LoadAsync("order-42");
        var state = AggregateEventReplayer.Replay(new OrderState(), storedEvents);

        state.OrderId.Should().Be("order-42");
        state.ItemCount.Should().Be(2);
    }

    [Fact]
    public async Task Replay_UnknownEventType_IsSkippedGracefully()
    {
        // Manually build a stored event with a bogus type name.
        var bogus = new StoredEvent(
            EventId: Guid.NewGuid(),
            StreamId: "s",
            SequenceNumber: 0,
            EventType: "This.Does.Not.Exist, SomeAssembly",
            Payload: "{}",
            OccurredOn: DateTimeOffset.UtcNow);

        var state = new OrderState();
        var act = () => AggregateEventReplayer.Replay(state, new[] { bogus });

        act.Should().NotThrow();
        state.OrderId.Should().BeEmpty();
    }
}

public sealed class InMemorySnapshotStoreTests
{
    [Fact]
    public async Task SaveAndLoadLatest_ReturnsSnapshot()
    {
        var store = new InMemorySnapshotStore<OrderState>();
        var snapshot = new EventSnapshot<OrderState>(
            StreamId: "order-1",
            SequenceNumber: 5,
            State: new OrderState { OrderId = "order-1", ItemCount = 3 },
            CreatedAt: DateTimeOffset.UtcNow);

        await store.SaveAsync(snapshot);
        var loaded = await store.LoadLatestAsync("order-1");

        loaded.Should().NotBeNull();
        loaded!.SequenceNumber.Should().Be(5);
        loaded.State.OrderId.Should().Be("order-1");
        loaded.State.ItemCount.Should().Be(3);
    }

    [Fact]
    public async Task LoadLatest_UnknownStream_ReturnsNull()
    {
        var store = new InMemorySnapshotStore<OrderState>();
        var result = await store.LoadLatestAsync("no-such-stream");
        result.Should().BeNull();
    }
}

public sealed class EventSourcedRepositoryTests
{
    [Fact]
    public async Task LoadAsync_ReturnsStateWithEventsApplied()
    {
        var eventStore = new InMemoryEventStore();
        var snapshotStore = new InMemorySnapshotStore<OrderState>();
        var repo = new EventSourcedRepository<OrderState>(eventStore, snapshotStore);

        await eventStore.AppendAsync("order-99", new IDomainEvent[]
        {
            new OrderCreatedEvent { OrderId = "order-99" },
            new ItemAddedEvent(),
        }, expectedVersion: -1);

        var (state, version) = await repo.LoadAsync("order-99");

        state.OrderId.Should().Be("order-99");
        state.ItemCount.Should().Be(1);
        version.Should().Be(1L);
    }

    [Fact]
    public async Task SaveAsync_CreatesSnapshotAfterSnapshotEveryEvents()
    {
        var eventStore = new InMemoryEventStore();
        var snapshotStore = new InMemorySnapshotStore<OrderState>();
        // Use snapshotEvery=3 so snapshot is created after 3 events (seq 0,1,2 -> version=2, (2+1)%3==0)
        var repo = new EventSourcedRepository<OrderState>(eventStore, snapshotStore, snapshotEvery: 3);

        // Append first event to get version 0
        await repo.SaveAsync("order-snap",
            new IDomainEvent[] { new OrderCreatedEvent { OrderId = "order-snap" } },
            expectedVersion: -1);

        // No snapshot yet (version=0, (0+1)%3 != 0)
        var noSnap = await snapshotStore.LoadLatestAsync("order-snap");
        noSnap.Should().BeNull();

        // Append second event -> version=1, (1+1)%3 != 0
        await repo.SaveAsync("order-snap",
            new IDomainEvent[] { new ItemAddedEvent() },
            expectedVersion: 0);

        // Append third event -> version=2, (2+1)%3==0 => snapshot created
        await repo.SaveAsync("order-snap",
            new IDomainEvent[] { new ItemAddedEvent() },
            expectedVersion: 1);

        var snap = await snapshotStore.LoadLatestAsync("order-snap");
        snap.Should().NotBeNull();
        snap!.SequenceNumber.Should().Be(2L);
        snap.State.OrderId.Should().Be("order-snap");
        snap.State.ItemCount.Should().Be(2);
    }
}
