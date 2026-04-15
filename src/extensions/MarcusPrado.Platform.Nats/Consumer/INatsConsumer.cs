namespace MarcusPrado.Platform.Nats.Consumer;

/// <summary>
/// Defines a subscriber that consumes messages from NATS subjects,
/// with optional JetStream at-least-once delivery.
/// </summary>
public interface INatsConsumer
{
    /// <summary>
    /// Subscribes to the specified NATS subject and invokes the handler for each
    /// received message.  When JetStream is enabled the messages are acknowledged
    /// after the handler completes successfully.
    /// </summary>
    /// <typeparam name="T">The message payload type.</typeparam>
    /// <param name="subject">The NATS subject to subscribe to.</param>
    /// <param name="handler">The async handler invoked for every received message.</param>
    /// <param name="ct">Cancellation token used to stop the subscription loop.</param>
    /// <returns>A <see cref="Task"/> that completes when the subscription loop exits.</returns>
    Task SubscribeAsync<T>(string subject, Func<T, CancellationToken, Task> handler, CancellationToken ct = default)
        where T : class;
}
