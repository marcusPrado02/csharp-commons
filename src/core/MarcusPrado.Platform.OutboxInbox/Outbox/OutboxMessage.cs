namespace MarcusPrado.Platform.OutboxInbox.Outbox;

/// <summary>A durable record of an event that must be published.</summary>
public sealed class OutboxMessage
{
    /// <summary>Gets or sets the unique message identifier.</summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Gets or sets the fully-qualified event type name.</summary>
    public string EventType { get; set; } = string.Empty;

    /// <summary>Gets or sets the JSON-serialised event payload.</summary>
    public string Payload { get; set; } = string.Empty;

    /// <summary>Gets or sets the target topic / exchange / queue name.</summary>
    public string Topic { get; set; } = string.Empty;

    /// <summary>Gets or sets the current processing state.</summary>
    public OutboxState State { get; set; } = OutboxState.Pending;

    /// <summary>Gets or sets when the message was enqueued.</summary>
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>Gets or sets when the message should not be processed before.</summary>
    public DateTimeOffset ScheduledAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>Gets or sets how many publish attempts have been made.</summary>
    public int RetryCount { get; set; }

    /// <summary>Gets or sets the last error message, if any.</summary>
    public string? LastError { get; set; }

    /// <summary>Gets or sets when the last attempt occurred.</summary>
    public DateTimeOffset? LastAttemptAt { get; set; }

    /// <summary>Gets or sets an optional correlation / trace identifier.</summary>
    public string? CorrelationId { get; set; }
}
