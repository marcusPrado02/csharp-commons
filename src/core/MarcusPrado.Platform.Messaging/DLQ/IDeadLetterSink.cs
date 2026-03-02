using MarcusPrado.Platform.Messaging.Envelope;

namespace MarcusPrado.Platform.Messaging.DLQ;

/// <summary>Routes failed messages to a dead-letter destination.</summary>
public interface IDeadLetterSink
{
    /// <summary>Sends <paramref name="envelope"/> to the dead-letter queue.</summary>
    Task SendToDeadLetterAsync(
        MessageEnvelope envelope,
        Exception? reason = null,
        CancellationToken ct = default);
}
