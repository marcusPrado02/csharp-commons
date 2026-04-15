namespace MarcusPrado.Platform.EfCore.Tests.Inbox;

public sealed class EfInboxStoreAdditionalTests : IDisposable
{
    private readonly TestDbContext _ctx;
    private readonly EfInboxStore _store;

    public EfInboxStoreAdditionalTests()
    {
        _ctx = TestDbContext.CreateInMemory(Guid.NewGuid().ToString());
        _ctx.Database.EnsureCreated();
        _store = new EfInboxStore(_ctx);
    }

    [Fact]
    public async Task GetPendingAsync_ReturnsEmpty_WhenNoMessagesExist()
    {
        var pending = await _store.GetPendingAsync(100);

        Assert.Empty(pending);
    }

    [Fact]
    public async Task GetPendingAsync_DoesNotReturn_ProcessedMessages()
    {
        var msg = new InboxMessage
        {
            MessageId = Guid.NewGuid().ToString(),
            EventType = "processed.event",
            Payload = "{}",
            State = InboxState.Processed,
        };
        _ctx.InboxMessages.Add(msg);
        await _ctx.SaveChangesAsync();

        var pending = await _store.GetPendingAsync(100);

        Assert.DoesNotContain(pending, m => m.Id == msg.Id);
    }

    [Fact]
    public async Task SaveAsync_DefaultStateIsPending()
    {
        var msg = new InboxMessage
        {
            MessageId = Guid.NewGuid().ToString(),
            EventType = "e",
            Payload = "{}",
        };
        await _store.SaveAsync(msg);

        var saved = await _ctx.InboxMessages.FirstAsync(m => m.Id == msg.Id);
        Assert.Equal(InboxState.Pending, saved.State);
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenContextIsNull()
    {
        var ex = Assert.Throws<ArgumentNullException>(() => new EfInboxStore(null!));
        Assert.Equal("context", ex.ParamName);
    }

    public void Dispose() => _ctx.Dispose();
}
