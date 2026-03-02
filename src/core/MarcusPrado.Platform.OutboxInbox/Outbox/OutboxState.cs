namespace MarcusPrado.Platform.OutboxInbox.Outbox;

/// <summary>Lifecycle state of an outbox message.</summary>
public enum OutboxState
{
    /// <summary>Waiting to be published.</summary>
    Pending,

    /// <summary>Successfully published to the message bus.</summary>
    Published,

    /// <summary>All retry attempts exhausted.</summary>
    Failed,

    /// <summary>Locked by a processor instance.</summary>
    Processing,
}
