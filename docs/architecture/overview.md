# Architecture Overview — MarcusPrado Platform Commons

> **Version**: 1.0
> **Date**: 2026-03-01
> **Status**: Active

## Purpose

`MarcusPrado.Platform.Commons` is a staff-engineer-grade reusable platform library for .NET 10+ microservices. It provides all the cross-cutting concerns (error handling, domain primitives, CQRS pipeline, messaging abstractions, observability, resilience, multi-tenancy, etc.) that every service team would otherwise reinvent.

The library ships as a set of independently installable NuGet packages so teams can adopt exactly the pieces they need without pulling in unrelated dependencies.

---

## Layer Diagram

```
┌──────────────────────────────────────────────────────────────────────────────┐
│  SAMPLES  (src/samples/)                                                     │
│  Sample.Service.MinimalApi  ·  Sample.Service.Worker                         │
│  → Reference Extensions + Core; demonstrate idiomatic usage                 │
└──────────────────────────┬───────────────────────────────────────────────────┘
                           │ depends on
┌──────────────────────────▼───────────────────────────────────────────────────┐
│  KITS  (src/kits/)                                                           │
│  TestKit  ·  ContractTestKit  ·  ChaosKit  ·  ObservabilityTestKit          │
│  → Test helpers built on top of Extensions and Core                          │
└──────────────────────────┬───────────────────────────────────────────────────┘
                           │ depends on
┌──────────────────────────▼───────────────────────────────────────────────────┐
│  EXTENSIONS  (src/extensions/)                                               │
│  AspNetCore  ·  AspNetCore.Auth  ·  AspNetCore.ProblemDetails                │
│  EfCore  ·  Postgres  ·  Redis  ·  Kafka  ·  RabbitMq                       │
│  OpenTelemetry  ·  Serilog  ·  HealthChecks                                 │
│  → Framework/infrastructure adapters; depend on Core only                   │
└──────────────────────────┬───────────────────────────────────────────────────┘
                           │ depends on
┌──────────────────────────▼───────────────────────────────────────────────────┐
│  CORE  (src/core/)                                                           │
│  Abstractions  ·  Domain  ·  Application  ·  Contracts                      │
│  Runtime  ·  MultiTenancy  ·  Security  ·  Observability                    │
│  Resilience  ·  Messaging  ·  Persistence  ·  OutboxInbox                   │
│  FeatureFlags  ·  RateLimiting  ·  BackgroundJobs  ·  Governance            │
│  → Pure .NET; zero infrastructure dependencies                               │
│                                                                              │
│  Dependency order within Core:                                               │
│    Abstractions  ←  Domain  ←  Application  ←  (all other core packages)    │
└──────────────────────────────────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────────────────────────────────┐
│  TOOLING  (src/tooling/)                                                     │
│  Analyzers (netstandard2.0)  ·  ArchTests                                   │
│  → Roslyn analyzers + NetArchTest rules; enforce the layer rules above       │
└──────────────────────────────────────────────────────────────────────────────┘
```

### Dependency rules (enforced)

| From \ To     | Abstractions | Domain | Application | Extensions | Kits | Samples |
|---------------|:---:|:---:|:---:|:---:|:---:|:---:|
| Abstractions  | —   | ✗   | ✗   | ✗   | ✗   | ✗   |
| Domain        | ✔   | —   | ✗   | ✗   | ✗   | ✗   |
| Application   | ✔   | ✔   | —   | ✗   | ✗   | ✗   |
| Extensions    | ✔   | ✔   | ✔   | ✗¹  | ✗   | ✗   |
| Kits          | ✔   | ✔   | ✔   | ✔   | —   | ✗   |
| Samples       | ✔   | ✔   | ✔   | ✔   | ✔   | —   |

¹ Extensions must not depend on each other directly; compose at the Samples/service level.

Full rules with rationale: [layer-rules.md](layer-rules.md)

---

## Solution Structure

```
MarcusPrado.Platform.Commons.slnx
├── src/
│   ├── core/           16 projects — pure .NET, no infrastructure
│   ├── extensions/     11 projects — framework & infra adapters
│   ├── kits/            4 projects — testing helpers
│   ├── samples/         2 projects — reference services
│   └── tooling/         2 projects — analyzers + arch-tests
└── tests/
    ├── unit/            per-project unit test suites (xUnit + FluentAssertions)
    ├── integration/     TestContainers-based integration tests
    ├── contract/        Pact contract tests
    └── architecture/    NetArchTest suite
```

---

## Key Design Decisions

See the [ADR index](adr/README.md) for the full list. Key decisions:

| ADR | Decision |
|-----|----------|
| [ADR-001](adr/ADR-001-result-type.md) | Use `Result<T>` instead of exceptions for expected failure paths |
| [ADR-002](adr/ADR-002-no-mediatr.md) | Own CQRS pipeline instead of MediatR |
| [ADR-003](adr/ADR-003-efcore-in-extension.md) | EF Core belongs in Extensions, never in Core |
| [ADR-004](adr/ADR-004-otel-semantic-conventions.md) | OpenTelemetry Semantic Conventions for all telemetry |
| [ADR-005](adr/ADR-005-versioning-strategy.md) | MinVer + Central Package Management for versioning |
| [ADR-006](adr/ADR-006-testing-strategy.md) | Layered test pyramid: unit / integration / contract / architecture |

---

## Target Framework & Build

| Concern | Value |
|---------|-------|
| Runtime | .NET 10 (`net10.0`) |
| Analyzers project | `netstandard2.0` |
| Language | C# 13, `LangVersion=latest`, `Nullable=enable` |
| Build gate | `TreatWarningsAsErrors=true` for all `src/core/`, `src/extensions/`, `src/kits/` |
| Formatter | CSharpier 1.2.6 (`dotnet csharpier check .`) |
| Style | Roslyn `.editorconfig` (SA\*, CA\*, S\*, RCS\*) |
| SDK pin | `global.json` → 10.0.103, rollForward=latestPatch |
| Package versions | Central Package Management via `Directory.Packages.props` |
| CI | GitHub Actions — build → test → analyze → pack → publish |
