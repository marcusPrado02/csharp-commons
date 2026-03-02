namespace MarcusPrado.Platform.OutboxInbox.Idempotency;

/// <summary>A value object identifying a unique idempotent operation.</summary>
public sealed record IdempotencyKey
{
    /// <summary>Gets the raw string representation of the key.</summary>
    public string Value { get; }

    /// <summary>Initialises a new <see cref="IdempotencyKey"/> with the given value.</summary>
    public IdempotencyKey(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Idempotency key must not be empty.", nameof(value));
        }

        Value = value;
    }

    /// <summary>Creates a key from a message identifier.</summary>
    public static IdempotencyKey FromMessageId(string messageId) => new($"msg:{messageId}");

    /// <summary>Creates a key from an operation name and a unique identifier.</summary>
    public static IdempotencyKey FromOperation(string operation, string id) => new($"{operation}:{id}");

    /// <inheritdoc/>
    public override string ToString() => Value;
}
