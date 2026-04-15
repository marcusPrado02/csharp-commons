using MarcusPrado.Platform.Messaging.Abstractions;

namespace MarcusPrado.Platform.Messaging.DLQ;

/// <summary>Re-publishes messages from the dead-letter queue to their original topic.</summary>
public sealed class DLQReprocessor
{
    private readonly IMessagePublisher _publisher;

    /// <summary>Initialises the reprocessor with the given publisher.</summary>
    public DLQReprocessor(IMessagePublisher publisher)
    {
        ArgumentNullException.ThrowIfNull(publisher);
        _publisher = publisher;
    }

    /// <summary>Re-publishes the given <paramref name="message"/> to <paramref name="originalTopic"/>.</summary>
    public async Task ReprocessAsync(DeadLetterMessage message, string originalTopic, CancellationToken ct = default)
    {
        await _publisher.PublishAsync(originalTopic, message.Original, null, ct);
    }
}
