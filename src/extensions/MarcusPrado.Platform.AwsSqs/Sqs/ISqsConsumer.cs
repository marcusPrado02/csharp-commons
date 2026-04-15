// <copyright file="ISqsConsumer.cs" company="MarcusPrado">
// Copyright (c) MarcusPrado. All rights reserved.
// </copyright>

namespace MarcusPrado.Platform.AwsSqs.Sqs;

/// <summary>Consumes messages from an AWS SQS queue.</summary>
public interface ISqsConsumer
{
    /// <summary>
    /// Starts a long-polling receive loop that calls <paramref name="handler"/> for each message
    /// until <paramref name="ct"/> is cancelled.
    /// </summary>
    /// <param name="queueUrl">The URL of the SQS queue to consume from.</param>
    /// <param name="handler">
    /// A delegate invoked for each received message. Return <see langword="true"/> to delete the
    /// message (acknowledged), or <see langword="false"/> to leave it for redelivery.
    /// </param>
    /// <param name="ct">A cancellation token that stops the consume loop when cancelled.</param>
    /// <returns>A <see cref="Task"/> representing the long-running consume operation.</returns>
    Task StartAsync(
        string queueUrl,
        Func<Message, CancellationToken, Task<bool>> handler,
        CancellationToken ct = default
    );
}
