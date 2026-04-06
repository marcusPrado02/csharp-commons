# Documentation Design — MarcusPrado Platform Commons

| Field | Value |
|-------|-------|
| **Date** | 2026-04-06 |
| **Status** | Approved |
| **Author** | Marcus Prado Silva |
| **Goal** | Comprehensive public documentation that demonstrates staff-engineer-level thinking for portfolio audiences |

---

## Context

The repository (`marcusPrado02/csharp-commons`) is a .NET 10 platform library with ~40 NuGet packages covering cross-cutting concerns for microservices. It is public and serves dual purposes:

1. **Portfolio** — demonstrates architectural maturity to senior engineers and technical recruiters
2. **Personal standard** — used as a base for future microservice repositories

Current documentation state:
- `README.md` — 2 lines (badge only)
- `docs/architecture/overview.md` — layer diagram, dependency rules
- `docs/architecture/layer-rules.md` — enforcement matrix
- `docs/architecture/adr/` — 4 ADRs (ADR-001 through ADR-004)

No community health files, no package docs, no contribution guide.

---

## Approach

**Option C selected:** Markdown-only, GitHub-native. No external site (Docusaurus). Maximum impact with minimum tooling overhead. GitHub renders Markdown natively; recruiters open repositories, not external sites.

---

## Section 1 — README Principal

**File:** `README.md` (replace current 2-line version)

**Structure:**

### 1.1 Header block
- Project name + tagline: *"Staff-engineer-grade .NET 10 platform library for microservices"*
- Badges row: CI status · NuGet version · coverage · mutation score · license

### 1.2 "What is this?" (3–4 sentences)
Explains the problem: every microservice team reinvents the same cross-cutting concerns (Result<T>, CQRS pipeline, multi-tenancy, observability, resilience). This library provides production-ready, independently installable NuGet packages for all of them.

### 1.3 Packages table
Full table of all ~40 packages with:
- Package name (linked to its doc file in `docs/packages/`)
- One-line description
- NuGet badge

### 1.4 Quickstart — 3 code examples
Real, runnable snippets demonstrating the most impressive patterns:

1. **Result<T> + Error catalog** — command handler returning `Result<Order>` with typed failure
2. **Full CQRS pipeline** — `ICommandBus.SendAsync<CreateOrderCommand>()` with validation + audit behaviors
3. **Minimal API endpoint** — `AddPlatformHttpClient<OrderServiceClient>()` with auth token + tenant propagation + resilience

### 1.5 Architecture diagram
Extract the layer diagram from `docs/architecture/overview.md` inline into README (ASCII art already exists).

### 1.6 Navigation links
- [Architecture & ADRs](docs/architecture/overview.md)
- [Package documentation](docs/packages/)
- [Contributing](CONTRIBUTING.md)
- [Changelog](CHANGELOG.md)
- [Security](SECURITY.md)

---

## Section 2 — Architecture Decision Records

**Directory:** `docs/architecture/adr/`

11 new ADRs following the established template (Context / Decision / Consequences / Status / Date / Deciders).

| ADR | Title | Core insight demonstrated |
|-----|-------|--------------------------|
| ADR-005 | Central Package Management via `Directory.Packages.props` | Dependency governance at scale |
| ADR-006 | Analyzer stack: StyleCop + SonarAnalyzer + Roslynator + CA with `TreatWarningsAsErrors` | Engineering culture and code quality enforcement |
| ADR-007 | Outbox/Inbox pattern for at-least-once messaging guarantee | Distributed systems correctness |
| ADR-008 | Multi-tenancy via ambient `ITenantContext` in the pipeline (not per-repository) | Non-obvious design tradeoff |
| ADR-009 | Custom Roslyn Analyzers (PLATFORM001–005) over code review conventions | Automated architectural enforcement |
| ADR-010 | MinVer for automatic semantic versioning from git tags | Release engineering discipline |
| ADR-011 | Testcontainers for integration tests (real infrastructure, no mocks) | Test philosophy and reliability |
| ADR-012 | W3C TraceContext propagation (open standard, not proprietary header) | Interoperability decision |
| ADR-013 | `AddStandardResilienceHandler()` (Microsoft.Extensions.Http.Resilience) over raw Polly | Abstraction level decision |
| ADR-014 | Explicit degradation states (`None / PartiallyDegraded / ReadOnly / Maintenance`) over generic feature flags | Operability design |
| ADR-015 | Broker-agnostic messaging via `IMessagePublisher` / `IMessageConsumer` abstractions | Portability and testability |

---

## Section 3 — Package Documentation

**Directory:** `docs/packages/`

10 files covering all packages by logical group (not one file per package):

