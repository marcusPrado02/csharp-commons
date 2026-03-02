namespace MarcusPrado.Platform.OpenTelemetry.Tests.Conventions;

public sealed class PlatformMetricNamesTests
{
    [Fact]
    public void CommandDurationMs_HasCorrectValue()
    {
        Assert.Equal("platform.command.duration_ms", PlatformMetricNames.CommandDurationMs);
    }

    [Fact]
    public void EventsPublished_HasCorrectValue()
    {
        Assert.Equal("platform.events.published", PlatformMetricNames.EventsPublished);
    }

    [Fact]
    public void EventsConsumed_HasCorrectValue()
    {
        Assert.Equal("platform.events.consumed", PlatformMetricNames.EventsConsumed);
    }

    [Fact]
    public void DlqMessages_HasCorrectValue()
    {
        Assert.Equal("platform.dlq.messages", PlatformMetricNames.DlqMessages);
    }

    [Fact]
    public void HttpRequestDurationMs_HasCorrectValue()
    {
        Assert.Equal("platform.http.request_duration_ms", PlatformMetricNames.HttpRequestDurationMs);
    }
}
