namespace MarcusPrado.Platform.DlqReprocessing.Options;

/// <summary>
/// Configuration options for the DLQ reprocessing subsystem.
/// </summary>
public sealed class DlqOptions
{
    /// <summary>
    /// Gets or sets the interval in seconds between polling cycles.
    /// Defaults to <c>30</c>.
    /// </summary>
    public int PollingIntervalSeconds { get; set; } = 30;

    /// <summary>
    /// Gets or sets the depth threshold above which a warning is logged per topic.
    /// Defaults to <c>100</c>.
    /// </summary>
    public int AlertThreshold { get; set; } = 100;

    /// <summary>
    /// Gets or sets the list of topics to monitor.
    /// </summary>
    public List<string> Topics { get; set; } = [];
}
