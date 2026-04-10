# Messaging Extensions

> `MarcusPrado.Platform.Kafka` · `MarcusPrado.Platform.RabbitMq` · `MarcusPrado.Platform.Nats` · `MarcusPrado.Platform.AzureServiceBus` · `MarcusPrado.Platform.AwsSqs` · `MarcusPrado.Platform.DlqReprocessing`

Concrete broker adapters that implement `IMessagePublisher` and `IMessageConsumer` from the core messaging abstractions. Swap brokers by changing a single DI registration — consumer code is broker-agnostic.

## Install

```bash
# Pick your broker(s)
dotnet add package MarcusPrado.Platform.Kafka
dotnet add package MarcusPrado.Platform.RabbitMq
dotnet add package MarcusPrado.Platform.Nats
dotnet add package MarcusPrado.Platform.AzureServiceBus
dotnet add package MarcusPrado.Platform.AwsSqs

# DLQ management API
dotnet add package MarcusPrado.Platform.DlqReprocessing
```

## Kafka

```csharp
// Registration
builder.Services.AddPlatformKafka(options =>
{
    options.BootstrapServers = "kafka:9092";
    options.ClientId         = "order-service";
    options.ConsumerGroupId  = "order-service-group";
    options.TopicPrefix      = "prod."; // optional namespace prefix
});

// Producer — inject IMessagePublisher anywhere
await publisher.PublishAsync("orders.created", new OrderCreatedMessage(orderId), ct: ct);

// Consumer — extend KafkaConsumer<T>, registers as BackgroundService
public sealed class OrderCreatedConsumer : KafkaConsumer<OrderCreatedMessage>
{
    public override string Topic => "orders.created";

    public override Task HandleAsync(
        MessageEnvelope<OrderCreatedMessage> envelope, CancellationToken ct)
        => _projection.ApplyAsync(envelope.Message, ct);
}
```

## RabbitMQ

```csharp
builder.Services.AddPlatformRabbitMq(options =>
{
    options.Host         = "rabbitmq";
    options.VirtualHost  = "/";
    options.Username     = "guest";
    options.Password     = builder.Configuration["RabbitMq:Password"];
    options.ExchangeName = "platform.events";
});

// Consumer — same pattern as Kafka, extend RabbitConsumer<T>
public sealed class PaymentProcessedConsumer : RabbitConsumer<PaymentProcessedMessage>
{
    public override string Topic => "payments.processed";
    // ...
}
```

## NATS (JetStream)

```csharp
builder.Services.AddPlatformNats(options =>
{
    options.Url             = "nats://nats:4222";
    options.StreamName      = "PLATFORM_EVENTS";
    options.ConsumerDurable = "order-service";
});
// At-least-once delivery via JetStream. Consumer extends NatsConsumer<T>.
```

## Azure Service Bus

```csharp
builder.Services.AddPlatformAzureServiceBus(options =>
{
    options.ConnectionString = builder.Configuration["AzureServiceBus:ConnectionString"];
    // or use DefaultAzureCredential:
    options.FullyQualifiedNamespace = "myns.servicebus.windows.net";
    options.UseDefaultAzureCredential = true;
});
// Consumer extends ServiceBusConsumer<T>. Lock renewal is automatic.
```

## AWS SQS / SNS

```csharp
builder.Services.AddPlatformAwsSqs(options =>
{
    options.Region   = "us-east-1";
    options.QueueUrl = "https://sqs.us-east-1.amazonaws.com/123/orders";
});

// Fan-out: SNS → SQS
builder.Services.AddPlatformAwsSns(options =>
{
    options.TopicArn = "arn:aws:sns:us-east-1:123:platform-events";
});
```

## DLQ Reprocessing

`DlqReprocessing` exposes a Minimal API for inspecting and reprocessing dead-letter messages, backed by `IDlqStore` and emitting OTel metrics.

```csharp
// Program.cs
builder.Services.AddPlatformDlqReprocessing();
app.MapPlatformDlqEndpoints(); // mounts under /dlq

// Endpoints:
// GET  /dlq/{topic}                        — list dead-letter messages
// POST /dlq/{topic}/reprocess/{messageId}  — requeue a single message
// DELETE /dlq/{topic}/{messageId}          — discard a dead-letter message
```

OTel metrics exposed:
- `platform.dlq.depth` — current number of dead-letter messages per topic
- `platform.dlq.reprocessed_total` — messages successfully requeued
- `platform.dlq.discarded_total` — messages explicitly discarded

## Key Types

| Type | Package | Purpose |
|------|---------|---------|
| `KafkaProducer` | Kafka | `IMessagePublisher` for Kafka |
| `KafkaConsumer<T>` | Kafka | Abstract BackgroundService consumer base |
| `RabbitProducer` | RabbitMq | `IMessagePublisher` for RabbitMQ |
| `RabbitConsumer<T>` | RabbitMq | Abstract BackgroundService consumer base |
| `NatsPublisher` | Nats | `IMessagePublisher` for NATS JetStream |
| `NatsConsumer<T>` | Nats | Abstract BackgroundService consumer base |
| `ServiceBusPublisher` | AzureServiceBus | `IMessagePublisher` for Azure Service Bus |
| `ServiceBusConsumer<T>` | AzureServiceBus | Consumer with automatic lock renewal |
| `ServiceBusDeadLetterSink` | AzureServiceBus | Routes failures to Service Bus DLQ |
| `SqsPublisher` | AwsSqs | `IMessagePublisher` for SQS |
| `SqsConsumer<T>` | AwsSqs | Long-polling consumer with automatic DLQ |
| `SnsPublisher` | AwsSqs | SNS fan-out publisher |
| `IDlqStore` | DlqReprocessing | Dead-letter message storage |
| `DlqReprocessingJob` | DlqReprocessing | Scheduled reprocessing job |

## Source

- [`src/extensions/MarcusPrado.Platform.Kafka`](../../src/extensions/MarcusPrado.Platform.Kafka)
- [`src/extensions/MarcusPrado.Platform.RabbitMq`](../../src/extensions/MarcusPrado.Platform.RabbitMq)
- [`src/extensions/MarcusPrado.Platform.Nats`](../../src/extensions/MarcusPrado.Platform.Nats)
- [`src/extensions/MarcusPrado.Platform.AzureServiceBus`](../../src/extensions/MarcusPrado.Platform.AzureServiceBus)
- [`src/extensions/MarcusPrado.Platform.AwsSqs`](../../src/extensions/MarcusPrado.Platform.AwsSqs)
- [`src/extensions/MarcusPrado.Platform.DlqReprocessing`](../../src/extensions/MarcusPrado.Platform.DlqReprocessing)
