using MarcusPrado.Platform.EfCore.Tests.Helpers;

namespace MarcusPrado.Platform.EfCore.Tests.Inbox;

public sealed class EfInboxStoreTests : IDisposable
{
    private readonly TestDbContext _ctx;
    private readonly EfInboxStore _store;

    public EfInboxStoreTests()
    {
        _ctx = TestDbContext.CreateInMemory(Guid.NewGuid().ToString());
        _ctx.Database.EnsureCreated();
        _store = new EfInboxStore(_ctx);
    }

    [Fact]
    public async Task SaveAsyncPersistsMessage()
    {
        var msg = new InboxMessage
        {
            MessageId = Guid.NewGuid().ToString(),
            EventType = "OrderPlaced",
            Payload = "{}",
        };

        await _store.SaveAsync(msg);

        var saved = await _ctx.InboxMessages.FirstOrDefaultAsync(m => m.Id == msg.Id);
        Assert.NotNull(saved);
        Assert.Equal("OrderPlaced", saved.EventType);
    }

    [Fact]
    public async Task GetPendingAsyncReturnsPendingMessages()
    {
        var msg = new InboxMessage
        {
            MessageId = Guid.NewGuid().ToString(),
            EventType = "TestEvent",
            Payload = "{}",
            State = InboxState.Pending,
        };
        _ctx.InboxMessages.Add(msg);
        await _ctx.SaveChangesAsync();

        var pending = await _store.GetPendingAsync(10);

        Assert.Contains(pending, m => m.Id == msg.Id);
    }

    [Fact(Skip = "ExecuteUpdateAsync is not supported by the InMemory EF provider")]
    public async Task MarkProcessedAsyncUpdatesState()
    {
        // ExecuteUpdateAsync is not supported by the InMemory provider;
        // we verify the method does not throw.
        var msg = new InboxMessage
        {
            MessageId = Guid.NewGuid().ToString(),
            EventType = "TestEvent",
            Payload = "{}",
            State = InboxState.Pending,
        };
        _ctx.InboxMessages.Add(msg);
        await _ctx.SaveChangesAsync();

        var ex = await Record.ExceptionAsync(() => _store.MarkProcessedAsync(msg.Id));
        Assert.Null(ex);
    }

    public void Dispose() => _ctx.Dispose();
}
