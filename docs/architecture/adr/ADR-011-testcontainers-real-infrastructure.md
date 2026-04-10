# ADR-011 — Testcontainers for Integration Tests (Real Infrastructure, Not Mocks)

| Field | Value |
|-------|-------|
| **Status** | Accepted |
| **Date** | 2026-03-01 |
| **Deciders** | Marcus Prado Silva |

---

## Context

Infrastructure code — EF Core queries, Redis commands, Kafka producers — can behave differently with a real provider than with a mock. Common failure modes hidden by mocks:

- EF Core query filters silently disabled by `IgnoreQueryFilters()`.
- Redis TTL behaviour differs from an in-memory dictionary.
- Kafka consumer group rebalancing under load.
- Postgres constraint violations on concurrent inserts.

The alternative to real containers is mocking: `Mock<ICache>`, `Mock<IMessagePublisher>`, etc. Mocks are fast and have no Docker dependency, but they verify that the code calls the right methods with the right arguments — not that the system actually works end-to-end.

---

## Decision

Integration tests use **real infrastructure via Testcontainers for .NET**.

`PlatformTestEnvironment` starts all required containers (PostgreSQL, Redis, Kafka, RabbitMQ, NATS, MongoDB) in parallel using `TestcontainersBuilder`. Each container is started once per test class collection and shared within the collection to avoid per-test startup cost.

`SnapshotRestorer` provides per-test isolation by wrapping each test in a transaction that is rolled back on completion (for PostgreSQL) or by resetting Redis keys with a test-specific prefix.

Unit tests (pure domain logic, pipeline behaviors) continue to use mocks/fakes — only infrastructure adapters warrant integration tests.

---

## Consequences

**Positive:**
- Integration tests catch real-world failures that mocked tests miss: constraint violations, serialization edge cases, index mismatches after migrations.
- Tests run against the same major version of each dependency as production.
- `PlatformTestEnvironment` is reusable across all services that consume this library.

**Negative:**
- Docker must be available on the CI runner. GitHub-hosted runners ship Docker by default.
- Test suite startup time increases by 10–30 seconds for container pull + startup. Mitigated by container reuse within test sessions and Ryuk cleanup.
- Flaky test risk increases slightly due to timing-sensitive operations (e.g., Kafka consumer ready signal). `Eventually` utility handles this with retries.

**Neutral:**
- In-memory implementations (`InMemoryEventStore`, `InMemoryOutboxStore`) remain available for unit tests where container startup would be disproportionate overhead.
