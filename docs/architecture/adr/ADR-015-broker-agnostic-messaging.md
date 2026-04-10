# ADR-015 — Broker-Agnostic Messaging via IMessagePublisher / IMessageConsumer

| Field | Value |
|-------|-------|
| **Status** | Accepted |
| **Date** | 2026-03-01 |
| **Deciders** | Marcus Prado Silva |

---

## Context

Platform services need to publish and consume messages across multiple contexts: production services using Kafka, worker services using RabbitMQ, cloud deployments using Azure Service Bus or AWS SQS, and tests using in-memory fakes. Without abstraction, each service must import the Confluent Kafka SDK, configure it, handle serialization, and implement error handling — coupling business logic to a specific broker.

Alternative approaches considered:

1. **Direct SDK usage** — `IProducer<Null, byte[]>` (Confluent) or `ServiceBusSender` (Azure) injected directly. Maximum control; no abstraction overhead. Results in broker lock-in and untestable application code.
2. **NServiceBus / MassTransit** — mature message bus frameworks with pluggable transports. Adds significant framework overhead, a licensing consideration (NServiceBus), and opinionated conventions that constrain message schema design.
3. **Custom thin abstractions** — `IMessagePublisher` / `IMessageConsumer` interfaces in the platform core, with broker-specific adapters in separate NuGet packages.

---

## Decision

Use **custom thin abstractions** in `MarcusPrado.Platform.Messaging`.

- `IMessagePublisher` has a single method: `PublishAsync<T>(topic, message, metadata?, ct)`.
- Consumer logic extends a broker-specific abstract base class (`KafkaConsumer<T>`, `RabbitConsumer<T>`, etc.) that handles connection management, serialization, and error handling, and calls `HandleAsync(envelope, ct)` — the only method the developer writes.
- `MessageEnvelope<T>` is broker-neutral, carrying the payload plus `MessageMetadata` (correlation ID, tenant ID, schema version, timestamp).

Switching from Kafka to RabbitMQ requires changing one DI registration (`AddPlatformKafka()` → `AddPlatformRabbitMq()`) and one base class per consumer. Message handler logic is untouched.

---

## Consequences

**Positive:**
- Handler code is pure business logic — no broker imports, no serialization code, no connection management.
- Unit tests inject `FakeMessagePublisher` (from TestKit) — no broker container needed for handler tests.
- New brokers (NATS, AWS SQS) are added as separate NuGet packages without touching core or any existing consumer.

**Negative:**
- The abstraction exposes the least common denominator. Broker-specific features (Kafka compaction, RabbitMQ exchange types, Service Bus sessions) require stepping outside the abstraction.
- `MessageMetadata` must be able to represent headers from all brokers — currently maps to Kafka headers, RabbitMQ basic properties, and NATS header entries. Edge cases (binary header values in Kafka vs. string-only in NATS) are handled by the adapter layer.

**Neutral:**
- `JsonMessageSerializer` is the default serializer (System.Text.Json). A custom `IMessageSerializer` can be registered for Avro, Protobuf, or MessagePack with a single DI override.
