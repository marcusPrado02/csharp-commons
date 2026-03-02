using MarcusPrado.Platform.Observability.Metrics;
using MarcusPrado.Platform.OpenTelemetry.Metrics;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MarcusPrado.Platform.OpenTelemetry.Tests;

public sealed class BusinessMetricsTests
{
    [Fact]
    public void AddPlatformBusinessMetrics_RegistersIBusinessMetrics()
    {
        var sp = new ServiceCollection()
            .AddPlatformBusinessMetrics()
            .BuildServiceProvider();

        sp.GetService<IBusinessMetrics>().Should().BeOfType<OtelBusinessMetrics>();
    }

    [Fact]
    public void RecordOrderPlaced_DoesNotThrow()
    {
        var m = new OtelBusinessMetrics();
        var act = () => m.RecordOrderPlaced("t1", 99.99m, "BRL");
        act.Should().NotThrow();
    }

    [Fact]
    public void RecordPaymentProcessed_DoesNotThrow()
    {
        var m   = new OtelBusinessMetrics();
        var act = () => m.RecordPaymentProcessed("t1", "succeeded", "stripe");
        act.Should().NotThrow();
    }

    [Fact]
    public void RecordUserSignup_DoesNotThrow()
    {
        var m   = new OtelBusinessMetrics();
        var act = () => m.RecordUserSignup("t1", "pro");
        act.Should().NotThrow();
    }

    [Fact]
    public void RecordEventConsumed_DoesNotThrow()
    {
        var m   = new OtelBusinessMetrics();
        var act = () => m.RecordEventConsumed("orders", "billing", 42);
        act.Should().NotThrow();
    }
}
