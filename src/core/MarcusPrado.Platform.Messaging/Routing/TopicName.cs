namespace MarcusPrado.Platform.Messaging.Routing;

/// <summary>Value object representing a topic, exchange, or queue name.</summary>
public sealed record TopicName
{
    /// <summary>Gets the raw topic name string.</summary>
    public string Value { get; }

    /// <summary>Initialises a new <see cref="TopicName"/> with the given value.</summary>
    public TopicName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Topic name must not be empty.", nameof(value));
        }

        Value = value;
    }

    /// <inheritdoc/>
    public override string ToString() => Value;

    /// <summary>Implicit conversion from <see cref="string"/>.</summary>
    public static implicit operator string(TopicName name) => name.Value;

    /// <summary>Explicit conversion from <see cref="string"/>.</summary>
    public static explicit operator TopicName(string value) => new(value);
}
