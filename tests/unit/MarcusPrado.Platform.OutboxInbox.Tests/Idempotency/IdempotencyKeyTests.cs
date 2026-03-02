namespace MarcusPrado.Platform.OutboxInbox.Tests.Idempotency;

public sealed class IdempotencyKeyTests
{
    [Fact]
    public void FromMessageId_PrependsPrefix()
    {
        var key = IdempotencyKey.FromMessageId("abc-123");
        Assert.Equal("msg:abc-123", key.Value);
    }

    [Fact]
    public void FromOperation_CombinesNameAndId()
    {
        var key = IdempotencyKey.FromOperation("payment", "42");
        Assert.Equal("payment:42", key.Value);
    }

    [Fact]
    public void EmptyValue_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new IdempotencyKey(string.Empty));
    }

    [Fact]
    public void ToString_ReturnsValue()
    {
        var key = new IdempotencyKey("x");
        Assert.Equal("x", key.ToString());
    }
}
