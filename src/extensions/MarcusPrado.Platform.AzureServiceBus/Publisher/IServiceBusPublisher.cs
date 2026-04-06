// <copyright file="IServiceBusPublisher.cs" company="MarcusPrado">
// Copyright (c) MarcusPrado. All rights reserved.
// </copyright>

namespace MarcusPrado.Platform.AzureServiceBus.Publisher;

/// <summary>Publishes messages to Azure Service Bus queues or topics.</summary>
public interface IServiceBusPublisher : IAsyncDisposable
{
    /// <summary>Serializes <paramref name="message"/> and sends it to the specified queue or topic.</summary>
    /// <typeparam name="T">The message payload type.</typeparam>
    /// <param name="queueOrTopic">The target queue or topic name.</param>
    /// <param name="message">The message to send.</param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task SendAsync<T>(string queueOrTopic, T message, CancellationToken ct = default);

    /// <summary>Serializes <paramref name="message"/> and sends it to the specified queue or topic with an optional session ID.</summary>
    /// <typeparam name="T">The message payload type.</typeparam>
    /// <param name="queueOrTopic">The target queue or topic name.</param>
    /// <param name="message">The message to send.</param>
    /// <param name="sessionId">The session ID to assign, or <see langword="null"/> for a non-session message.</param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task SendAsync<T>(string queueOrTopic, T message, string? sessionId, CancellationToken ct = default);
}
