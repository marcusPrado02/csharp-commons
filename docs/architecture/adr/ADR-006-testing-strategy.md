# ADR-006 — Testing strategy: layered test pyramid

> **Summary**: The platform adopts a four-layer test pyramid — unit, integration,
> contract, and architecture tests — each at a different scope and running at a
> different CI stage. Mutation testing (Stryker) runs on-demand to validate test
> suite quality. This strategy maximises feedback speed without sacrificing
> coverage of real infrastructure behaviour.

| Field | Value |
|-------|-------|
| **Status** | Accepted |
| **Date** | 2026-03-15 |
| **Author** | Marcus Prado Silva (Platform Architect) |
| **Tags** | testing, quality, ci-cd, tdd, test-pyramid |
| **Supersedes** | — |
| **Superseded by** | — |

---

## Context

A platform library has a different testing challenge than a single service:

- **Breadth** — ~70 packages, each with independent contracts. A bug in
  `MarcusPrado.Platform.Abstractions` can break every consumer simultaneously.
- **Infrastructure dependencies** — Extensions wrap real brokers (Kafka,
  RabbitMQ), databases (Postgres, MongoDB), and caches (Redis). Mocking these
  was the historical approach; it produced tests that passed in CI but failed
  in production when the real dependency behaved differently.
- **API compatibility** — consumers pin package versions. An undetected breaking
  change in a Core interface forces coordinated updates across all consuming
  services.
- **Layer rules** — the dependency constraints described in `layer-rules.md`
  must be verified mechanically, not just by convention.

### Why not mock the infrastructure?

Three production incidents drove the decision to use real infrastructure in
integration tests:

1. A mocked Redis `IDistributedCache` returned `null` for missing keys; the
   real `StackExchange.Redis` client raised `RedisServerException`. The mock
   test passed; the production deployment failed on first cache miss.
2. A mocked Kafka producer silently dropped messages with a payload >1 MB
   (the real broker limit). The mock had no size enforcement.
3. A mocked `DbContext` did not enforce unique constraints; a duplicate-insert
   bug reached production.

---

## Decision

The platform ships four test layers, each in a separate directory under
`tests/`:

```
tests/
  unit/           → Fast, pure logic tests. No I/O, no containers.
  integration/    → Real infrastructure via Testcontainers. Per-package scope.
  contract/       → Pact consumer/provider tests for message schemas.
  architecture/   → NetArchTest rules enforcing layer dependency constraints.
```

### Layer 1 — Unit tests

**Tool**: xUnit 2.9 + FluentAssertions 7 + NSubstitute 5 + Bogus

**Scope**: Pure domain and application logic. All infrastructure dependencies
are replaced with `NSubstitute` fakes or hand-written stubs.

**Target**: Every method in `Result<T>`, `ErrorCategory` mapping, pipeline
behavior logic, domain entity invariants, value objects.

```csharp
public sealed class CreateOrderHandlerTests
{
    [Fact]
    public async Task HandleAsync_EmptyLines_ReturnsValidationFailure()
    {
        var handler = new CreateOrderHandler(
            repository: Substitute.For<IOrderRepository>(),
            clock: new FakeClock());

        var result = await handler.HandleAsync(
            new CreateOrderCommand(CustomerId: Guid.NewGuid(), Lines: []),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Category.Should().Be(ErrorCategory.Validation);
        result.Error.Code.Should().Be("order.empty_lines");
    }
}
```

**CI stage**: `test` job (runs on every push and PR). Target: < 10 s total
for the unit test suite.

### Layer 2 — Integration tests

**Tool**: Testcontainers 3 (via `MarcusPrado.Platform.IntegrationTestEnvironment`)

**Scope**: Each Extension package is tested against a real containerised
infrastructure dependency. Tests verify the full round-trip: DI registration →
actual infrastructure call → response.

**No mocking of infrastructure** (see context above). Testcontainers spins up
the real Docker image in CI; tests are environment-agnostic.

```csharp
// Shared fixture spins up containers once per test class collection
[Collection(nameof(PostgresCollection))]
public sealed class EfRepositoryTests(PostgresFixture fixture)
{
    [Fact]
    public async Task AddAsync_ThenFindAsync_ReturnsPersistedEntity()
    {
        await using var ctx = fixture.CreateDbContext<OrderDbContext>();
        var repo = new EfRepository<Order, OrderId>(ctx);

        var order = Order.Create(CustomerId.New(), SampleLines()).Value;
        await repo.AddAsync(order, CancellationToken.None);
        await ctx.SaveChangesAsync();

        var found = await repo.FindAsync(order.Id, CancellationToken.None);
        found.Should().NotBeNull();
        found!.CustomerId.Should().Be(order.CustomerId);
    }
}
```

**CI stage**: `test` job (same as unit, but tagged `[Trait("Category", "Integration")]`
so they can be parallelised or filtered separately).

### Layer 3 — Contract tests

**Tool**: PactNet 5 (via `MarcusPrado.Platform.ContractTestKit`)

**Scope**: Message schema contracts between publishers and consumers.
Verifies that a message published by `MarcusPrado.Platform.Kafka` matches
the schema expected by a consuming service.

```
Consumer defines the contract:
  "When I receive an OrderCreatedEvent, it must have: orderId (Guid),
   customerId (Guid), totalAmount (decimal), createdAt (DateTimeOffset)"

Provider verifies:
  "My Kafka publisher for OrderCreatedEvent produces a message that
   satisfies the consumer's contract."
```

**CI stage**: `test` job. Pact broker is optional in CI; a file-based pact
works for the platform's own publisher/consumer pairs.

### Layer 4 — Architecture tests

**Tool**: NetArchTest 1.3 (via `MarcusPrado.Platform.ArchTests`)

