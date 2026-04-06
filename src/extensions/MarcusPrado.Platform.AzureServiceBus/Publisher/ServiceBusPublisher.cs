// <copyright file="ServiceBusPublisher.cs" company="MarcusPrado">
// Copyright (c) MarcusPrado. All rights reserved.
// </copyright>

using System.Text.Json;

namespace MarcusPrado.Platform.AzureServiceBus.Publisher;

/// <summary>Azure Service Bus implementation of <see cref="IServiceBusPublisher"/>.</summary>
public sealed class ServiceBusPublisher : IServiceBusPublisher
{
    private readonly ServiceBusClient _client;

    /// <summary>Initialises a new instance of <see cref="ServiceBusPublisher"/>.</summary>
    /// <param name="client">The <see cref="ServiceBusClient"/> used to create senders.</param>
    public ServiceBusPublisher(ServiceBusClient client)
    {
        ArgumentNullException.ThrowIfNull(client);
        _client = client;
    }

    /// <inheritdoc/>
    public Task SendAsync<T>(string queueOrTopic, T message, CancellationToken ct = default)
        => SendAsync(queueOrTopic, message, sessionId: null, ct);

    /// <inheritdoc/>
    public async Task SendAsync<T>(string queueOrTopic, T message, string? sessionId, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(queueOrTopic);
        ArgumentNullException.ThrowIfNull(message);

        var json = JsonSerializer.Serialize(message);
        var sbMessage = new ServiceBusMessage(json)
        {
            ContentType = "application/json",
        };

        if (sessionId is not null)
        {
            sbMessage.SessionId = sessionId;
        }

        await using var sender = _client.CreateSender(queueOrTopic);
        await sender.SendMessageAsync(sbMessage, ct).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}
