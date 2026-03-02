namespace MarcusPrado.Platform.OutboxInbox.Inbox;

/// <summary>Configures the <see cref="InboxProcessor"/>.</summary>
public sealed class InboxProcessorOptions
{
    /// <summary>Gets or sets how often the processor polls for pending messages.</summary>
    public TimeSpan PollingInterval { get; set; } = TimeSpan.FromSeconds(5);

    /// <summary>Gets or sets how many messages are retrieved per polling cycle.</summary>
    public int BatchSize { get; set; } = 50;

    /// <summary>Gets or sets how many times a message is retried before being marked failed.</summary>
    public int MaxRetries { get; set; } = 5;

    /// <summary>Gets or sets how long an idempotency record is retained.</summary>
    public TimeSpan IdempotencyTtl { get; set; } = TimeSpan.FromDays(1);
}
