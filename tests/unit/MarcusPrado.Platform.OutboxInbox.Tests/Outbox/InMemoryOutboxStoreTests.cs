namespace MarcusPrado.Platform.OutboxInbox.Tests.Outbox;

public sealed class InMemoryOutboxStoreTests
{
    private readonly InMemoryOutboxStore _store = new();

    [Fact]
    public async Task SaveAndGetPending_ReturnsSavedMessage()
    {
        var msg = new OutboxMessage { State = OutboxState.Pending };
        await _store.SaveAsync(msg);

        var pending = await _store.GetPendingAsync(10);

        Assert.Contains(pending, m => m.Id == msg.Id);
    }

    [Fact]
    public async Task MarkPublished_UpdatesState()
    {
        var msg = new OutboxMessage();
        await _store.SaveAsync(msg);

        await _store.MarkPublishedAsync(msg.Id);

        Assert.Equal(OutboxState.Published, _store.Messages.First(m => m.Id == msg.Id).State);
    }

    [Fact]
    public async Task MarkFailed_UpdatesStateAndError()
    {
        var msg = new OutboxMessage();
        await _store.SaveAsync(msg);

        await _store.MarkFailedAsync(msg.Id, "timeout");

        var stored = _store.Messages.First(m => m.Id == msg.Id);
        Assert.Equal(OutboxState.Failed, stored.State);
        Assert.Equal("timeout", stored.LastError);
    }

    [Fact]
    public async Task GetPending_RespectsScheduledAt()
    {
        var future = new OutboxMessage { ScheduledAt = DateTimeOffset.UtcNow.AddHours(1) };
        var now = new OutboxMessage { ScheduledAt = DateTimeOffset.UtcNow.AddSeconds(-1) };
        await _store.SaveAsync(future);
        await _store.SaveAsync(now);

        var pending = await _store.GetPendingAsync(10);

        Assert.DoesNotContain(pending, m => m.Id == future.Id);
        Assert.Contains(pending, m => m.Id == now.Id);
    }

    [Fact]
    public async Task IncrementRetry_BumpsRetryCount()
    {
        var msg = new OutboxMessage();
        await _store.SaveAsync(msg);

        await _store.IncrementRetryAsync(msg.Id, DateTimeOffset.UtcNow.AddSeconds(5));

        Assert.Equal(1, _store.Messages.First(m => m.Id == msg.Id).RetryCount);
    }

    [Fact]
    public async Task GetPending_RespectsBatchSize()
    {
        for (var i = 0; i < 5; i++)
        {
            await _store.SaveAsync(new OutboxMessage { ScheduledAt = DateTimeOffset.UtcNow.AddSeconds(-1) });
        }

        var pending = await _store.GetPendingAsync(3);

        Assert.Equal(3, pending.Count);
    }
}
