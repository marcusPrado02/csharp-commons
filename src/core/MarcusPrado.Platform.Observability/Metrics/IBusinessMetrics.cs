namespace MarcusPrado.Platform.Observability.Metrics;

/// <summary>
/// Records domain-level business metrics for observability dashboards
/// and SLO tracking.
/// </summary>
public interface IBusinessMetrics
{
    /// <summary>Records an order placed event.</summary>
    void RecordOrderPlaced(string tenantId, decimal value, string currency);

    /// <summary>Records a payment processed event.</summary>
    void RecordPaymentProcessed(string tenantId, string status, string gateway);

    /// <summary>Records a new user signup.</summary>
    void RecordUserSignup(string tenantId, string plan);

    /// <summary>Records a message consumed from an event topic.</summary>
    void RecordEventConsumed(string topic, string consumerGroup, long latencyMs);
}
