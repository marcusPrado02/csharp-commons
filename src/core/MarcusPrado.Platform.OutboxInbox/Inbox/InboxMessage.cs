namespace MarcusPrado.Platform.OutboxInbox.Inbox;

/// <summary>A durable record of an inbound event awaiting processing.</summary>
public sealed class InboxMessage
{
    /// <summary>Gets or sets the unique inbox record identifier.</summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Gets or sets the message identifier from the originating system (used for deduplication).</summary>
    public string MessageId { get; set; } = string.Empty;

    /// <summary>Gets or sets the fully-qualified event type name.</summary>
    public string EventType { get; set; } = string.Empty;

    /// <summary>Gets or sets the JSON-serialised event payload.</summary>
    public string Payload { get; set; } = string.Empty;

    /// <summary>Gets or sets the source topic / queue this message arrived from.</summary>
    public string Source { get; set; } = string.Empty;

    /// <summary>Gets or sets the current processing state.</summary>
    public InboxState State { get; set; } = InboxState.Pending;

    /// <summary>Gets or sets when the message arrived.</summary>
    public DateTimeOffset ReceivedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>Gets or sets when the message was processed.</summary>
    public DateTimeOffset? ProcessedAt { get; set; }

    /// <summary>Gets or sets how many dispatch attempts have been made.</summary>
    public int RetryCount { get; set; }

    /// <summary>Gets or sets the last error message, if any.</summary>
    public string? LastError { get; set; }
}
