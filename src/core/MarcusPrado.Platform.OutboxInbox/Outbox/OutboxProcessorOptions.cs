namespace MarcusPrado.Platform.OutboxInbox.Outbox;

/// <summary>Configures the <see cref="OutboxProcessor"/>.</summary>
public sealed class OutboxProcessorOptions
{
    /// <summary>Gets or sets how often the processor polls for pending messages.</summary>
    public TimeSpan PollingInterval { get; set; } = TimeSpan.FromSeconds(5);

    /// <summary>Gets or sets how many messages are retrieved per polling cycle.</summary>
    public int BatchSize { get; set; } = 50;

    /// <summary>Gets or sets how many times a message is retried before being marked failed.</summary>
    public int MaxRetries { get; set; } = 5;

    /// <summary>Gets or sets the base delay used for exponential backoff calculation.</summary>
    public TimeSpan RetryBaseDelay { get; set; } = TimeSpan.FromSeconds(10);
}
