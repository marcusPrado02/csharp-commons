namespace MarcusPrado.Platform.OutboxInbox.Inbox;

/// <summary>Lifecycle state of an inbox message.</summary>
public enum InboxState
{
    /// <summary>Received but not yet processed.</summary>
    Pending,

    /// <summary>Successfully dispatched to the handler.</summary>
    Processed,

    /// <summary>All retry attempts exhausted.</summary>
    Failed,

    /// <summary>Duplicate — already processed via idempotency check.</summary>
    Duplicate,
}
