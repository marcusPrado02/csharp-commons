// <copyright file="ISqsPublisher.cs" company="MarcusPrado">
// Copyright (c) MarcusPrado. All rights reserved.
// </copyright>

namespace MarcusPrado.Platform.AwsSqs.Sqs;

/// <summary>Publishes messages to an AWS SQS queue.</summary>
public interface ISqsPublisher
{
    /// <summary>Serializes <paramref name="message"/> as JSON and sends it to the specified SQS queue.</summary>
    /// <typeparam name="T">The message payload type.</typeparam>
    /// <param name="queueUrl">The URL of the target SQS queue.</param>
    /// <param name="message">The message to send.</param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous send operation.</returns>
    Task SendAsync<T>(string queueUrl, T message, CancellationToken ct = default);

    /// <summary>Serializes <paramref name="message"/> as JSON and sends it to the specified FIFO SQS queue.</summary>
    /// <typeparam name="T">The message payload type.</typeparam>
    /// <param name="queueUrl">The URL of the target SQS FIFO queue.</param>
    /// <param name="message">The message to send.</param>
    /// <param name="messageGroupId">The FIFO message group ID, or <see langword="null"/> for standard queues.</param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous send operation.</returns>
    Task SendAsync<T>(string queueUrl, T message, string? messageGroupId, CancellationToken ct = default);
}
