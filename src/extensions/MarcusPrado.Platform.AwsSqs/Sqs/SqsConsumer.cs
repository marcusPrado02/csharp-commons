// <copyright file="SqsConsumer.cs" company="MarcusPrado">
// Copyright (c) MarcusPrado. All rights reserved.
// </copyright>

namespace MarcusPrado.Platform.AwsSqs.Sqs;

/// <summary>AWS SQS implementation of <see cref="ISqsConsumer"/>.</summary>
public sealed class SqsConsumer : ISqsConsumer
{
    private readonly IAmazonSQS _client;
    private readonly SqsOptions _options;
    private readonly ILogger<SqsConsumer> _logger;

    /// <summary>Initialises a new instance of <see cref="SqsConsumer"/>.</summary>
    /// <param name="client">The <see cref="IAmazonSQS"/> client used to receive and delete messages.</param>
    /// <param name="options">The resolved <see cref="SqsOptions"/>.</param>
    /// <param name="logger">The logger.</param>
    public SqsConsumer(IAmazonSQS client, IOptions<SqsOptions> options, ILogger<SqsConsumer> logger)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(logger);
        _client = client;
        _options = options.Value;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task StartAsync(
        string queueUrl,
        Func<Message, CancellationToken, Task<bool>> handler,
        CancellationToken ct = default
    )
    {
        ArgumentNullException.ThrowIfNull(queueUrl);
        ArgumentNullException.ThrowIfNull(handler);

        while (!ct.IsCancellationRequested)
        {
            ReceiveMessageResponse response;

            try
            {
                response = await _client
                    .ReceiveMessageAsync(
                        new ReceiveMessageRequest
                        {
                            QueueUrl = queueUrl,
                            MaxNumberOfMessages = _options.MaxNumberOfMessages,
                            WaitTimeSeconds = _options.WaitTimeSeconds,
                            VisibilityTimeout = _options.VisibilityTimeoutSeconds,
                        },
                        ct
                    )
                    .ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                break;
            }

            foreach (var message in response.Messages)
            {
                try
                {
                    var success = await handler(message, ct).ConfigureAwait(false);

                    if (success)
                    {
                        await _client.DeleteMessageAsync(queueUrl, message.ReceiptHandle, ct).ConfigureAwait(false);
                    }
                }
#pragma warning disable CA1031 // Consumer must not surface unexpected handler exceptions; log and continue
                catch (Exception ex) when (!ct.IsCancellationRequested)
                {
                    _logger.LogError(
                        ex,
                        "Unhandled exception processing SQS message {MessageId} from {QueueUrl}",
                        message.MessageId,
                        queueUrl
                    );
                }
#pragma warning restore CA1031
            }
        }
    }
}
