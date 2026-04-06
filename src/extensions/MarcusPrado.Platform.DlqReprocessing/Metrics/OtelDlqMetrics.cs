namespace MarcusPrado.Platform.DlqReprocessing.Metrics;

/// <summary>
/// OpenTelemetry implementation of <see cref="IDlqMetrics"/> using <see cref="System.Diagnostics.Metrics.Meter"/>.
/// </summary>
public sealed class OtelDlqMetrics : IDlqMetrics, IDisposable
{
    /// <summary>The meter name used for all DLQ metrics.</summary>
    public const string MeterName = "MarcusPrado.Platform.DlqReprocessing";

    private readonly Meter _meter;
    private readonly Counter<long> _reprocessedCounter;
    private readonly Counter<long> _deletedCounter;

    // We keep the latest depth per topic so the observable gauge can report it.
    private readonly ConcurrentDictionary<string, long> _depths = new(StringComparer.Ordinal);

    /// <summary>
    /// Initialises a new <see cref="OtelDlqMetrics"/> and creates the underlying instruments.
    /// </summary>
    public OtelDlqMetrics()
    {
        _meter = new Meter(MeterName, "1.0.0");

        _ = _meter.CreateObservableGauge<long>(
            "dlq.depth",
            () =>
            {
                var measurements = new List<Measurement<long>>();
                foreach (var (topic, depth) in _depths)
                    measurements.Add(new Measurement<long>(depth, new KeyValuePair<string, object?>("topic", topic)));
                return measurements;
            },
            unit: "messages",
            description: "Current number of dead-lettered messages per topic");

        _reprocessedCounter = _meter.CreateCounter<long>(
            "dlq.reprocessed_total",
            unit: "messages",
            description: "Total number of dead-lettered messages requeued for reprocessing");

        _deletedCounter = _meter.CreateCounter<long>(
            "dlq.deleted_total",
            unit: "messages",
            description: "Total number of dead-lettered messages permanently deleted");
    }

    /// <inheritdoc />
    public void RecordDepth(string topic, long depth)
    {
        ArgumentNullException.ThrowIfNull(topic);
        _depths[topic] = depth;
    }

    /// <inheritdoc />
    public void RecordReprocessed(string topic)
    {
        ArgumentNullException.ThrowIfNull(topic);
        _reprocessedCounter.Add(1, new KeyValuePair<string, object?>("topic", topic));
    }

    /// <inheritdoc />
    public void RecordDeleted(string topic)
    {
        ArgumentNullException.ThrowIfNull(topic);
        _deletedCounter.Add(1, new KeyValuePair<string, object?>("topic", topic));
    }

    /// <inheritdoc />
    public void Dispose() => _meter.Dispose();
}
