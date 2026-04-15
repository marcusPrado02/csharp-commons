using System.Diagnostics.Metrics;
using FluentAssertions;
using MarcusPrado.Platform.Observability.Metrics;
using MarcusPrado.Platform.OpenTelemetry.Metrics;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MarcusPrado.Platform.OpenTelemetry.Tests;

/// <summary>
/// Test double that tracks all calls made to <see cref="IBusinessMetrics"/> methods.
/// </summary>
internal sealed class TrackingBusinessMetrics : IBusinessMetrics
{
    public int OrdersPlacedCount { get; private set; }
    public decimal LastOrderAmount { get; private set; }
    public string? LastOrderCurrency { get; private set; }

    public int PaymentsProcessedCount { get; private set; }
    public decimal LastPaymentAmount { get; private set; }
    public string? LastPaymentCurrency { get; private set; }
    public bool LastPaymentSuccess { get; private set; }

    public int UserSignupsCount { get; private set; }
    public string? LastSignupPlan { get; private set; }

    public int EventsConsumedCount { get; private set; }
    public string? LastEventType { get; private set; }
    public string? LastEventSource { get; private set; }

    public void RecordOrderPlaced(decimal amount, string currency)
    {
        OrdersPlacedCount++;
        LastOrderAmount = amount;
        LastOrderCurrency = currency;
    }

    public void RecordPaymentProcessed(decimal amount, string currency, bool success)
    {
        PaymentsProcessedCount++;
        LastPaymentAmount = amount;
        LastPaymentCurrency = currency;
        LastPaymentSuccess = success;
    }

    public void RecordUserSignup(string plan)
    {
        UserSignupsCount++;
        LastSignupPlan = plan;
    }

    public void RecordEventConsumed(string eventType, string source)
    {
        EventsConsumedCount++;
        LastEventType = eventType;
        LastEventSource = source;
    }
}

public sealed class BusinessMetricsTests
{
    // ── DI registration ───────────────────────────────────────────────────────

    [Fact]
    public void AddPlatformBusinessMetrics_RegistersIBusinessMetrics()
    {
        var sp = new ServiceCollection()
            .AddPlatformBusinessMetrics()
            .BuildServiceProvider();

        sp.GetService<IBusinessMetrics>().Should().BeOfType<OtelBusinessMetrics>();
    }

    // ── OtelBusinessMetrics smoke tests (constructor must not throw) ──────────

    [Fact]
    public void OtelBusinessMetrics_DefaultCtor_DoesNotThrow()
    {
        var act = () => new OtelBusinessMetrics();
        act.Should().NotThrow();
    }

    [Fact]
    public void OtelBusinessMetrics_WithMeter_DoesNotThrow()
    {
        using var meter = new Meter("TestMeter.Business", "1.0");
        var act = () => new OtelBusinessMetrics(meter);
        act.Should().NotThrow();
    }

    [Fact]
    public void RecordOrderPlaced_DoesNotThrow()
    {
        var m = new OtelBusinessMetrics();
        var act = () => m.RecordOrderPlaced(99.99m, "BRL");
        act.Should().NotThrow();
    }

    [Fact]
    public void RecordPaymentProcessed_DoesNotThrow()
    {
        var m = new OtelBusinessMetrics();
        var act = () => m.RecordPaymentProcessed(49.50m, "USD", true);
        act.Should().NotThrow();
    }

    [Fact]
    public void RecordUserSignup_DoesNotThrow()
    {
        var m = new OtelBusinessMetrics();
        var act = () => m.RecordUserSignup("pro");
        act.Should().NotThrow();
    }

    [Fact]
    public void RecordEventConsumed_DoesNotThrow()
    {
        var m = new OtelBusinessMetrics();
        var act = () => m.RecordEventConsumed("OrderCreated", "orders-service");
        act.Should().NotThrow();
    }

    // ── TrackingBusinessMetrics tests — verify call counts and arguments ──────

    [Fact]
    public void TrackingMetrics_RecordOrderPlaced_IncrementsCountAndCapturesArgs()
    {
        var tracker = new TrackingBusinessMetrics();

        tracker.RecordOrderPlaced(150.00m, "BRL");
        tracker.RecordOrderPlaced(200.00m, "USD");

        tracker.OrdersPlacedCount.Should().Be(2);
        tracker.LastOrderAmount.Should().Be(200.00m);
        tracker.LastOrderCurrency.Should().Be("USD");
    }

    [Fact]
    public void TrackingMetrics_RecordPaymentProcessed_CapturesSuccessFlag()
    {
        var tracker = new TrackingBusinessMetrics();

        tracker.RecordPaymentProcessed(75.00m, "EUR", success: true);

        tracker.PaymentsProcessedCount.Should().Be(1);
        tracker.LastPaymentAmount.Should().Be(75.00m);
        tracker.LastPaymentCurrency.Should().Be("EUR");
        tracker.LastPaymentSuccess.Should().BeTrue();
    }

    [Fact]
    public void TrackingMetrics_RecordPaymentProcessed_FailedPayment_CapturesFalse()
    {
        var tracker = new TrackingBusinessMetrics();

        tracker.RecordPaymentProcessed(30.00m, "USD", success: false);

        tracker.PaymentsProcessedCount.Should().Be(1);
        tracker.LastPaymentSuccess.Should().BeFalse();
    }

    [Fact]
    public void TrackingMetrics_RecordUserSignup_IncrementsCountAndCapturesPlan()
    {
        var tracker = new TrackingBusinessMetrics();

        tracker.RecordUserSignup("enterprise");

        tracker.UserSignupsCount.Should().Be(1);
        tracker.LastSignupPlan.Should().Be("enterprise");
    }

    [Fact]
    public void TrackingMetrics_RecordEventConsumed_IncrementsCountAndCapturesTags()
    {
        var tracker = new TrackingBusinessMetrics();

        tracker.RecordEventConsumed("PaymentProcessed", "payment-service");

        tracker.EventsConsumedCount.Should().Be(1);
        tracker.LastEventType.Should().Be("PaymentProcessed");
        tracker.LastEventSource.Should().Be("payment-service");
    }

    [Fact]
    public void TrackingMetrics_MultipleEventConsumed_AccumulatesCount()
    {
        var tracker = new TrackingBusinessMetrics();

        tracker.RecordEventConsumed("OrderCreated", "orders");
        tracker.RecordEventConsumed("OrderShipped", "shipping");
        tracker.RecordEventConsumed("OrderDelivered", "delivery");

        tracker.EventsConsumedCount.Should().Be(3);
        tracker.LastEventType.Should().Be("OrderDelivered");
        tracker.LastEventSource.Should().Be("delivery");
    }
}
