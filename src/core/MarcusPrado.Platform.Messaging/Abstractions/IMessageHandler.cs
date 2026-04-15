using MarcusPrado.Platform.Messaging.Envelope;

namespace MarcusPrado.Platform.Messaging.Abstractions;

/// <summary>Handles a message of type <typeparamref name="TMessage"/>.</summary>
/// <typeparam name="TMessage">The message payload type.</typeparam>
public interface IMessageHandler<TMessage>
    where TMessage : class
{
    /// <summary>Processes the received <paramref name="envelope"/>.</summary>
    Task HandleAsync(MessageEnvelope<TMessage> envelope, CancellationToken ct = default);
}
