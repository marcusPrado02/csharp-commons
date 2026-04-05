namespace MarcusPrado.Platform.Observability.Metrics;

/// <summary>
/// Records domain-level business metrics for observability dashboards
/// and SLO tracking.
/// </summary>
public interface IBusinessMetrics
{
    /// <summary>Records an order placed event with the given amount and currency.</summary>
    /// <param name="amount">The monetary value of the order.</param>
    /// <param name="currency">The ISO 4217 currency code (e.g. "USD", "BRL").</param>
    void RecordOrderPlaced(decimal amount, string currency);

    /// <summary>Records a payment processed event.</summary>
    /// <param name="amount">The monetary value of the payment.</param>
    /// <param name="currency">The ISO 4217 currency code.</param>
    /// <param name="success">Whether the payment succeeded.</param>
    void RecordPaymentProcessed(decimal amount, string currency, bool success);

    /// <summary>Records a new user signup for the given plan.</summary>
    /// <param name="plan">The subscription plan (e.g. "free", "pro", "enterprise").</param>
    void RecordUserSignup(string plan);

    /// <summary>Records a domain event consumed from an event source.</summary>
    /// <param name="eventType">The type of the event (e.g. "OrderCreated").</param>
    /// <param name="source">The source system or topic that produced the event.</param>
    void RecordEventConsumed(string eventType, string source);
}
