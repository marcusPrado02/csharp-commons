namespace MarcusPrado.Platform.Nats.Consumer;

/// <summary>
/// NATS-backed <see cref="INatsConsumer"/> with optional JetStream
/// at-least-once delivery semantics.
/// </summary>
public sealed class NatsConsumer : INatsConsumer
{
    private static readonly JsonSerializerOptions _serializerOptions = new(JsonSerializerDefaults.Web);

    private readonly INatsConnection _connection;
    private readonly NatsOptions _options;

    /// <summary>
    /// Initialises the consumer with an injected NATS connection and options.
    /// </summary>
    /// <param name="connection">The NATS connection to use for subscriptions.</param>
    /// <param name="options">NATS configuration options.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="connection"/> or <paramref name="options"/> is
    /// <see langword="null"/>.
    /// </exception>
    public NatsConsumer(INatsConnection connection, NatsOptions options)
    {
        ArgumentNullException.ThrowIfNull(connection);
        ArgumentNullException.ThrowIfNull(options);
        _connection = connection;
        _options = options;
    }

    /// <inheritdoc/>
    public async Task SubscribeAsync<T>(
        string subject,
        Func<T, CancellationToken, Task> handler,
        CancellationToken ct = default
    )
        where T : class
    {
        ArgumentNullException.ThrowIfNull(handler);
        ArgumentException.ThrowIfNullOrWhiteSpace(subject);

        if (_options.JetStream)
        {
            await SubscribeJetStreamAsync(subject, handler, ct).ConfigureAwait(false);
        }
        else
        {
            await SubscribeCoreAsync(subject, handler, ct).ConfigureAwait(false);
        }
    }

    private async Task SubscribeCoreAsync<T>(
        string subject,
        Func<T, CancellationToken, Task> handler,
        CancellationToken ct
    )
        where T : class
    {
#pragma warning disable S3267 // Async enumerables cannot use LINQ Select — null guards are intentional
        await foreach (var msg in _connection.SubscribeAsync<string>(subject, cancellationToken: ct))
        {
            if (msg.Data is null)
            {
                continue;
            }

            var payload = JsonSerializer.Deserialize<T>(msg.Data, _serializerOptions);
            if (payload is not null)
            {
                await handler(payload, ct).ConfigureAwait(false);
            }
        }
#pragma warning restore S3267
    }

    private async Task SubscribeJetStreamAsync<T>(
        string subject,
        Func<T, CancellationToken, Task> handler,
        CancellationToken ct
    )
        where T : class
    {
        var js = new NatsJSContext(_connection);

        // Derive a stream name by replacing dots with underscores.
        var streamName = subject.Replace(".", "_", StringComparison.Ordinal);
        var consumerName = streamName + "_consumer";

        try
        {
            await js.GetStreamAsync(streamName, cancellationToken: ct).ConfigureAwait(false);
        }
        catch (NatsJSException)
        {
            await js.CreateStreamAsync(new StreamConfig(streamName, [subject]), ct).ConfigureAwait(false);
        }

        var consumer = await js.CreateOrUpdateConsumerAsync(streamName, new ConsumerConfig(consumerName), ct)
            .ConfigureAwait(false);

        await foreach (var msg in consumer.ConsumeAsync<string>(cancellationToken: ct))
        {
            if (msg.Data is null)
            {
                await msg.AckAsync(cancellationToken: ct).ConfigureAwait(false);
                continue;
            }

            var payload = JsonSerializer.Deserialize<T>(msg.Data, _serializerOptions);
            if (payload is not null)
            {
                await handler(payload, ct).ConfigureAwait(false);
            }

            await msg.AckAsync(cancellationToken: ct).ConfigureAwait(false);
        }
    }
}
