# Layer Dependency Rules

> These rules are the authoritative source of truth for allowed dependencies
> between projects in `MarcusPrado.Platform.Commons`.
>
> They are enforced at three levels:
> 1. **Build time** — Roslyn analyzers in `MarcusPrado.Platform.Analyzers`
> 2. **Test time** — NetArchTest rules in `MarcusPrado.Platform.ArchTests`
> 3. **Code review** — this document as a shared reference

---

## 1. Abstractions

**Project**: `src/core/MarcusPrado.Platform.Abstractions`

| Rule | Description |
|------|-------------|
| `ABS-01` | Must not reference any other platform project |
| `ABS-02` | Must not reference any NuGet package except the BCL (`Microsoft.NETCore.App`) |
| `ABS-03` | Must not reference EF Core, ASP.NET Core, or any infrastructure library |

**Rationale**: Abstractions is the bedrock. It defines the contracts (`IClock`,
`ICommandBus`, `IRepository`, etc.) that everything else depends on. A circular
dependency here would break the entire dependency graph.

---

## 2. Domain

**Project**: `src/core/MarcusPrado.Platform.Domain`

| Rule | Description |
|------|-------------|
| `DOM-01` | May only reference `Abstractions` within the platform |
| `DOM-02` | Must not reference EF Core (`Microsoft.EntityFrameworkCore`) |
| `DOM-03` | Must not reference ASP.NET Core (`Microsoft.AspNetCore`) |
| `DOM-04` | Must not reference any messaging broker SDK (Kafka, RabbitMQ, etc.) |
| `DOM-05` | All domain exceptions must inherit from `DomainException` |
| `DOM-06` | `IRepository<T>` may be used (via `Abstractions`) but never instantiated in Domain |

**Rationale**: The domain model must be pure. It expresses business rules without
knowing how data is stored, how HTTP requests arrive, or how messages are queued.
Keeping Domain clean makes unit testing fast (no stubs, no containers).

---

## 3. Application

**Project**: `src/core/MarcusPrado.Platform.Application`

| Rule | Description |
|------|-------------|
| `APP-01` | May reference `Abstractions` and `Domain` |
| `APP-02` | Must not reference Extensions |
| `APP-03` | Must not reference EF Core, ASP.NET Core, or infrastructure SDKs |
| `APP-04` | All commands must implement `ICommand` or `ICommand<TResult>` |
| `APP-05` | All queries must implement `IQuery<TResult>` |
| `APP-06` | All handlers must be registered through `ICommandBus` / `IQueryBus`, never called directly |
| `APP-07` | Application exceptions (`NotFoundException`, `ConflictException`, etc.) must inherit `AppException` |

**Rationale**: Application orchestrates domain objects and calls infrastructure
abstractions. It must not know whether storage is Postgres or MongoDB — that is
the Extensions layer's concern.

---

## 4. Other Core Packages

**Projects**: `Runtime`, `MultiTenancy`, `Security`, `Observability`, `Resilience`,
`Messaging`, `Persistence`, `OutboxInbox`, `FeatureFlags`, `RateLimiting`,
`BackgroundJobs`, `Governance`, `Contracts`

| Rule | Description |
|------|-------------|
| `CORE-01` | May reference `Abstractions`; may reference `Domain` where domain types are needed |
| `CORE-02` | Must not reference infrastructure SDKs (EF Core, Kafka, RabbitMQ drivers, etc.) |
| `CORE-03` | Must not reference each other in a circular fashion |
| `CORE-04` | Must not reference `Application` (Application is the consumer of Core, not the other way around) |

---

## 5. Extensions

**Projects**: `AspNetCore`, `AspNetCore.Auth`, `AspNetCore.ProblemDetails`,
`EfCore`, `Postgres`, `Redis`, `Kafka`, `RabbitMq`, `OpenTelemetry`,
`Serilog`, `HealthChecks`

| Rule | Description |
|------|-------------|
| `EXT-01` | May reference Core projects (`Abstractions`, `Domain`, `Application`, and other Core packages) |
| `EXT-02` | **Must not** reference other Extension projects directly |
| `EXT-03` | May reference third-party infrastructure packages (EF Core, Confluent.Kafka, StackExchange.Redis, etc.) |
| `EXT-04` | Must provide implementations of Core interfaces — not new abstractions |
| `EXT-05` | `MarcusPrado.Platform.EfCore` is the **only** project allowed to reference `Microsoft.EntityFrameworkCore` |

**Rationale (EXT-02)**: Extension inter-dependencies create a dependency tangle.
If `AspNetCore.Auth` references `Redis`, teams adopting Auth must take Redis.
Composition happens at the service (Samples) level via `AddPlatformCore()`,
`AddPlatformCqrs()`, `AddPlatformAuth()`, etc.

---

## 6. Kits (TestKit, ContractTestKit, ChaosKit, ObservabilityTestKit)

| Rule | Description |
|------|-------------|
| `KIT-01` | May reference Core and Extension projects |
| `KIT-02` | Must not be referenced by Core or Extension projects |
| `KIT-03` | Must be marked `IsPackable=false` or shipped only to a private feed |
| `KIT-04` | May reference TestContainers, xUnit, FluentAssertions, Pact |

---

## 7. Tooling (Analyzers, ArchTests)

| Rule | Description |
|------|-------------|
| `TOOL-01` | `MarcusPrado.Platform.Analyzers` targets `netstandard2.0` and must not reference Core/Extension projects at compile time |
| `TOOL-02` | `MarcusPrado.Platform.ArchTests` is a test project; it may reference all platform assemblies as subjects under test |
| `TOOL-03` | Analyzer packages ship with `PrivateAssets=all` so they do not appear in consumer NuGet dependency graphs |

---

## 8. Samples

| Rule | Description |
|------|-------------|
| `SAMP-01` | May reference any platform project |
| `SAMP-02` | Must never be referenced by any platform project |
| `SAMP-03` | Exist solely to demonstrate idiomatic platform usage |
| `SAMP-04` | Are marked `IsPackable=false` and are excluded from CI pack/publish stages |

---

## Enforcement Matrix

| Rule set | Roslyn Analyzer | ArchTest | CI stage |
|----------|:-:|:-:|:-:|
| EF Core in Core (`DOM-02`, `APP-03`, `CORE-02`) | `NoEfCoreInDomainAnalyzer` (MP0001) | `LayeringRules` | `build` + `test` |
| ASP.NET Core in Core (`DOM-03`, `APP-03`) | `NoAspNetInDomainAnalyzer` (MP0002) | `LayeringRules` | `build` + `test` |
| Infrastructure in Domain | `DomainMustNotReferenceInfrastructureAnalyzer` (MP0003) | `DomainDependencyRules` | `build` + `test` |
| Result type usage | `EnforceResultTypeAnalyzer` (MP0004) | — | `build` |
| Exception naming (`DOM-05`, `APP-07`) | — | `NamingConventionRules` | `test` |
| Extension circular deps (`EXT-02`) | — | `LayeringRules` | `test` |

---

## Visualised Dependency Flow

```
Abstractions
     ▲
     │
   Domain ──────────────────────────────────────────┐
     ▲                                               │
     │                                               │
 Application ◄── (all other Core packages) ◄────────┘
     ▲
     │
 Extensions  (each Extension depends on Core; never on another Extension)
     ▲
     │
 Kits / Samples  (may use everything)
```

See [overview.md](overview.md) for the full layer diagram.
