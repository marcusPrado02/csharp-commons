using MarcusPrado.Platform.Messaging.Serialization;

namespace MarcusPrado.Platform.Kafka.Serialization;

/// <summary>Kafka-specific wrapper over <see cref="IMessageSerializer"/>.</summary>
public sealed class KafkaMessageSerializer : IMessageSerializer
{
    private readonly JsonMessageSerializer _inner = new();

    /// <inheritdoc/>
    public string Serialize<T>(T message)
        where T : class => _inner.Serialize(message);

    /// <inheritdoc/>
    public T? Deserialize<T>(string data)
        where T : class => _inner.Deserialize<T>(data);
}
