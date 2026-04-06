namespace MarcusPrado.Platform.DlqReprocessing.Metrics;

/// <summary>
/// Abstraction for recording DLQ-related OpenTelemetry metrics.
/// </summary>
public interface IDlqMetrics
{
    /// <summary>
    /// Records the current depth of the DLQ for <paramref name="topic"/>.
    /// </summary>
    /// <param name="topic">The topic or queue name.</param>
    /// <param name="depth">Current number of dead-lettered messages.</param>
    void RecordDepth(string topic, long depth);

    /// <summary>
    /// Increments the counter tracking total reprocessed messages for <paramref name="topic"/>.
    /// </summary>
    /// <param name="topic">The topic or queue name.</param>
    void RecordReprocessed(string topic);

    /// <summary>
    /// Increments the counter tracking total deleted messages for <paramref name="topic"/>.
    /// </summary>
    /// <param name="topic">The topic or queue name.</param>
    void RecordDeleted(string topic);
}
