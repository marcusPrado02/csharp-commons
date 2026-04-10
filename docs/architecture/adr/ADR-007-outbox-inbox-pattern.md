# ADR-007 — Outbox/Inbox Pattern for At-Least-Once Messaging

| Field | Value |
|-------|-------|
| **Status** | Accepted |
| **Date** | 2026-03-01 |
| **Deciders** | Marcus Prado Silva |

---

## Context

Microservices that publish events after a database write face the dual-write problem: the write and the message publish are two separate I/O operations with no shared transaction boundary. If the process crashes between them, you either get a committed record with no event published, or an event published for a write that rolled back.

Options considered:

1. **Fire-and-forget publish after SaveChanges** — simple, but loses messages on crash or broker unavailability.
2. **Two-phase commit / XA transaction** — prevents loss but adds significant operational complexity and latency; most cloud brokers do not support XA.
3. **Transactional Outbox pattern** — write the event as a row in the same database transaction as the business record; a background processor reads pending rows and publishes them.
4. **Event-first / event-sourcing** — the event IS the record of truth; eliminates the dual-write at the cost of a more complex read model.

---

## Decision

Use the **Transactional Outbox pattern** with a companion **Inbox pattern** for consumers.

`AppDbContextBase` includes `OutboxMessages` and `InboxMessages` DbSets. `OutboxProcessor` (a `BackgroundService`) polls pending outbox rows and calls `IMessagePublisher`. `InboxProcessor` records processed `MessageId` values to deduplicate redeliveries.

The decision was made against Event Sourcing as the default because:
- Most services in the target use-case maintain relational state and benefit from SQL queries on current state.
- Outbox adds guaranteed delivery without requiring event replay on every read.

---

## Consequences

**Positive:**
- Messages are never lost even if the broker is temporarily unavailable.
- Consumers receive at-least-once delivery and can be made idempotent cheaply via the Inbox pattern.
- No dependency on broker-specific transactions or XA.

**Negative:**
- Adds two tables (`outbox_messages`, `inbox_messages`) to every service database.
- `OutboxProcessor` adds polling overhead; this is mitigated by adjustable polling intervals and exponential backoff on failures.
- Delivery order is approximate — the processor publishes in insertion order, but concurrent transactions can produce out-of-order rows.

**Neutral:**
- `EfCoreOutboxStore` and `EfCoreInboxStore` are the production implementations. An `InMemoryOutboxStore` is available for unit tests with no container requirement.
