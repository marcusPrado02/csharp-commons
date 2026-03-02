namespace MarcusPrado.Platform.OutboxInbox.Tests.Inbox;

public sealed class InboxMessageTests
{
    [Fact]
    public void NewMessage_HasPendingState()
    {
        var msg = new InboxMessage();
        Assert.Equal(InboxState.Pending, msg.State);
    }

    [Fact]
    public void TwoMessages_HaveDifferentIds()
    {
        var a = new InboxMessage();
        var b = new InboxMessage();
        Assert.NotEqual(a.Id, b.Id);
    }
}
