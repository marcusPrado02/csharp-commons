namespace MarcusPrado.Platform.OutboxInbox.Tests.Outbox;

public sealed class OutboxMessageTests
{
    [Fact]
    public void NewMessage_HasPendingState()
    {
        var msg = new OutboxMessage();
        Assert.Equal(OutboxState.Pending, msg.State);
    }

    [Fact]
    public void NewMessage_HasNonEmptyId()
    {
        var msg = new OutboxMessage();
        Assert.NotEqual(Guid.Empty, msg.Id);
    }

    [Fact]
    public void TwoMessages_HaveDifferentIds()
    {
        var a = new OutboxMessage();
        var b = new OutboxMessage();
        Assert.NotEqual(a.Id, b.Id);
    }
}
