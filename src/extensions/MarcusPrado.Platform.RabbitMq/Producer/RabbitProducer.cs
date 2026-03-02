using System.Text;
using MarcusPrado.Platform.Messaging.Abstractions;
using MarcusPrado.Platform.Messaging.Envelope;
using MarcusPrado.Platform.Messaging.Serialization;
using MarcusPrado.Platform.RabbitMq.Options;
using RabbitMQ.Client;

namespace MarcusPrado.Platform.RabbitMq.Producer;

/// <summary>RabbitMQ-backed <see cref="IMessagePublisher"/>.</summary>
public sealed class RabbitProducer : IMessagePublisher, IAsyncDisposable
{
    private readonly IChannel _channel;
    private readonly IMessageSerializer _serializer;
    private readonly RabbitMqOptions _options;

    /// <summary>Initialises the producer. Call <see cref="CreateAsync"/> instead of this constructor.</summary>
    private RabbitProducer(
        IChannel channel,
        IMessageSerializer serializer,
        RabbitMqOptions options)
    {
        _channel = channel;
        _serializer = serializer;
        _options = options;
    }

    /// <summary>Creates and initialises a <see cref="RabbitProducer"/>.</summary>
    public static async Task<RabbitProducer> CreateAsync(
        RabbitMqOptions options,
        IMessageSerializer serializer)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(serializer);

        var factory = new ConnectionFactory { Uri = new Uri(options.ConnectionString) };
        var connection = await factory.CreateConnectionAsync();
        var channel = await connection.CreateChannelAsync();

        await channel.ExchangeDeclareAsync(
            exchange: options.Exchange,
            type: options.ExchangeType,
            durable: true,
            autoDelete: false);

        return new RabbitProducer(channel, serializer, options);
    }

    /// <inheritdoc/>
    public async Task PublishAsync<TMessage>(
        string topic,
        TMessage message,
        MessageMetadata? metadata = null,
        CancellationToken ct = default)
        where TMessage : class
    {
        ArgumentNullException.ThrowIfNull(message);

        var envelope = new MessageEnvelope<TMessage>
        {
            Metadata = metadata ?? new MessageMetadata(),
            Payload = message,
        };

        var json = _serializer.Serialize(envelope);
        var body = Encoding.UTF8.GetBytes(json);

        var props = new BasicProperties
        {
            ContentType = "application/json",
            DeliveryMode = DeliveryModes.Persistent,
            MessageId = envelope.Metadata.MessageId,
            CorrelationId = envelope.Metadata.CorrelationId,
        };

        await _channel.BasicPublishAsync(
            exchange: _options.Exchange,
            routingKey: topic,
            mandatory: false,
            basicProperties: props,
            body: body,
            cancellationToken: ct);
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        await _channel.CloseAsync();
        _channel.Dispose();
    }
}
