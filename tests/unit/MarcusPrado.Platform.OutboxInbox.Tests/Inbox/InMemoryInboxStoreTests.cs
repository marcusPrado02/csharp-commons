namespace MarcusPrado.Platform.OutboxInbox.Tests.Inbox;

public sealed class InMemoryInboxStoreTests
{
    private readonly InMemoryInboxStore _store = new();

    [Fact]
    public async Task SaveAndGetPending_ReturnsSavedMessage()
    {
        var msg = new InboxMessage { State = InboxState.Pending };
        await _store.SaveAsync(msg);

        var pending = await _store.GetPendingAsync(10);

        Assert.Contains(pending, m => m.Id == msg.Id);
    }

    [Fact]
    public async Task MarkProcessed_UpdatesState()
    {
        var msg = new InboxMessage();
        await _store.SaveAsync(msg);

        await _store.MarkProcessedAsync(msg.Id);

        Assert.Equal(InboxState.Processed, _store.Messages.First(m => m.Id == msg.Id).State);
    }

    [Fact]
    public async Task MarkDuplicate_UpdatesState()
    {
        var msg = new InboxMessage();
        await _store.SaveAsync(msg);

        await _store.MarkDuplicateAsync(msg.Id);

        Assert.Equal(InboxState.Duplicate, _store.Messages.First(m => m.Id == msg.Id).State);
    }

    [Fact]
    public async Task MarkFailed_UpdatesStateAndError()
    {
        var msg = new InboxMessage();
        await _store.SaveAsync(msg);

        await _store.MarkFailedAsync(msg.Id, "boom");

        var stored = _store.Messages.First(m => m.Id == msg.Id);
        Assert.Equal(InboxState.Failed, stored.State);
        Assert.Equal("boom", stored.LastError);
    }
}