**Scope**: Mechanically enforces the layer dependency rules from `layer-rules.md`
and `ADR-003`. Runs against compiled assemblies, not source.

```csharp
public sealed class LayeringTests
{
    [Fact]
    public void DomainMustNotReferenceEfCore() =>
        Types.InAssembly(typeof(Order).Assembly)
             .ShouldNot()
             .HaveDependencyOn("Microsoft.EntityFrameworkCore")
             .Because("ADR-003: EF Core belongs in Extensions, never Core")
             .Check();

    [Fact]
    public void ExtensionsMustNotReferenceEachOther() =>
        Types.InAssemblyMatching("MarcusPrado.Platform.*.dll")
             .That().ResideInNamespaceStartingWith("MarcusPrado.Platform")
             .And().AreNot().InAssembly(typeof(EfRepository<,>).Assembly)
             .ShouldNot()
             .HaveDependencyOn("MarcusPrado.Platform.EfCore")
             .Because("ADR layer rule EXT-02: Extensions must not reference each other")
             .Check();
}
```

**CI stage**: `test` job. Failures here are **build-breaking** — they indicate
a layer rule violation that must be fixed before merge.

### Mutation testing (on-demand)

**Tool**: Stryker.NET (via `mutation.yml` workflow — `workflow_dispatch`)

**Scope**: Core packages. Mutates production code and checks whether the test
suite detects the mutation (kills the mutant).

**Not blocking**: Mutation testing is slow (minutes per project). It runs
manually or on a weekly schedule to measure test suite health, not as a
merge gate. A mutation score badge (`>70 %`) is displayed in the README.

```bash
# Run locally for a specific package
cd src/core/MarcusPrado.Platform.Application
dotnet stryker
```

### Test stack rationale

| Concern | Tool | Why |
|---------|------|-----|
| Assertions | FluentAssertions 7 | Natural-language `.Should()` chains; specific failure messages; broad type support |
| Mocking | NSubstitute 5 | Source-generator-based (no `Castle.DynamicProxy` at runtime); cleaner syntax than Moq |
| Test data | Bogus | Fluent fake data builders; reproducible seeds for regression tests |
| Containers | Testcontainers | Official .NET SDK; parallel container startup; automatic cleanup |
| Contracts | PactNet 5 | Industry-standard consumer-driven contracts; CI-compatible file-based pacts |
| Architecture | NetArchTest | Declarative, readable rules; runs on compiled assemblies |
| Mutation | Stryker.NET | Best-in-class for .NET; HTML report; configurable thresholds |

---

## Consequences

### Positive

- **Infrastructure bugs caught before production** — three classes of bugs
  (cache miss behaviour, broker limits, DB constraint enforcement) that
  previously reached production are now caught in integration tests.
- **Layer rules enforced mechanically** — architecture test failures break the
  build; no dependency on code-review vigilance.
- **Fast feedback loop** — unit tests run in < 10 s; the full `test` CI job
  (unit + integration + contract + architecture) completes in < 3 min with
  Testcontainers parallel startup.
- **Mutation score as a quality gate** — the `>70 %` threshold in the badge
  prevents hollow coverage numbers (tests that cover lines but make no
  assertions).

### Negative / Trade-offs

- **Docker required for integration tests** — developers without Docker Desktop
  (or Podman) cannot run integration tests locally. CI always has Docker.
- **Testcontainers pull time** — first run pulls Docker images; subsequent
  runs use the local image cache. CI layer caching mitigates this.
- **Pact contract overhead** — maintaining consumer contracts requires
  discipline; stale contracts can block providers. The file-based approach
  (no Pact Broker) reduces this overhead for the platform's internal use.
- **Stryker runtime** — full mutation run on `Application` takes ~4 min.
  On-demand only mitigates blocking, but mutation debt can accumulate if
  runs are infrequent.

---

## Alternatives Considered

| Alternative | Reason rejected |
|-------------|-----------------|
| Mock all infrastructure in integration tests | Three production incidents (see Context) demonstrate that mock/real divergence is a meaningful risk |
| Moq instead of NSubstitute | NSubstitute's source-generator approach removes the `Castle.DynamicProxy` dependency; cleaner syntax for `Arg.Any<>` verification |
| WireMock for HTTP integration tests | Appropriate for service-to-service tests; for platform extension tests, Testcontainers with the real broker is more accurate |
| SpecFlow / Cucumber for BDD | Overhead of `.feature` file maintenance exceeds the value for a platform library without end-user stories |
| Mutation testing as a CI gate | Too slow to be a merge gate at current project scale; more valuable as a periodic health metric |

---

## References

- [The Practical Test Pyramid](https://martinfowler.com/articles/practical-test-pyramid.html) — Martin Fowler
- [Consumer-Driven Contracts](https://martinfowler.com/articles/consumerDrivenContracts.html) — Martin Fowler
- [Testcontainers for .NET](https://dotnet.testcontainers.org/)
- [PactNet 5](https://github.com/pact-foundation/pact-net)
- [NetArchTest](https://github.com/BenMorris/NetArchTest)
- [Stryker.NET](https://stryker-mutator.io/docs/stryker-net/introduction/)
- ADR-003 — layer rules enforced by architecture tests in this layer
- `tests/unit/` — unit test suites
- `tests/integration/` — Testcontainers-based integration suites
- `tests/contract/` — Pact consumer/provider tests
- `tests/architecture/` — NetArchTest architecture test suite
- `.github/workflows/mutation.yml` — on-demand Stryker workflow
- `src/kits/MarcusPrado.Platform.IntegrationTestEnvironment/` — Testcontainers fixtures
- `src/kits/MarcusPrado.Platform.ContractTestKit/` — Pact helpers
