using System.Diagnostics.Metrics;
using MarcusPrado.Platform.Observability.Metrics;

namespace MarcusPrado.Platform.OpenTelemetry.Metrics;

/// <summary>
/// OpenTelemetry implementation of <see cref="IBusinessMetrics"/> using
/// <see cref="System.Diagnostics.Metrics"/> instruments.
/// </summary>
public sealed class OtelBusinessMetrics : IBusinessMetrics
{
    private readonly Counter<long> _ordersPlaced;
    private readonly Histogram<double> _ordersAmount;
    private readonly Counter<long> _paymentsProcessed;
    private readonly Histogram<double> _paymentsAmount;
    private readonly Counter<long> _userSignups;
    private readonly Counter<long> _eventsConsumed;

    /// <summary>
    /// Initialises a new instance of <see cref="OtelBusinessMetrics"/>
    /// using the provided <see cref="Meter"/>.
    /// </summary>
    /// <param name="meter">The <see cref="Meter"/> to create instruments on.</param>
    public OtelBusinessMetrics(Meter meter)
    {
        ArgumentNullException.ThrowIfNull(meter);

        _ordersPlaced = meter.CreateCounter<long>(
            "business.orders.placed",
            unit: "orders",
            description: "Total number of orders placed."
        );

        _ordersAmount = meter.CreateHistogram<double>(
            "business.orders.amount",
            unit: "currency",
            description: "Monetary value of orders placed."
        );

        _paymentsProcessed = meter.CreateCounter<long>(
            "business.payments.processed",
            unit: "payments",
            description: "Total number of payments processed."
        );

        _paymentsAmount = meter.CreateHistogram<double>(
            "business.payments.amount",
            unit: "currency",
            description: "Monetary value of payments processed."
        );

        _userSignups = meter.CreateCounter<long>(
            "business.users.signup",
            unit: "users",
            description: "Total number of user signups."
        );

        _eventsConsumed = meter.CreateCounter<long>(
            "business.events.consumed",
            unit: "events",
            description: "Total number of domain events consumed."
        );
    }

    /// <summary>
    /// Initialises a new instance of <see cref="OtelBusinessMetrics"/>
    /// using a default platform <see cref="Meter"/>.
    /// </summary>
    public OtelBusinessMetrics()
        : this(new Meter("MarcusPrado.Platform.Business", "1.0.0")) { }

    /// <inheritdoc/>
    public void RecordOrderPlaced(decimal amount, string currency)
    {
        var tagCurrency = new KeyValuePair<string, object?>("currency", currency);
        _ordersPlaced.Add(1, tagCurrency);
        _ordersAmount.Record((double)amount, tagCurrency);
    }

    /// <inheritdoc/>
    public void RecordPaymentProcessed(decimal amount, string currency, bool success)
    {
        var tagCurrency = new KeyValuePair<string, object?>("currency", currency);
        var tagSuccess = new KeyValuePair<string, object?>("success", success);
        _paymentsProcessed.Add(1, tagCurrency, tagSuccess);
        _paymentsAmount.Record((double)amount, tagCurrency, tagSuccess);
    }

    /// <inheritdoc/>
    public void RecordUserSignup(string plan)
    {
        _userSignups.Add(1, new KeyValuePair<string, object?>("plan", plan));
    }

    /// <inheritdoc/>
    public void RecordEventConsumed(string eventType, string source)
    {
        _eventsConsumed.Add(
            1,
            new KeyValuePair<string, object?>("event_type", eventType),
            new KeyValuePair<string, object?>("source", source)
        );
    }
}
