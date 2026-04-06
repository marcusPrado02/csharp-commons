// <copyright file="ISnsPublisher.cs" company="MarcusPrado">
// Copyright (c) MarcusPrado. All rights reserved.
// </copyright>

namespace MarcusPrado.Platform.AwsSqs.Sns;

/// <summary>Publishes messages to an AWS SNS topic for fan-out to subscribed SQS queues.</summary>
public interface ISnsPublisher
{
    /// <summary>Serializes <paramref name="message"/> as JSON and publishes it to the specified SNS topic.</summary>
    /// <typeparam name="T">The message payload type.</typeparam>
    /// <param name="topicArn">The ARN of the target SNS topic.</param>
    /// <param name="message">The message to publish.</param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous publish operation.</returns>
    Task PublishAsync<T>(string topicArn, T message, CancellationToken ct = default);

    /// <summary>Serializes <paramref name="message"/> as JSON and publishes it to the specified SNS topic with a subject.</summary>
    /// <typeparam name="T">The message payload type.</typeparam>
    /// <param name="topicArn">The ARN of the target SNS topic.</param>
    /// <param name="message">The message to publish.</param>
    /// <param name="subject">The subject of the SNS notification.</param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous publish operation.</returns>
    Task PublishAsync<T>(string topicArn, T message, string subject, CancellationToken ct = default);
}
