namespace MarcusPrado.Platform.Messaging.Routing;

/// <summary>Value object representing a Kafka consumer group or RabbitMQ binding key.</summary>
public sealed record ConsumerGroup
{
    /// <summary>Gets the raw consumer group name.</summary>
    public string Value { get; }

    /// <summary>Initialises a new <see cref="ConsumerGroup"/> with the given value.</summary>
    public ConsumerGroup(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Consumer group must not be empty.", nameof(value));
        }

        Value = value;
    }

    /// <inheritdoc/>
    public override string ToString() => Value;

    /// <summary>Implicit conversion to <see cref="string"/>.</summary>
    public static implicit operator string(ConsumerGroup group) => group.Value;
}