| File | Packages covered |
|------|-----------------|
| `core-abstractions.md` | Result<T>, Error, ICommand/IQuery, IRepository, ICorrelationContext, ITenantContext, IUserContext |
| `core-domain.md` | AggregateRoot, DomainEvent, ValueObject, Specification, DomainException |
| `core-cqrs.md` | CommandBus, QueryBus, pipeline behaviors (validation, audit, logging, idempotency) |
| `core-messaging.md` | IMessagePublisher, IMessageConsumer, Outbox pattern, Inbox pattern |
| `extensions-web.md` | AspNetCore, Auth, ProblemDetails, RateLimiting, CORS, OpenAPI/Scalar, API Versioning, Security Headers, IP Filter |
| `extensions-data.md` | EfCore, Postgres, MySql, Redis, MongoDb, DataAccess tracing |
| `extensions-messaging.md` | Kafka, RabbitMq, Nats, AzureServiceBus, AwsSqs/AwsSns, DlqReprocessing |
| `extensions-observability.md` | OpenTelemetry, Serilog, CircuitBreaker dashboard, SLO/Error Budget, Business Metrics |
| `extensions-resilience.md` | DistributedLock, CacheStampede prevention, Graceful Degradation, HealthChecks, StartupVerification |
| `kits-testing.md` | TestKit, ContractTestKit (Pact), ChaosKit, PerformanceTestKit, ApprovalTestKit |

**Each file follows this structure:**
1. Purpose (2 sentences)
2. Install (`dotnet add package`)
3. Quick example (10–20 lines of real code)
4. Key types table (type → purpose)
5. Link to source

---

## Section 4 — Community Health Files

**Root-level files:**

### `CONTRIBUTING.md`
- Prerequisites (`.NET 10 SDK`, `Docker` for integration tests)
- Clone + build + test instructions (3 commands)
- Coding standards (link to `.editorconfig`, StyleCop, CA)
- Commit message format (Conventional Commits: `feat`, `fix`, `docs`, `chore`)
- PR process: branch → PR → CI must pass → review
- ADR process: when a new architectural decision is made, open an ADR PR first
- How to add a new package (checklist: csproj, GlobalUsings, solution entry, tests, docs)

### `SECURITY.md`
- Supported versions table
- How to report a vulnerability (GitHub private advisory, not public issues)
- Response SLA (acknowledge within 72h, patch within 30 days for critical)

### `CODE_OF_CONDUCT.md`
- Contributor Covenant v2.1 (industry standard, no customization needed)

### `CHANGELOG.md`
- Generated by `git-cliff` (already configured in `cliff.toml` and `release.yml`)
- Seeded with entries from v0.1.0 to current main branch state

### `.github/ISSUE_TEMPLATE/bug_report.yml`
- Structured YAML template: version, repro steps, expected vs actual, logs

### `.github/ISSUE_TEMPLATE/feature_request.yml`
- Structured YAML template: problem statement, proposed solution, alternatives considered

### `.github/pull_request_template.md`
- Checklist: tests pass · docs updated · ADR created (if architectural) · CHANGELOG entry (if user-facing)

---

## File Tree (new files only)

```
csharp-commons/
├── README.md                                          ← full rewrite
├── CONTRIBUTING.md                                    ← new
├── SECURITY.md                                        ← new
├── CODE_OF_CONDUCT.md                                 ← new
├── CHANGELOG.md                                       ← new (git-cliff generated)
├── .github/
│   ├── ISSUE_TEMPLATE/
│   │   ├── bug_report.yml                             ← new
│   │   └── feature_request.yml                        ← new
│   └── pull_request_template.md                       ← new
└── docs/
    ├── architecture/
    │   └── adr/
    │       ├── ADR-005-central-package-management.md  ← new
    │       ├── ADR-006-analyzer-stack.md              ← new
    │       ├── ADR-007-outbox-inbox.md                ← new
    │       ├── ADR-008-multi-tenancy-context.md       ← new
    │       ├── ADR-009-roslyn-analyzers.md            ← new
    │       ├── ADR-010-minver-versioning.md           ← new
    │       ├── ADR-011-testcontainers.md              ← new
    │       ├── ADR-012-w3c-tracecontext.md            ← new
    │       ├── ADR-013-http-resilience.md             ← new
    │       ├── ADR-014-degradation-states.md          ← new
    │       └── ADR-015-broker-agnostic-messaging.md   ← new
    └── packages/
        ├── core-abstractions.md                       ← new
        ├── core-domain.md                             ← new
        ├── core-cqrs.md                               ← new
        ├── core-messaging.md                          ← new
        ├── extensions-web.md                          ← new
        ├── extensions-data.md                         ← new
        ├── extensions-messaging.md                    ← new
        ├── extensions-observability.md                ← new
        ├── extensions-resilience.md                   ← new
        └── kits-testing.md                            ← new
```

**Total: 29 new files.**

---

## Success Criteria

A senior engineer opening the repository for the first time should be able to:

1. Understand what the library does and why it exists within 60 seconds (README)
2. Install and use any package with a working example without reading source code (package docs)
3. Understand the reasoning behind every major design choice (ADRs)
4. Know exactly how to contribute without asking anyone (CONTRIBUTING.md)
5. Trust that security is taken seriously (SECURITY.md)

---

## Out of Scope

- Docusaurus / GitHub Pages site
- Auto-generated API reference (XML doc → website)
- Localization of documentation (English only)
- Video walkthroughs or screencasts
