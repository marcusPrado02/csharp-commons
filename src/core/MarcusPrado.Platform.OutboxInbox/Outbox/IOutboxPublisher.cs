namespace MarcusPrado.Platform.OutboxInbox.Outbox;

/// <summary>Dispatches an outbox message to the external message bus.</summary>
public interface IOutboxPublisher
{
    /// <summary>Publishes <paramref name="message"/> to the appropriate topic.</summary>
    Task PublishAsync(OutboxMessage message, CancellationToken ct = default);
}
