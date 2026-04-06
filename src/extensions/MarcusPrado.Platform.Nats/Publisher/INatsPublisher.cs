namespace MarcusPrado.Platform.Nats.Publisher;

/// <summary>Defines a publisher that sends messages to NATS subjects.</summary>
public interface INatsPublisher
{
    /// <summary>
    /// Publishes a message of type <typeparamref name="T"/> to the specified NATS subject.
    /// </summary>
    /// <typeparam name="T">The message payload type.</typeparam>
    /// <param name="subject">The NATS subject to publish to.</param>
    /// <param name="message">The message payload to publish.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A <see cref="Task"/> that completes when the message is published.</returns>
    Task PublishAsync<T>(string subject, T message, CancellationToken ct = default)
        where T : class;
}
