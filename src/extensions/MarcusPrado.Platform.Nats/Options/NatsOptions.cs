namespace MarcusPrado.Platform.Nats.Options;

/// <summary>Configuration options for the NATS platform integration.</summary>
public sealed class NatsOptions
{
    /// <summary>Gets or sets the NATS server URL (default: nats://localhost:4222).</summary>
    public string Url { get; set; } = "nats://localhost:4222";

    /// <summary>
    /// Gets or sets the maximum number of reconnect attempts before giving up.
    /// Use <c>-1</c> for infinite retries (default: 3).
    /// </summary>
    public int MaxReconnectAttempts { get; set; } = 3;

    /// <summary>
    /// Gets or sets a value indicating whether JetStream at-least-once delivery
    /// is enabled for consumers (default: <see langword="false"/>).
    /// </summary>
    public bool JetStream { get; set; }
}
