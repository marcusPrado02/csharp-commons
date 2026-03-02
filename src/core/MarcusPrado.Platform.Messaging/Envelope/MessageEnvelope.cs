namespace MarcusPrado.Platform.Messaging.Envelope;

/// <summary>Wraps a typed message payload with its transport metadata.</summary>
public sealed class MessageEnvelope<TPayload>
    where TPayload : class
{
    /// <summary>Gets the message metadata (headers).</summary>
    public MessageMetadata Metadata { get; init; } = new();

    /// <summary>Gets the message payload.</summary>
    public TPayload Payload { get; init; } = default!;
}

/// <summary>Non-generic envelope for deserialisation scenarios.</summary>
public sealed class MessageEnvelope
{
    /// <summary>Gets the message metadata (headers).</summary>
    public MessageMetadata Metadata { get; init; } = new();

    /// <summary>Gets the raw JSON payload.</summary>
    public string Payload { get; init; } = string.Empty;

    /// <summary>Gets the fully-qualified CLR type name of the payload.</summary>
    public string PayloadType { get; init; } = string.Empty;
}
