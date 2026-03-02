namespace MarcusPrado.Platform.OutboxInbox.Inbox;

/// <summary>Handles a specific event type arriving via the inbox.</summary>
public interface IInboxMessageHandler
{
    /// <summary>Gets the event type name this handler is responsible for.</summary>
    string EventType { get; }

    /// <summary>Processes the raw JSON <paramref name="payload"/>.</summary>
    Task HandleAsync(string payload, CancellationToken ct = default);
}
