# MarcusPrado.Platform.Commons

A staff-engineer-grade reusable platform library for **.NET 10+ microservices**.  
Ships as a set of independently installable NuGet packages — adopt exactly the pieces you need, without pulling in unrelated dependencies.

[![CI](https://github.com/MarcusPrado/csharp-commons/actions/workflows/ci.yml/badge.svg)](https://github.com/MarcusPrado/csharp-commons/actions/workflows/ci.yml)
[![NuGet](https://img.shields.io/nuget/v/MarcusPrado.Platform.Abstractions.svg)](https://www.nuget.org/packages?q=MarcusPrado.Platform)
[![GitHub Packages](https://img.shields.io/badge/GitHub%20Packages-available-blue)](https://github.com/MarcusPrado?tab=packages)
[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4)](https://dotnet.microsoft.com)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)
![Mutation Score](https://img.shields.io/badge/mutation%20score-%3E70%25-brightgreen)

---

## Architecture

```
Samples / Services
       ↓
     Kits  (testing helpers)
       ↓
  Extensions  (infrastructure adapters: EF Core, Kafka, Redis, …)
       ↓
    Core  (pure .NET: Domain, Application, Messaging, Observability, …)
```

Extensions never depend on each other — compose at the service level.  
Full details: [docs/architecture/overview.md](docs/architecture/overview.md)

---

## Package Catalog

### Core — pure .NET, no infrastructure dependencies

| Package | Description |
|---------|-------------|
| `MarcusPrado.Platform.Abstractions` | Base interfaces and `Result<T>` type |
| `MarcusPrado.Platform.Abstractions.Blockchain` | Blockchain provider abstraction |
| `MarcusPrado.Platform.Abstractions.Documents` | Document storage abstraction |
| `MarcusPrado.Platform.Abstractions.Email` | Email sender abstraction |
| `MarcusPrado.Platform.Abstractions.GraphQL` | GraphQL schema abstraction |
| `MarcusPrado.Platform.Abstractions.Payment` | Payment gateway abstraction |
| `MarcusPrado.Platform.Abstractions.Search` | Search provider abstraction |
| `MarcusPrado.Platform.Abstractions.ServiceDiscovery` | Service discovery abstraction |
| `MarcusPrado.Platform.Abstractions.Sms` | SMS sender abstraction |
| `MarcusPrado.Platform.Abstractions.Storage` | Object storage abstraction |
| `MarcusPrado.Platform.Domain` | Domain primitives, entities, value objects |
| `MarcusPrado.Platform.Application` | CQRS pipeline (own implementation, no MediatR) |
| `MarcusPrado.Platform.Contracts` | Shared message contracts |
| `MarcusPrado.Platform.Runtime` | Service runtime lifecycle helpers |
| `MarcusPrado.Platform.Messaging` | Messaging abstractions (publishers, consumers) |
| `MarcusPrado.Platform.Observability` | Telemetry interfaces and OTel semantic conventions |
| `MarcusPrado.Platform.Persistence` | Repository and Unit of Work abstractions |
| `MarcusPrado.Platform.OutboxInbox` | Transactional outbox / inbox pattern |
| `MarcusPrado.Platform.Security` | Authentication and authorisation abstractions |
| `MarcusPrado.Platform.MultiTenancy` | Multi-tenancy context and resolution |
| `MarcusPrado.Platform.FeatureFlags` | Feature flag evaluation |
| `MarcusPrado.Platform.RateLimiting` | Rate limiter abstractions |
| `MarcusPrado.Platform.BackgroundJobs` | Background job scheduler abstraction |
| `MarcusPrado.Platform.Resilience` | Retry, circuit-breaker, bulkhead policies |
| `MarcusPrado.Platform.Governance` | Policy and compliance hooks |
| `MarcusPrado.Platform.AuditLog` | Audit log abstractions |
| `MarcusPrado.Platform.BackupRestore` | Backup/restore lifecycle hooks |
| `MarcusPrado.Platform.Workflow` | Workflow engine abstraction |
| `MarcusPrado.Platform.ErrorCatalog` | Centralised error code registry |

### Extensions — infrastructure adapters (depend on Core only)

| Package | Description |
|---------|-------------|
| `MarcusPrado.Platform.AspNetCore` | ASP.NET Core integration (middleware, DI) |
| `MarcusPrado.Platform.AspNetCore.Auth` | JWT / OAuth2 / OIDC integration |
| `MarcusPrado.Platform.AspNetCore.ProblemDetails` | RFC 7807 Problem Details factory |
| `MarcusPrado.Platform.EfCore` | EF Core repository + UoW implementation |
| `MarcusPrado.Platform.Postgres` | PostgreSQL data source configuration |
| `MarcusPrado.Platform.MySql` | MySQL data source configuration |
| `MarcusPrado.Platform.MongoDb` | MongoDB repository implementation |
| `MarcusPrado.Platform.Redis` | Redis distributed cache + pub/sub |
| `MarcusPrado.Platform.Kafka` | Kafka publisher and consumer |
| `MarcusPrado.Platform.RabbitMq` | RabbitMQ publisher and consumer |
| `MarcusPrado.Platform.AzureServiceBus` | Azure Service Bus publisher and consumer |
| `MarcusPrado.Platform.AwsSqs` | AWS SQS/SNS publisher and consumer |
| `MarcusPrado.Platform.Nats` | NATS publisher and consumer |
| `MarcusPrado.Platform.OpenTelemetry` | OpenTelemetry SDK wiring (traces, metrics, logs) |
| `MarcusPrado.Platform.Serilog` | Serilog structured logging setup |
| `MarcusPrado.Platform.HealthChecks` | ASP.NET Core health check registrations |
| `MarcusPrado.Platform.Grpc` | gRPC interceptors and client factory |
| `MarcusPrado.Platform.Http` | Typed HTTP client factory with resilience |
| `MarcusPrado.Platform.Hangfire` | Hangfire background job adapter |
| `MarcusPrado.Platform.Quartz` | Quartz.NET scheduler adapter |
| `MarcusPrado.Platform.Elasticsearch` | Elasticsearch search provider implementation |
| `MarcusPrado.Platform.HotChocolate` | Hot Chocolate GraphQL server integration |
| `MarcusPrado.Platform.SignalR` | ASP.NET Core SignalR integration |
| `MarcusPrado.Platform.Consul` | Consul service discovery integration |
| `MarcusPrado.Platform.Configuration` | Layered configuration and secrets integration |
| `MarcusPrado.Platform.Secrets` | Secrets manager adapters (Vault, AWS Secrets, etc.) |
| `MarcusPrado.Platform.DataAccess` | Dapper / raw SQL data access helpers |
| `MarcusPrado.Platform.EventSourcing` | Event sourcing store and projections |
| `MarcusPrado.Platform.EventRouting` | Domain event → integration event routing |
| `MarcusPrado.Platform.DlqReprocessing` | Dead-letter queue reprocessing with Minimal API |
| `MarcusPrado.Platform.DistributedLock` | Distributed lock abstraction (Redis-backed) |
| `MarcusPrado.Platform.Degradation` | Degraded-mode circuit logic |
| `MarcusPrado.Platform.ExceptionEnrichment` | Unhandled exception enrichment middleware |
| `MarcusPrado.Platform.MailKit` | MailKit email sender implementation |
| `MarcusPrado.Platform.Twilio` | Twilio SMS sender implementation |
| `MarcusPrado.Platform.Stripe` | Stripe payment gateway implementation |
| `MarcusPrado.Platform.Nethereum` | Nethereum Ethereum provider implementation |
| `MarcusPrado.Platform.Protobuf` | Protobuf serialization helpers |
| `MarcusPrado.Platform.Pdf` | PDF generation adapter |
| `MarcusPrado.Platform.Excel` | Excel file generation adapter |
| `MarcusPrado.Platform.Observability` | Full OTel setup (combines OpenTelemetry ext) |
| `MarcusPrado.Platform.Security` | Encryption, hashing, and claims helpers |

### Kits — testing helpers (depend on Extensions + Core)

| Package | Description |
|---------|-------------|
| `MarcusPrado.Platform.TestKit` | xUnit base classes, fixtures, and builder helpers |
| `MarcusPrado.Platform.ObservabilityTestKit` | In-memory OTel exporters for verifying telemetry |
| `MarcusPrado.Platform.ContractTestKit` | Pact consumer / provider test helpers |
| `MarcusPrado.Platform.ApprovalTestKit` | Approval testing helpers (Verify) |
| `MarcusPrado.Platform.ChaosKit` | Chaos engineering helpers (latency, fault injection) |
| `MarcusPrado.Platform.IntegrationTestEnvironment` | Testcontainers-based environment bootstrapper |
| `MarcusPrado.Platform.PerformanceTestKit` | NBomber / k6 test setup helpers |

---

## Installation

### From NuGet.org (recommended)

```bash
dotnet add package MarcusPrado.Platform.Abstractions
dotnet add package MarcusPrado.Platform.Domain
dotnet add package MarcusPrado.Platform.Application
# add whichever extensions you need:
dotnet add package MarcusPrado.Platform.EfCore
dotnet add package MarcusPrado.Platform.OpenTelemetry
```

### From GitHub Packages

> **Note**: GitHub Packages NuGet requires authentication even for public packages.

1. Create or update `nuget.config` at your solution root:

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
    <add key="github-marcusprado"
         value="https://nuget.pkg.github.com/MarcusPrado/index.json" />
  </packageSources>
  <packageSourceCredentials>
    <github-marcusprado>
      <add key="Username" value="YOUR_GITHUB_USERNAME" />
      <!-- Use a PAT with read:packages scope, or set via env var -->
      <add key="ClearTextPassword" value="%GITHUB_TOKEN%" />
    </github-marcusprado>
  </packageSourceCredentials>
</configuration>
```

2. Set the `GITHUB_TOKEN` environment variable to a GitHub PAT with `read:packages` scope:

```bash
export GITHUB_TOKEN=ghp_...
dotnet restore
```

---

## Quick Start

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddPlatformCore()                    // Domain + Application pipeline
    .AddPlatformObservability(builder.Configuration)   // OTel traces + metrics
    .AddPlatformEfCore<MyDbContext>(builder.Configuration)
    .AddPlatformAspNetCore();             // middleware, problem details

var app = builder.Build();
app.UsePlatformMiddleware();
app.Run();
```

```csharp
// A command handler using the built-in CQRS pipeline
public sealed class CreateOrderHandler : ICommandHandler<CreateOrderCommand, OrderId>
{
    public async Task<Result<OrderId>> HandleAsync(
        CreateOrderCommand command, CancellationToken ct)
    {
        var order = Order.Create(command.CustomerId, command.Items);
        if (order.IsFailure) return order.Error;

        await _repository.AddAsync(order.Value, ct);
        return order.Value.Id;
    }
}
```

---

## Architecture Decision Records

| ADR | Decision |
|-----|----------|
| [ADR-001](docs/architecture/adr/ADR-001-result-type.md) | `Result<T>` instead of exceptions for expected failure paths |
| [ADR-002](docs/architecture/adr/ADR-002-no-mediatr.md) | Own CQRS pipeline instead of MediatR |
| [ADR-003](docs/architecture/adr/ADR-003-efcore-in-extension.md) | EF Core belongs in Extensions, never in Core |
| [ADR-004](docs/architecture/adr/ADR-004-otel-semantic-conventions.md) | OpenTelemetry Semantic Conventions for all telemetry |

---

## Contributing

See [TASKS.md](TASKS.md) for the current backlog and active work.  
The layer dependency rules are enforced by architecture tests — see [docs/architecture/layer-rules.md](docs/architecture/layer-rules.md).

```bash
# Build and run all tests locally
dotnet restore
dotnet build -c Release
dotnet test -c Release

# Check formatting before pushing
dotnet tool restore
dotnet csharpier check .
dotnet format --verify-no-changes --severity error
```
