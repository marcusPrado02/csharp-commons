// <copyright file="SnsPublisher.cs" company="MarcusPrado">
// Copyright (c) MarcusPrado. All rights reserved.
// </copyright>

using System.Text.Json;

namespace MarcusPrado.Platform.AwsSqs.Sns;

/// <summary>AWS SNS implementation of <see cref="ISnsPublisher"/>.</summary>
public sealed class SnsPublisher : ISnsPublisher
{
    private readonly IAmazonSimpleNotificationService _client;

    /// <summary>Initialises a new instance of <see cref="SnsPublisher"/>.</summary>
    /// <param name="client">The <see cref="IAmazonSimpleNotificationService"/> client used to publish messages.</param>
    public SnsPublisher(IAmazonSimpleNotificationService client)
    {
        ArgumentNullException.ThrowIfNull(client);
        _client = client;
    }

    /// <inheritdoc/>
    public Task PublishAsync<T>(string topicArn, T message, CancellationToken ct = default)
        => PublishAsync(topicArn, message, subject: string.Empty, ct);

    /// <inheritdoc/>
    public async Task PublishAsync<T>(string topicArn, T message, string subject, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(topicArn);
        ArgumentNullException.ThrowIfNull(message);
        ArgumentNullException.ThrowIfNull(subject);

        var json = JsonSerializer.Serialize(message);

        var request = new PublishRequest
        {
            TopicArn = topicArn,
            Message = json,
        };

        if (!string.IsNullOrEmpty(subject))
        {
            request.Subject = subject;
        }

        await _client.PublishAsync(request, ct).ConfigureAwait(false);
    }
}
