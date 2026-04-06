// <copyright file="ServiceBusDeadLetterSink.cs" company="MarcusPrado">
// Copyright (c) MarcusPrado. All rights reserved.
// </copyright>

namespace MarcusPrado.Platform.AzureServiceBus.DeadLetter;

/// <summary>Moves messages to the Azure Service Bus dead-letter sub-queue.</summary>
public sealed class ServiceBusDeadLetterSink
{
    private readonly ServiceBusClient _client;

    /// <summary>Initialises a new instance of <see cref="ServiceBusDeadLetterSink"/>.</summary>
    /// <param name="client">The <see cref="ServiceBusClient"/> used to create receivers.</param>
    public ServiceBusDeadLetterSink(ServiceBusClient client)
    {
        ArgumentNullException.ThrowIfNull(client);
        _client = client;
    }

    /// <summary>
    /// Moves <paramref name="message"/> to the dead-letter sub-queue with the provided
    /// <paramref name="reason"/>.
    /// </summary>
    /// <param name="queueOrTopic">The queue or topic name from which the message was received.</param>
    /// <param name="message">The message to dead-letter.</param>
    /// <param name="reason">A short description of why the message is being dead-lettered.</param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task DeadLetterAsync(
        string queueOrTopic,
        ServiceBusReceivedMessage message,
        string reason,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(queueOrTopic);
        ArgumentNullException.ThrowIfNull(message);
        ArgumentNullException.ThrowIfNull(reason);

        await using var receiver = _client.CreateReceiver(queueOrTopic);
        await receiver.DeadLetterMessageAsync(message, reason, cancellationToken: ct).ConfigureAwait(false);
    }
}
