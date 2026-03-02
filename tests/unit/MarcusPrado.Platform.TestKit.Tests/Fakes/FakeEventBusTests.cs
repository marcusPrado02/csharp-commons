namespace MarcusPrado.Platform.TestKit.Tests.Fakes;

public sealed class FakeEventBusTests
{
    private sealed record OrderPlaced(Guid OrderId);
    private sealed record PaymentReceived(decimal Amount);

    [Fact]
    public async Task PublishAsync_CapturesEvent()
    {
        var bus = new FakeEventBus();
        var evt = new OrderPlaced(Guid.NewGuid());
        await bus.PublishAsync(evt);
        Assert.Single(bus.PublishedEvents);
        Assert.Contains(evt, bus.PublishedEvents);
    }

    [Fact]
    public async Task EventsOf_FiltersCorrectType()
    {
        var bus = new FakeEventBus();
        await bus.PublishAsync(new OrderPlaced(Guid.NewGuid()));
        await bus.PublishAsync(new PaymentReceived(99.99m));
        await bus.PublishAsync(new OrderPlaced(Guid.NewGuid()));

        var orders = bus.EventsOf<OrderPlaced>().ToList();
        Assert.Equal(2, orders.Count);
    }

    [Fact]
    public async Task Reset_ClearsEvents()
    {
        var bus = new FakeEventBus();
        await bus.PublishAsync(new OrderPlaced(Guid.NewGuid()));
        bus.Reset();
        Assert.Empty(bus.PublishedEvents);
        Assert.Equal(0, bus.Count);
    }

    [Fact]
    public async Task PublishAsync_NullEvent_Throws()
    {
        var bus = new FakeEventBus();
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => bus.PublishAsync<OrderPlaced>(null!));
    }
}
