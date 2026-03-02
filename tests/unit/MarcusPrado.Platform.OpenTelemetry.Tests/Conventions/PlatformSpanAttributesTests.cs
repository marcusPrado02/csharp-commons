namespace MarcusPrado.Platform.OpenTelemetry.Tests.Conventions;

public sealed class PlatformSpanAttributesTests
{
    [Fact]
    public void TenantId_HasCorrectValue()
    {
        Assert.Equal("tenant.id", PlatformSpanAttributes.TenantId);
    }

    [Fact]
    public void CorrelationId_HasCorrectValue()
    {
        Assert.Equal("correlation.id", PlatformSpanAttributes.CorrelationId);
    }

    [Fact]
    public void UserId_HasCorrectValue()
    {
        Assert.Equal("user.id", PlatformSpanAttributes.UserId);
    }

    [Fact]
    public void CommandName_HasCorrectValue()
    {
        Assert.Equal("command.name", PlatformSpanAttributes.CommandName);
    }

    [Fact]
    public void EventName_HasCorrectValue()
    {
        Assert.Equal("event.name", PlatformSpanAttributes.EventName);
    }
}
