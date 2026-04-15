// <copyright file="IServiceBusConsumer.cs" company="MarcusPrado">
// Copyright (c) MarcusPrado. All rights reserved.
// </copyright>

namespace MarcusPrado.Platform.AzureServiceBus.Consumer;

/// <summary>Consumes messages from an Azure Service Bus queue or topic subscription.</summary>
public interface IServiceBusConsumer : IAsyncDisposable
{
    /// <summary>
    /// Starts a <see cref="ServiceBusProcessor"/> that calls <paramref name="handler"/> for every
    /// received message until <paramref name="ct"/> is cancelled.
    /// </summary>
    /// <param name="queueOrTopic">The queue or topic name to process messages from.</param>
    /// <param name="handler">The delegate invoked for each received message.</param>
    /// <param name="ct">A cancellation token that stops the processor when cancelled.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task StartAsync(
        string queueOrTopic,
        Func<ServiceBusReceivedMessage, CancellationToken, Task> handler,
        CancellationToken ct = default
    );
}
