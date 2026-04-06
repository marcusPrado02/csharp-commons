using System.Diagnostics;
using System.Text;
using Confluent.Kafka;

namespace MarcusPrado.Platform.Kafka.Tests.Unit;

public sealed class KafkaTracePropagatorTests
{
    [Fact]
    public void Inject_NullActivity_DoesNotThrow()
    {
        var headers = new Headers();

        var act = () => KafkaTracePropagator.Inject(null, headers);

        act.Should().NotThrow();
    }

    [Fact]
    public void Inject_ActiveActivity_AddsTraceparentHeader()
    {
        using var activity = new Activity("test-op");
        activity.Start();

        var headers = new Headers();
        KafkaTracePropagator.Inject(activity, headers);

        var keys = headers.Select(h => h.Key).ToList();
        keys.Should().ContainMatch("traceparent");
    }

    [Fact]
    public void Extract_NullHeaders_ReturnsDefaultContext()
    {
        var ctx = KafkaTracePropagator.Extract(null);

        ctx.Should().Be(default(ActivityContext));
    }

    [Fact]
    public void Extract_EmptyHeaders_ReturnsDefaultContext()
    {
        var headers = new Headers();

        var ctx = KafkaTracePropagator.Extract(headers);

        ctx.Should().Be(default(ActivityContext));
    }

    [Fact]
    public void InjectExtract_RoundTrip_RestoresTraceId()
    {
        using var activity = new Activity("roundtrip");
        activity.SetIdFormat(ActivityIdFormat.W3C);
        activity.Start();

        var headers = new Headers();
        KafkaTracePropagator.Inject(activity, headers);

        var extracted = KafkaTracePropagator.Extract(headers);

        extracted.TraceId.Should().Be(activity.TraceId);
    }
}
