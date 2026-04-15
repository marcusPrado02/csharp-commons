namespace MarcusPrado.Platform.EfCore.Tests.Outbox;

public sealed class EfOutboxStoreAdditionalTests : IDisposable
{
    private readonly TestDbContext _ctx;
    private readonly EfOutboxStore _store;

    public EfOutboxStoreAdditionalTests()
    {
        _ctx = TestDbContext.CreateInMemory(Guid.NewGuid().ToString());
        _ctx.Database.EnsureCreated();
        _store = new EfOutboxStore(_ctx);
    }

    [Fact]
    public async Task GetPendingAsync_ReturnsEmpty_WhenNoMessagesExist()
    {
        var pending = await _store.GetPendingAsync(100);

        Assert.Empty(pending);
    }

    [Fact]
    public async Task GetPendingAsync_DoesNotReturn_PublishedMessages()
    {
        var msg = new OutboxMessage
        {
            EventType = "published.event",
            Payload = "{}",
            Topic = "t",
            State = OutboxState.Published,
        };
        _ctx.OutboxMessages.Add(msg);
        await _ctx.SaveChangesAsync();

        var pending = await _store.GetPendingAsync(100);

        Assert.DoesNotContain(pending, m => m.Id == msg.Id);
    }

    [Fact]
    public async Task GetPendingAsync_DoesNotReturn_FailedMessages()
    {
        var msg = new OutboxMessage
        {
            EventType = "failed.event",
            Payload = "{}",
            Topic = "t",
            State = OutboxState.Failed,
        };
        _ctx.OutboxMessages.Add(msg);
        await _ctx.SaveChangesAsync();

        var pending = await _store.GetPendingAsync(100);

        Assert.DoesNotContain(pending, m => m.Id == msg.Id);
    }

    [Fact]
    public async Task SaveAsync_Persists_ScheduledAtAndCreatedAt()
    {
        var before = DateTimeOffset.UtcNow.AddSeconds(-1);
        var msg = new OutboxMessage
        {
            EventType = "scheduled.event",
            Payload = "{}",
            Topic = "t",
        };

        await _store.SaveAsync(msg);
        var after = DateTimeOffset.UtcNow.AddSeconds(1);

        var saved = await _ctx.OutboxMessages.FirstAsync(m => m.Id == msg.Id);
        Assert.InRange(saved.CreatedAt, before, after);
        Assert.InRange(saved.ScheduledAt, before, after);
    }

    [Fact]
    public async Task SaveAsync_DefaultStateIsPending()
    {
        var msg = new OutboxMessage
        {
            EventType = "e",
            Payload = "{}",
            Topic = "t",
        };
        await _store.SaveAsync(msg);

        var saved = await _ctx.OutboxMessages.FirstAsync(m => m.Id == msg.Id);
        Assert.Equal(OutboxState.Pending, saved.State);
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenContextIsNull()
    {
        var ex = Assert.Throws<ArgumentNullException>(() => new EfOutboxStore(null!));
        Assert.Equal("context", ex.ParamName);
    }

    public void Dispose() => _ctx.Dispose();
}
