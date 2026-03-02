using MarcusPrado.Platform.EfCore.Tests.Helpers;

namespace MarcusPrado.Platform.EfCore.Tests.Outbox;

public sealed class EfOutboxStoreTests : IDisposable
{
    private readonly TestDbContext _ctx;
    private readonly EfOutboxStore _store;

    public EfOutboxStoreTests()
    {
        _ctx = TestDbContext.CreateInMemory(Guid.NewGuid().ToString());
        _ctx.Database.EnsureCreated();
        _store = new EfOutboxStore(_ctx);
    }

    [Fact]
    public async Task SaveAsyncPersistsMessage()
    {
        var msg = new OutboxMessage
        {
            EventType = "OrderPlaced",
            Payload = "{}",
            Topic = "orders",
        };

        await _store.SaveAsync(msg);

        var saved = await _ctx.OutboxMessages.FirstOrDefaultAsync(m => m.Id == msg.Id);
        Assert.NotNull(saved);
        Assert.Equal("OrderPlaced", saved.EventType);
    }

    [Fact]
    public async Task GetPendingAsyncReturnsPendingMessages()
    {
        var msg = new OutboxMessage
        {
            EventType = "TestEvent",
            Payload = "{}",
            Topic = "test",
            State = OutboxState.Pending,
        };
        _ctx.OutboxMessages.Add(msg);
        await _ctx.SaveChangesAsync();

        var pending = await _store.GetPendingAsync(10);

        Assert.Contains(pending, m => m.Id == msg.Id);
    }

    [Fact(Skip = "ExecuteUpdateAsync is not supported by the InMemory EF provider")]
    public async Task MarkPublishedAsyncUpdatesState()
    {
        // ExecuteUpdateAsync is not supported by the InMemory provider;
        // we verify the method does not throw with a real (published) message in context.
        var msg = new OutboxMessage
        {
            EventType = "TestEvent",
            Payload = "{}",
            Topic = "test",
            State = OutboxState.Pending,
        };
        _ctx.OutboxMessages.Add(msg);
        await _ctx.SaveChangesAsync();

        var ex = await Record.ExceptionAsync(() => _store.MarkPublishedAsync(msg.Id));
        Assert.Null(ex);
    }

    [Fact(Skip = "ExecuteUpdateAsync is not supported by the InMemory EF provider")]
    public async Task MarkFailedAsyncUpdatesStateAndError()
    {
        // ExecuteUpdateAsync is not supported by the InMemory provider;
        // we verify the method does not throw.
        var msg = new OutboxMessage
        {
            EventType = "TestEvent",
            Payload = "{}",
            Topic = "test",
        };
        _ctx.OutboxMessages.Add(msg);
        await _ctx.SaveChangesAsync();

        var ex = await Record.ExceptionAsync(() => _store.MarkFailedAsync(msg.Id, "connection refused"));
        Assert.Null(ex);
    }

    public void Dispose() => _ctx.Dispose();
}
