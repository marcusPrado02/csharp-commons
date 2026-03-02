using Confluent.Kafka;
using MarcusPrado.Platform.Kafka.Options;
using MarcusPrado.Platform.Messaging.Abstractions;
using MarcusPrado.Platform.Messaging.Serialization;
using Envelope = MarcusPrado.Platform.Messaging.Envelope;
using PlatformMetadata = MarcusPrado.Platform.Messaging.Envelope.MessageMetadata;

namespace MarcusPrado.Platform.Kafka.Producer;

/// <summary>Kafka-backed <see cref="IMessagePublisher"/>.</summary>
public sealed class KafkaProducer : IMessagePublisher, IDisposable
{
    private readonly IProducer<string, string> _producer;
    private readonly IMessageSerializer _serializer;
    private readonly KafkaOptions _options;

    /// <summary>Initialises the producer with the given Kafka options.</summary>
    public KafkaProducer(KafkaOptions options, IMessageSerializer serializer)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(serializer);
        _options = options;
        _serializer = serializer;

        var config = new ProducerConfig
        {
            BootstrapServers = options.BootstrapServers,
            ClientId = options.ClientId,
        };
        _producer = new ProducerBuilder<string, string>(config).Build();
    }

    /// <inheritdoc/>
    public async Task PublishAsync<TMessage>(
        string topic,
        TMessage message,
        PlatformMetadata? metadata = null,
        CancellationToken ct = default)
        where TMessage : class
    {
        ArgumentNullException.ThrowIfNull(message);
        var fullTopic = string.IsNullOrEmpty(_options.TopicPrefix)
            ? topic
            : $"{_options.TopicPrefix}{topic}";

        var envelope = new Envelope.MessageEnvelope<TMessage>
        {
            Metadata = metadata ?? new PlatformMetadata(),
            Payload = message,
        };

        var json = _serializer.Serialize(envelope);
        var msg = new Message<string, string>
        {
            Key = envelope.Metadata.MessageId,
            Value = json,
        };

        await _producer.ProduceAsync(fullTopic, msg, ct);
    }

    /// <inheritdoc/>
    public void Dispose() => _producer.Dispose();
}
