namespace MarcusPrado.Platform.RabbitMq.Options;

/// <summary>Configuration for the RabbitMQ producer and consumer.</summary>
public sealed class RabbitMqOptions
{
    /// <summary>Gets or sets the AMQP connection URI.</summary>
    public string ConnectionString { get; set; } = "amqp://guest:guest@localhost:5672/";

    /// <summary>Gets or sets the exchange name for outbound messages.</summary>
    public string Exchange { get; set; } = "platform.events";

    /// <summary>Gets or sets the exchange type (direct, topic, fanout).</summary>
    public string ExchangeType { get; set; } = "topic";

    /// <summary>Gets or sets the maximum consumer prefetch count.</summary>
    public ushort PrefetchCount { get; set; } = 10;

    /// <summary>Gets or sets the DLQ exchange name.</summary>
    public string DlqExchange { get; set; } = "platform.dlq";

    /// <summary>Gets or sets the maximum number of retries before DLQ.</summary>
    public int MaxRetries { get; set; } = 3;
}
