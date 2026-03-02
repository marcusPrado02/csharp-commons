namespace MarcusPrado.Platform.Kafka.Options;

/// <summary>Configuration for the Kafka producer and consumer.</summary>
public sealed class KafkaOptions
{
    /// <summary>Gets or sets the Kafka bootstrap server addresses.</summary>
    public string BootstrapServers { get; set; } = "localhost:9092";

    /// <summary>Gets or sets an optional topic prefix (e.g. "my-app.").</summary>
    public string TopicPrefix { get; set; } = string.Empty;

    /// <summary>Gets or sets the default consumer group ID.</summary>
    public string ConsumerGroupId { get; set; } = "platform-consumer-group";

    /// <summary>Gets or sets the maximum number of retries before sending to DLQ.</summary>
    public int MaxRetries { get; set; } = 3;

    /// <summary>Gets or sets the DLQ topic suffix.</summary>
    public string DlqSuffix { get; set; } = ".dlq";

    /// <summary>Gets or sets the client ID.</summary>
    public string ClientId { get; set; } = "platform-client";
}
