using MarcusPrado.Platform.Messaging.Envelope;

namespace MarcusPrado.Platform.Messaging.DLQ;

/// <summary>Carries the failed message and its failure context to the DLQ.</summary>
public sealed class DeadLetterMessage
{
    /// <summary>Gets the original message envelope.</summary>
    public MessageEnvelope Original { get; init; } = new();

    /// <summary>Gets the error that caused the failure.</summary>
    public string? ErrorMessage { get; init; }

    /// <summary>Gets the number of processing attempts made.</summary>
    public int RetryCount { get; init; }

    /// <summary>Gets when the message was sent to the DLQ.</summary>
    public DateTimeOffset SentAt { get; init; } = DateTimeOffset.UtcNow;
}
