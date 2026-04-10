# Core Messaging

> `MarcusPrado.Platform.Messaging` · `MarcusPrado.Platform.OutboxInbox`

Broker-agnostic messaging contracts and the Outbox/Inbox pattern for guaranteed at-least-once delivery. The abstractions decouple business logic from Kafka, RabbitMQ, NATS, Azure Service Bus, or AWS SQS — switch brokers by swapping a single DI registration.

## Install

```bash
dotnet add package MarcusPrado.Platform.Messaging
dotnet add package MarcusPrado.Platform.OutboxInbox
```

## Publishing Messages

```csharp
// Define a message contract
public record OrderShippedMessage(OrderId OrderId, DateTimeOffset ShippedAt)
    : IEventContract;

// Publish via IMessagePublisher (injected from DI)
public class ShippingService(IMessagePublisher publisher)
{
    public async Task MarkShippedAsync(OrderId orderId, CancellationToken ct)
    {
        var message = new OrderShippedMessage(orderId, DateTimeOffset.UtcNow);
        await publisher.PublishAsync("orders.shipped", message, ct: ct);
    }
}
```

## Consuming Messages

```csharp
// Consumer — extend KafkaConsumer<T>, RabbitConsumer<T>, etc.
// The abstract base registers itself as a BackgroundService automatically.
public sealed class OrderShippedConsumer(IOrderProjection projection)
    : KafkaConsumer<OrderShippedMessage>  // or RabbitConsumer, NatsConsumer...
{
    public override string Topic => "orders.shipped";

    public override async Task HandleAsync(
        MessageEnvelope<OrderShippedMessage> envelope, CancellationToken ct)
    {
        await projection.UpdateStatusAsync(
            envelope.Message.OrderId,
            OrderStatus.Shipped,
            ct);
    }
}
```

## Outbox Pattern (Transactional Messaging)

The Outbox pattern guarantees a message is published if and only if the database transaction commits. `OutboxProcessor` polls the outbox table and publishes pending messages via `IMessagePublisher`.

```csharp
// 1. Save business state + outbox row in the SAME transaction
public class PlaceOrderHandler(AppDbContext db, IOutboxStore outbox)
{
    public async Task<Result<OrderId>> HandleAsync(PlaceOrderCommand cmd, CancellationToken ct)
    {
        var order = Order.Create(cmd.CustomerId, cmd.Lines).Value;
        db.Orders.Add(order);

        // Enqueue for transactional dispatch
        await outbox.SaveAsync(new OutboxMessage
        {
            Topic   = "orders.created",
            Payload = JsonSerializer.Serialize(new OrderCreatedMessage(order.Id)),
        }, ct);

        await db.SaveChangesAsync(ct); // both rows committed atomically
        return order.Id;
    }
}

// 2. Register the outbox processor in Program.cs
builder.Services.AddInMemoryOutboxInbox();  // dev / tests
// or: builder.Services.AddEfCoreOutboxInbox();  // production (PostgreSQL)
```

## Inbox Pattern (Idempotent Consumers)

`InboxProcessor` deduplicates incoming messages using a unique `MessageId`. Duplicate deliveries (common in at-least-once brokers) are silently ignored.

```csharp
// The KafkaConsumer / RabbitConsumer base classes check the inbox automatically.
// No additional code needed in the handler.
```

## Dead-Letter Queue

```csharp
// Implement IDeadLetterSink to route failed messages
public class AlertingDeadLetterSink(IAlertService alerts) : IDeadLetterSink
{
    public async Task SendAsync(DeadLetterMessage message, CancellationToken ct)
    {
        await alerts.SendCriticalAlertAsync(
            $"Message {message.MessageId} on {message.Topic} failed {message.RetryCount} times.",
            ct);
    }
}
```

## Key Types

| Type | Package | Purpose |
|------|---------|---------|
| `IMessagePublisher` | Messaging | `PublishAsync<T>(topic, message, metadata?, ct)` |
| `IMessageConsumer` | Messaging | Marker interface; `Topic` property for subscription |
| `IMessageHandler<T>` | Messaging | `HandleAsync(envelope, ct)` — implement per message type |
| `MessageEnvelope<T>` | Messaging | Wraps payload with `MessageMetadata` (headers, messageId, timestamp) |
| `MessageMetadata` | Messaging | CorrelationId, TenantId, SchemaVersion, custom headers |
| `IOutboxStore` | OutboxInbox | Save / GetPending / MarkPublished / MarkFailed |
| `IInboxStore` | OutboxInbox | Deduplication store |
| `OutboxProcessor` | OutboxInbox | `IHostedService` that polls and publishes pending outbox rows |
| `InboxProcessor` | OutboxInbox | `IHostedService` that records processed message IDs |
| `IDeadLetterSink` | Messaging | Receives messages after exhausting retries |
| `DLQReprocessor` | Messaging | Requeues dead-letter messages for reprocessing |

## Source

- [`src/core/MarcusPrado.Platform.Messaging`](../../src/core/MarcusPrado.Platform.Messaging)
- [`src/core/MarcusPrado.Platform.OutboxInbox`](../../src/core/MarcusPrado.Platform.OutboxInbox)
