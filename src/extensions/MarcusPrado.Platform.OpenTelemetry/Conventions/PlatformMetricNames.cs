namespace MarcusPrado.Platform.OpenTelemetry.Conventions;

/// <summary>Standard metric names used across platform services.</summary>
public static class PlatformMetricNames
{
    /// <summary>Histogram of command handler execution duration in milliseconds.</summary>
    public const string CommandDurationMs = "platform.command.duration_ms";

    /// <summary>Counter of events successfully published.</summary>
    public const string EventsPublished = "platform.events.published";

    /// <summary>Counter of events successfully consumed.</summary>
    public const string EventsConsumed = "platform.events.consumed";

    /// <summary>Counter of messages sent to the dead-letter queue.</summary>
    public const string DlqMessages = "platform.dlq.messages";

    /// <summary>Histogram of HTTP request duration in milliseconds.</summary>
    public const string HttpRequestDurationMs = "platform.http.request_duration_ms";
}
