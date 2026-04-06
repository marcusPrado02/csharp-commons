// <copyright file="SqsPublisher.cs" company="MarcusPrado">
// Copyright (c) MarcusPrado. All rights reserved.
// </copyright>

using System.Text.Json;

namespace MarcusPrado.Platform.AwsSqs.Sqs;

/// <summary>AWS SQS implementation of <see cref="ISqsPublisher"/>.</summary>
public sealed class SqsPublisher : ISqsPublisher
{
    private readonly IAmazonSQS _client;

    /// <summary>Initialises a new instance of <see cref="SqsPublisher"/>.</summary>
    /// <param name="client">The <see cref="IAmazonSQS"/> client used to send messages.</param>
    public SqsPublisher(IAmazonSQS client)
    {
        ArgumentNullException.ThrowIfNull(client);
        _client = client;
    }

    /// <inheritdoc/>
    public Task SendAsync<T>(string queueUrl, T message, CancellationToken ct = default)
        => SendAsync(queueUrl, message, messageGroupId: null, ct);

    /// <inheritdoc/>
    public async Task SendAsync<T>(string queueUrl, T message, string? messageGroupId, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(queueUrl);
        ArgumentNullException.ThrowIfNull(message);

        var json = JsonSerializer.Serialize(message);

        var request = new SendMessageRequest
        {
            QueueUrl = queueUrl,
            MessageBody = json,
        };

        if (messageGroupId is not null)
        {
            request.MessageGroupId = messageGroupId;
            request.MessageDeduplicationId = Guid.NewGuid().ToString("N");
        }

        await _client.SendMessageAsync(request, ct).ConfigureAwait(false);
    }
}
