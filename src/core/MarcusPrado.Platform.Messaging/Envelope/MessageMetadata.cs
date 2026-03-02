namespace MarcusPrado.Platform.Messaging.Envelope;

/// <summary>Headers / metadata attached to every message in transit.</summary>
public sealed class MessageMetadata
{
    /// <summary>Unique identifier for this message occurrence.</summary>
    public string MessageId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>Correlation identifier for distributed tracing.</summary>
    public string? CorrelationId { get; set; }

    /// <summary>Originating tenant identifier.</summary>
    public string? TenantId { get; set; }

    /// <summary>Schema version of the payload.</summary>
    public string SchemaVersion { get; set; } = "1";

    /// <summary>Timestamp when the message was produced.</summary>
    public DateTimeOffset ProducedAt { get; set; } = DateTimeOffset.UtcNow;
}
