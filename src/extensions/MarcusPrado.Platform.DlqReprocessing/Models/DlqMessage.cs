namespace MarcusPrado.Platform.DlqReprocessing.Models;

/// <summary>
/// Represents a dead-lettered message awaiting inspection or reprocessing.
/// </summary>
/// <param name="Id">Unique identifier of the message.</param>
/// <param name="Topic">The broker topic or queue this message originated from.</param>
/// <param name="Payload">Raw message payload.</param>
/// <param name="FailureReason">Human-readable description of why the message was dead-lettered.</param>
/// <param name="AttemptCount">Number of processing attempts made so far.</param>
/// <param name="EnqueuedAt">UTC timestamp when the message was first added to the DLQ.</param>
/// <param name="LastAttemptAt">UTC timestamp of the most recent processing attempt, if any.</param>
public sealed record DlqMessage(
    string Id,
    string Topic,
    string Payload,
    string FailureReason,
    int AttemptCount,
    DateTimeOffset EnqueuedAt,
    DateTimeOffset? LastAttemptAt
);
