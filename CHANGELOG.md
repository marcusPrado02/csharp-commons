# Changelog

All notable changes to this project are documented in this file.

The format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).
Versioning follows [Semantic Versioning](https://semver.org/spec/v2.0.0.html).
This file is generated automatically by [`git-cliff`](https://git-cliff.org/) on each release — see `cliff.toml` for configuration.

---

## [Unreleased]

### Added
- `MarcusPrado.Platform.Nats` — JetStream publisher/consumer with at-least-once delivery
- `MarcusPrado.Platform.AzureServiceBus` — publisher, consumer with automatic lock renewal, dead-letter sink
- `MarcusPrado.Platform.AwsSqs` — SQS long-polling consumer, SQS/SNS publisher, SNS→SQS fan-out
- `MarcusPrado.Platform.DlqReprocessing` — Minimal API endpoints, OTel metrics, `IDlqStore`
- `MarcusPrado.Platform.EventSourcing` — `IEventStore`, `EventSourcedRepository`, snapshots, saga orchestration, projections
- `MarcusPrado.Platform.DistributedLock` — Redis (Redlock) and PostgreSQL advisory lock implementations
- `MarcusPrado.Platform.Degradation` — four named operating modes with middleware enforcement and management endpoints
- `MarcusPrado.Platform.Configuration` — hot-reload via `IOptionsHotReload<T>`, encrypted config provider
- `MarcusPrado.Platform.Security` — `OidcClientService`, `AesGcmEncryption`, `KeyRotationService`, digital signatures, PII masking, mTLS
- `MarcusPrado.Platform.Quartz` / `Hangfire` — `IJobScheduler` adapters
- `MarcusPrado.Platform.Cli` — `dotnet tool` with scaffold, config encrypt, dlq inspect, health, arch validate commands
- `MarcusPrado.Platform.Analyzers` — PLATFORM001–005 Roslyn analyzers with `AddResultWrapperCodeFix`
- `MarcusPrado.Platform.Templates` — `dotnet new` templates: `platform-api`, `platform-worker`, `platform-domain`, `platform-command`
- `MarcusPrado.Platform.ApprovalTestKit` — `PlatformVerifySettings`, `ApiResponseVerifier`, `DomainEventVerifier`, `SqlQueryVerifier`
- `MarcusPrado.Platform.ChaosKit` — `LatencyFault`, `ErrorFault`, `PacketLossFault`, `ChaosRunner`
- `MarcusPrado.Platform.ContractTestKit` — Pact CDC verifier + async contract verifier
- `MarcusPrado.Platform.PerformanceTestKit` — NBomber-based `PlatformLoadTest` with P50/P95/P99 reporting
- Business Metrics OTel instrumentation via `IBusinessMetrics`
- SLO / Error Budget tracking via `SloMetricsCollector`
- Circuit Breaker management endpoints and `CircuitBreakerRegistry`
- Cache stampede prevention via `StampedeProtectedCache` (XFetch algorithm)
- Advanced health checks: `MemoryPressureHealthCheck`, `ThreadPoolStarvationHealthCheck`, `ExternalDependencyHealthCheck`
- Startup verification via `IStartupVerification` and `StartupVerificationHostedService`
- i18n: `AcceptLanguageMiddleware`, `LocalizedErrorTranslator`, `ValidationMessageLocalizer` (en-US, pt-BR, es-ES)
- `ErrorCatalog` — structured typed error constants, `IErrorTranslator`, `ErrorDocumentationGenerator`
- Exception enrichment: `ExceptionFingerprinter`, `ExceptionGrouper`, `DeveloperExceptionPageEnricher`
- API Changelog tooling: `ApiSurfaceExtractor`, `ApiDiffEngine`, `ChangelogRenderer`
- Package documentation in `docs/packages/` (10 files)
- Architecture Decision Records ADR-007 through ADR-015

---

## [0.9.0] — 2026-03-01

### Added
- Initial scaffolding: 40 projects across `src/core/`, `src/extensions/`, `src/kits/`, `src/tooling/`, `src/samples/`
- `Result<T>` discriminated union with `Map`, `Bind`, `Match`, implicit conversions
- Full domain primitives: `Entity<TId>`, `AggregateRoot<TId>`, `ValueObject`, `Specification<T>`, `DomainEvent`, `IBusinessRule`
- CQRS pipeline with 8 built-in behaviors (validation, tracing, metrics, logging, authorization, idempotency, transaction, retry)
- `AppDbContextBase` with audit filling, domain event dispatch, multi-tenant filters, outbox/inbox tables
- `MarcusPrado.Platform.Kafka` — producer, consumer, OTel tracing propagation
- `MarcusPrado.Platform.RabbitMq` — producer, consumer, publisher confirms
- `MarcusPrado.Platform.Redis` — `ICache`, `RedisQuotaStore`, `IIdempotencyStore`
- `MarcusPrado.Platform.OpenTelemetry` — `AddPlatformTelemetry()` one-call OTel setup
- `MarcusPrado.Platform.Serilog` — structured logging with PII redaction and correlation enrichment
- ASP.NET Core: correlation, tenant, exception, and request-logging middlewares
- JWT + API Key authentication handlers
- RFC 9457 ProblemDetails factory and exception mapper
- Minimal API conventions: `IEndpoint`, `EndpointGroupBase`, `EndpointDiscovery`
- API versioning, OpenAPI/Scalar, CORS profiles, rate limiting, security headers, IP filtering, response compression, request size limiting
- `MarcusPrado.Platform.Http` — `TypedHttpClient` with `AddStandardResilienceHandler()`
- `MarcusPrado.Platform.TestKit` — `PlatformTestEnvironment`, `IntegrationFixture`, `FakeClock`, `FakeTenantContext`, `SnapshotRestorer`, `Eventually`
- `MarcusPrado.Platform.ArchTests` — layering, naming, contract compatibility rules
- Concrete adapters: Nethereum, Stripe, MailKit, SendGrid, Twilio, AwsSns (SMS), Elasticsearch, OpenSearch, HotChocolate, QuestPDF, ClosedXml, Consul, MongoDb
- Mutation testing configuration via Stryker per core project
- MinVer + git-cliff release automation in CI

---

*This file is managed by `git-cliff`. To generate an updated version locally: `git cliff --output CHANGELOG.md`*
