using MarcusPrado.Platform.Messaging.Envelope;

namespace MarcusPrado.Platform.Messaging.Abstractions;

/// <summary>Publishes messages to a topic or exchange.</summary>
public interface IMessagePublisher
{
    /// <summary>Publishes <paramref name="message"/> to the given <paramref name="topic"/>.</summary>
    Task PublishAsync<TMessage>(
        string topic,
        TMessage message,
        MessageMetadata? metadata = null,
        CancellationToken ct = default)
        where TMessage : class;
}
