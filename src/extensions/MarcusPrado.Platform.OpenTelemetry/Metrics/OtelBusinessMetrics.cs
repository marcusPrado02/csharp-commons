using System.Diagnostics.Metrics;
using MarcusPrado.Platform.Observability.Metrics;

namespace MarcusPrado.Platform.OpenTelemetry.Metrics;

/// <summary>
/// OpenTelemetry implementation of <see cref="IBusinessMetrics"/> using
/// <see cref="System.Diagnostics.Metrics"/> instruments.
/// </summary>
public sealed class OtelBusinessMetrics : IBusinessMetrics
{
    private static readonly Meter PlatformMeter = new("MarcusPrado.Platform.Business", "1.0.0");

    private static readonly Counter<long> OrdersPlaced =
        PlatformMeter.CreateCounter<long>(
            "business.orders.placed",
            unit: "orders",
            description: "Total number of orders placed.");

    private static readonly Counter<decimal> OrderValue =
        PlatformMeter.CreateCounter<decimal>(
            "business.orders.value",
            unit: "currency",
            description: "Total monetary value of orders placed.");

    private static readonly Counter<long> PaymentsProcessed =
        PlatformMeter.CreateCounter<long>(
            "business.payments.processed",
            unit: "payments",
            description: "Total number of payments processed.");

    private static readonly Counter<long> UserSignups =
        PlatformMeter.CreateCounter<long>(
            "business.users.signups",
            unit: "users",
            description: "Total number of user signups.");

    private static readonly Histogram<long> EventConsumedLatency =
        PlatformMeter.CreateHistogram<long>(
            "business.events.consumed.latency",
            unit: "ms",
            description: "Latency of consumed events in milliseconds.");

    /// <inheritdoc/>
    public void RecordOrderPlaced(string tenantId, decimal value, string currency)
    {
        var tags = new TagList
        {
            { "tenant.id", tenantId },
            { "currency", currency },
        };

        OrdersPlaced.Add(1, tags);
        OrderValue.Add(value, tags);
    }

    /// <inheritdoc/>
    public void RecordPaymentProcessed(string tenantId, string status, string gateway)
    {
        PaymentsProcessed.Add(1, new TagList
        {
            { "tenant.id", tenantId },
            { "payment.status", status },
            { "payment.gateway", gateway },
        });
    }

    /// <inheritdoc/>
    public void RecordUserSignup(string tenantId, string plan)
    {
        UserSignups.Add(1, new TagList
        {
            { "tenant.id", tenantId },
            { "user.plan", plan },
        });
    }

    /// <inheritdoc/>
    public void RecordEventConsumed(string topic, string consumerGroup, long latencyMs)
    {
        EventConsumedLatency.Record(latencyMs, new TagList
        {
            { "messaging.topic", topic },
            { "messaging.consumer_group", consumerGroup },
        });
    }
}
