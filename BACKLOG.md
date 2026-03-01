# Backlog de Implementação - MarcusPrado Platform Commons (.NET)

> Biblioteca reutilizável de plataforma de engenharia nível staff para .NET 9+
>
> **Objetivo**: Fornecer uma biblioteca modular, testável e de alta qualidade para microsserviços ASP.NET Core com suporte para REST, eventos, gRPC, GraphQL e múltiplos stacks (Minimal API, Worker Services, Kafka, RabbitMQ, EF Core).
>
> **Solução**: `MarcusPrado.Platform.Commons.slnx`  
> **Target framework**: `net9.0` (Analyzers: `netstandard2.0`)  
> **Build**: `dotnet build` | **Testes**: `dotnet test` | **Lint**: `dotnet csharpier` / `dotnet format`

---

## ✅ Concluídos

### 1. ✅ Scaffolding inicial da solução
**Status**: Concluído em 01/03/2026

**Projetos criados** (35 total — 286 stubs `.cs`):

**`src/core/`** (16 projetos):
- `MarcusPrado.Platform.Abstractions` — IClock, IGuidFactory, IIdGenerator, IJsonSerializer, IHasher, IEncryption, ICompression; IRequestContext, ICorrelationContext, ITenantContext, IUserContext; ICommandBus, IQueryBus, IEventBus, IDispatcher; IUnitOfWork, ITransaction; IAppMeter, IAppTracer, IAppLogger; IValidator, IValidationResult; IErrorCatalog, IErrorTranslator
- `MarcusPrado.Platform.Domain` — Entity, AggregateRoot, ValueObject, DomainException, BusinessRuleViolationException, IBusinessRule; EntityId, TenantId, UserId, CorrelationId; IDomainEvent, DomainEvent, DomainEventEnvelope, IDomainEventRecorder, IDomainEventPublisher; IPolicy, PolicyResult; ISpecification, Specification; IAuditable, AuditRecord; ISemanticVersion, CompatibilityMode
- `MarcusPrado.Platform.Application` — ICommand, ICommandHandler, IQuery, IQueryHandler, ICommandResult, IQueryResult; IPipelineBehavior, ValidationBehavior, AuthorizationBehavior, IdempotencyBehavior, TransactionBehavior, RetryBehavior, LoggingBehavior, MetricsBehavior, TracingBehavior; AppException, NotFoundException, ConflictException, UnauthorizedException, ForbiddenException; IMapper, IScheduler
- `MarcusPrado.Platform.Contracts` — ApiEnvelope, ApiError, Pagination, Sort, Filter; ProblemDetailsModel, ProblemDetailsMapper; IApiVersionPolicy, ApiVersion, DeprecationPolicy; IEventContract, EventContractEnvelope, EventSchemaVersion; ICompatibilityRules, BackwardCompatibleRule, ForwardCompatibleRule
- `MarcusPrado.Platform.Runtime` — IAppConfiguration, EnvConfiguration, ConfigurationKey; IHostedLifecycle, StartupHook, ShutdownHook, GracefulShutdown; DeploymentEnvironment, Region, InstanceInfo
- `MarcusPrado.Platform.MultiTenancy` — TenantContext, ITenantResolver, HeaderTenantResolver, JwtTenantResolver; ITenantIsolationStrategy, SchemaPerTenantStrategy, DatabasePerTenantStrategy, DiscriminatorStrategy; TenantQuota, ITenantQuotaProvider, QuotaExceededException
- `MarcusPrado.Platform.Security` — ITokenValidator, ITokenIntrospector, AuthenticationResult; IPolicyAuthorizer, AuthorizationDecision, Permission, Scope; PiiClassifier, IPiiRedactor, RedactionRule; ISecretProvider, SecretReference; ISecurityAuditSink, SecurityAuditEvent, SecurityAuditCategory
- `MarcusPrado.Platform.Observability` — CorrelationContext, CorrelationEnricher; LogEvent, LogSanitizer; MetricNames, MetricTags, BusinessMetric, IBusinessMetrics; TraceNames, SpanAttributes, ITracing; IHealthProbe, HealthStatus, DependencyHealthProbe; ServiceLevelObjective, ErrorBudget
- `MarcusPrado.Platform.Resilience` — RetryPolicy, CircuitBreakerPolicy, TimeoutPolicy, BulkheadPolicy, RateLimitPolicy, HedgingPolicy; ExponentialBackoff, DecorrelatedJitterBackoff; AdaptiveConcurrencyLimiter, BackpressureSignal, OverloadException; ResilientExecutor, ResilienceContext
- `MarcusPrado.Platform.Messaging` — IMessageBus, IMessagePublisher, IMessageConsumer, IMessageHandler; MessageEnvelope, MessageHeaders, MessageMetadata; TopicName, ConsumerGroup; IInboxStore, InboxMessage, InboxState; DeadLetterMessage, IDeadLetterSink, DLQReprocessor; IMessageSerializer, JsonMessageSerializer
- `MarcusPrado.Platform.Persistence` — IRepository, IReadRepository, IWriteRepository; IUnitOfWork, TransactionScope; IMigrationRunner, MigrationPlan; OptimisticConcurrencyException, IConcurrencyToken; IAuditWriter, AuditEntry
- `MarcusPrado.Platform.OutboxInbox` — OutboxMessage, OutboxState, IOutboxStore, IOutboxPublisher, OutboxProcessor; InboxMessage, InboxState, IInboxStore, InboxProcessor; IdempotencyKey, IIdempotencyStore, IdempotencyRecord
- `MarcusPrado.Platform.FeatureFlags` — FeatureFlag, FeatureVariant; IFeatureFlagProvider, FeatureFlagContext, FeatureDecision
- `MarcusPrado.Platform.RateLimiting` — IRateLimitPolicy, FixedWindowPolicy, SlidingWindowPolicy, TokenBucketPolicy; QuotaKey, QuotaCounter, IQuotaStore
- `MarcusPrado.Platform.BackgroundJobs` — IJob, IJobHandler, IJobScheduler; JobRunner, JobContext, JobResult
- `MarcusPrado.Platform.Governance` — IContractRegistry, ContractRegistration, ContractMetadata; CompatibilityReport, ContractCompatibilityChecker; DeprecationNotice, DeprecationSchedule; PlatformStandard, StandardViolation; AdrRecord, IAdrStore

**`src/extensions/`** (11 projetos):
- `MarcusPrado.Platform.AspNetCore` — CorrelationMiddleware, ExceptionMiddleware, RequestLoggingMiddleware, TenantResolutionMiddleware; ProblemDetailsFilter; WebApplicationExtensions, ServiceCollectionExtensions
- `MarcusPrado.Platform.AspNetCore.Auth` — JwtAuthenticationHandler, ApiKeyAuthenticationHandler; AuthServiceExtensions
- `MarcusPrado.Platform.AspNetCore.ProblemDetails` — ProblemDetailsFactory, ExceptionMapper; ProblemDetailsExtensions
- `MarcusPrado.Platform.EfCore` — AppDbContextBase, TenantDbContextDecorator; EfOutboxStore, EfInboxStore; EfUnitOfWork; EfMigrationRunner
- `MarcusPrado.Platform.Postgres` — PostgresConnectionFactory, PostgresHealthProbe; PostgresExtensions
- `MarcusPrado.Platform.Redis` — ICache, RedisCache; RedisQuotaStore
- `MarcusPrado.Platform.Kafka` — KafkaProducer, KafkaConsumer; KafkaMessageSerializer
- `MarcusPrado.Platform.RabbitMq` — RabbitProducer, RabbitConsumer
- `MarcusPrado.Platform.OpenTelemetry` — OpenTelemetryConfigurator
- `MarcusPrado.Platform.Serilog` — SerilogConfigurator
- `MarcusPrado.Platform.HealthChecks` — LivenessCheck, ReadinessCheck; HealthCheckExtensions

**`src/kits/`** (4 projetos):
- `MarcusPrado.Platform.TestKit` — IntegrationFixture, ApiFixture; PostgresContainer, RedisContainer, KafkaContainer, RabbitMqContainer; FakeClock, FakeTenantContext; Eventually
- `MarcusPrado.Platform.ContractTestKit` — PactVerifier, PactPublisher; AsyncContractVerifier
- `MarcusPrado.Platform.ChaosKit` — LatencyFault, ErrorFault, PacketLossFault; ChaosRunner
- `MarcusPrado.Platform.ObservabilityTestKit` — InMemoryMetricCollector, InMemorySpanCollector; MetricAssertions, TraceAssertions

**`src/tooling/`** (2 projetos):
- `MarcusPrado.Platform.Analyzers` (`netstandard2.0`) — NoEfCoreInDomainAnalyzer, NoAspNetInDomainAnalyzer, DomainMustNotReferenceInfrastructureAnalyzer, EnforceResultTypeAnalyzer, EnforceIdempotencyKeyUsageAnalyzer; AddResultWrapperCodeFix
- `MarcusPrado.Platform.ArchTests` — DomainDependencyRules, LayeringRules, NamingConventionRules, ContractCompatibilityRules

**`src/samples/`** (2 projetos):
- `Sample.Service.MinimalApi` — ASP.NET Core Minimal API
- `Sample.Service.Worker` — .NET Worker Service

**`tests/`**: `unit/`, `integration/`, `contract/`, `architecture/` (diretórios prontos)

---

## 🎯 Fundação e Qualidade (Prioridade Alta)

### 2. Implementar `Result<T>` e tratamento de erros

**Módulo**: `MarcusPrado.Platform.Abstractions` / `MarcusPrado.Platform.Domain`

**Implementar**:
- `Result<T>` — struct discriminada com `IsSuccess`, `IsFailure`, `Value`, `Error`
- `Result` (não-genérico) — para operações sem retorno de valor
- `Error` — record com `Code` (string), `Message`, `Category` (enum), `Severity` (enum)
- `ErrorCategory` enum — `Validation`, `NotFound`, `Conflict`, `Unauthorized`, `Forbidden`, `Technical`, `External`, `Timeout`
- `Severity` enum — `Info`, `Warning`, `Error`, `Critical`
- `IResultExtensions` — métodos `Map`, `Bind`, `Match`, `OnSuccess`, `OnFailure`
- Operador implícito `T → Result<T>` e `Error → Result<T>`

**Equivalência Java**: `commons-kernel-result` com `Result<T>`, `Problem`, `ErrorCode`, `ErrorCategory`

**Dependências NuGet sugeridas**: Nenhuma (implementação nativa .NET)

**Testes** (≥ 15 casos):
- Criação de success/failure
- Encadeamento com Bind/Map
- Match pattern
- Operadores implícitos
- Comportamento thread-safe

---

### 3. Implementar primitivos de domínio completos

**Módulo**: `MarcusPrado.Platform.Domain`

**Implementar**:
- `Entity<TId>` — genérico, com `Id`, `DomainEvents` (coleção), `AddDomainEvent`, `ClearDomainEvents`, `Equals`/`GetHashCode` por identidade
- `AggregateRoot<TId>` — herda `Entity<TId>`, adiciona versionamento
- `ValueObject` — abstract com `GetEqualityComponents()` abstract, `Equals`/`GetHashCode`/operadores `==`/`!=` automáticos
- `EntityId` — record abstract base; `TypedId<T>` genérico; helpers de conversão implícita
- `Specification<T>` — `IsSatisfiedBy(T entity)`, operadores `And`, `Or`, `Not` (composite)
- `IDomainEventRecorder` — coleção de `IDomainEvent` acumulados durante o ciclo de vida do aggregate
- `AuditRecord` — com `CreatedAt`, `CreatedBy`, `UpdatedAt`, `UpdatedBy`, `DeletedAt` (soft delete)

**Equivalência Java**: `commons-kernel-domain` com records, builders, equals por identidade

---

### 4. Configurar análise estática de código

**Arquivos a criar/atualizar**:
- `.editorconfig` — regras Roslyn + CSharpier
- `Directory.Build.props` — Analyzers habilitados para todos os projetos, `TreatWarningsAsErrors` para core
- `Directory.Packages.props` — Centralizar todas as versões de NuGet (Central Package Management)
- `global.json` — Fixar versão do SDK .NET 9
- `.csharpierrc` — Configuração do formatter CSharpier

**Analyzers a adicionar** (via `Directory.Build.props`):
- `Microsoft.CodeAnalysis.NetAnalyzers` — regras CA*
- `StyleCop.Analyzers` — convenções de código
- `Roslynator.Analyzers` — regras extras de qualidade
- `SonarAnalyzer.CSharp` — segurança e bugs
- Projeto interno `MarcusPrado.Platform.Analyzers` — regras de arquitetura

**Equivalência Java**: Checkstyle + SpotBugs + PMD + Spotless

---

### 5. Configurar pipeline CI/CD

**Arquivo**: `.github/workflows/ci.yml`

**Stages**:
1. `build` — `dotnet build --no-restore -c Release`
2. `test` — `dotnet test --no-build --collect:"XPlat Code Coverage"` + upload Codecov
3. `analyze` — `dotnet format --verify-no-changes` + CSharpier check
4. `pack` — `dotnet pack` para todos os projetos core + extensions
5. `publish` — push para NuGet.org (em tags `v*`)

**Equivalência Java**: Maven + GitHub Actions com `mvn verify` + JaCoCo + Spotless

---

### 6. Documentação de arquitetura

**Arquivos a criar**:
- `docs/architecture/overview.md` — Diagrama de camadas (Core → Extensions → Kits → Samples)
- `docs/architecture/adr/ADR-001-result-type.md` — Decisão pelo padrão Result<T>
- `docs/architecture/adr/ADR-002-no-mediatr.md` — CQRS sem MediatR (pipeline próprio)
- `docs/architecture/adr/ADR-003-efcore-in-extension.md` — EF Core apenas em Extensions, nunca em Core
- `docs/architecture/adr/ADR-004-otel-semantic-conventions.md` — OpenTelemetry Semantic Conventions
- `docs/architecture/layer-rules.md` — Regras de dependência entre camadas (enforced por ArchTests)

---

### 7. Benchmarks de performance

**Projeto**: `tests/benchmarks/MarcusPrado.Platform.Benchmarks` (BenchmarkDotNet)

**Benchmarks prioritários**:
- `ResultBenchmark` — alocações de `Result<T>` vs exceptions
- `PipelineBenchmark` — throughput do pipeline CQRS com N behaviors
- `MessageSerializerBenchmark` — JSON vs MessagePack vs Protobuf
- `ConcurrencyLimiterBenchmark` — `AdaptiveConcurrencyLimiter` sob carga

**Equivalência Java**: JMH benchmarks em `commons-kernel-benchmarks`

---

## 🏗️ ArchTests e Validação de Arquitetura

### 8. Implementar regras ArchTests com NetArchTest

**Projeto**: `src/tooling/MarcusPrado.Platform.ArchTests`

**Dependência**: `NetArchTest.Rules` (≥ 1.4.0)

**Regras a implementar em `DomainDependencyRules`**:
- Domain não pode referenciar Extensions, EF Core, ASP.NET Core, ou infraestrutura
- `IRepository` não pode ser instanciado no Domain (apenas referenciado por interface)
- Exceptions de domínio devem herdar de `DomainException`

**Regras em `LayeringRules`**:
- Abstractions não tem dependência de nenhum outro projeto da plataforma
- Domain depende apenas de Abstractions
- Application depende de Domain e Abstractions (nunca de Extensions)
- Extensions podem depender de Core, nunca de outros Extensions diretamente

**Regras em `NamingConventionRules`**:
- Interfaces começam com `I`
- Handlers terminam com `Handler`
- Commands terminam com `Command`
- Queries terminam com `Query`
- Exceptions terminam com `Exception`

**Regras em `ContractCompatibilityRules`**:
- Contratos públicos marcados com `[ApiContract]` não removem propriedades
- Event contracts implementam `IEventContract`

**Testes de arquitetura** (suíte separada em `tests/architecture/`):
- Rodam como testes xUnit/NUnit normais no CI

**Equivalência Java**: `commons-arch-tests` com ArchUnit

---

## 🔌 Extensões ASP.NET Core

### 9. Implementar `MarcusPrado.Platform.AspNetCore` completo

**Dependências NuGet**: `Microsoft.AspNetCore.App`

**Middleware**:
- `CorrelationMiddleware` — extrai/gera `X-Correlation-ID` e `X-Request-ID`; propaga no `ICorrelationContext`
- `TenantResolutionMiddleware` — resolve `TenantId` via header, JWT, subdomínio; popula `ITenantContext`
- `ExceptionMiddleware` — captura todas as exceções, mapeia para RFC 9457 ProblemDetails via `ExceptionMapper`
- `RequestLoggingMiddleware` — structured logging de request/response com Serilog enrichers

**Extensions**:
- `WebApplicationExtensions.UsePlatformMiddlewares()` — registra middlewares na ordem correta
- `ServiceCollectionExtensions.AddPlatformCore()` — registra `IClock`, `IGuidFactory`, `IJsonSerializer`
- `ServiceCollectionExtensions.AddPlatformCqrs()` — registra pipeline CQRS com todos os behaviors

**Testes** (≥ 12 casos, usando `WebApplicationFactory<T>`):
- Correlation ID propagado no response header
- Tenant resolvido de header customizado
- Exception → ProblemDetails correto (status, type, title)
- Logging estruturado com request/response

**Equivalência Java**: `commons-spring-starter-web` com filtros e auto-configuration

---

### 10. Implementar `MarcusPrado.Platform.AspNetCore.Auth` completo

**Dependências NuGet**: `Microsoft.AspNetCore.Authentication.JwtBearer`

**Handlers**:
- `JwtAuthenticationHandler` — valida JWT, extrai claims, popula `IUserContext`
- `ApiKeyAuthenticationHandler` — valida API key via header `X-Api-Key`

**Authorization**:
- `PermissionRequirement` + `PermissionAuthorizationHandler` — autorização baseada em `Permission`
- `ScopeRequirement` + `ScopeAuthorizationHandler` — validação de scopes OAuth2

**Extensions**:
- `AuthServiceExtensions.AddPlatformAuth()` — registra JWT + API Key com defaults seguros
- `AuthServiceExtensions.AddPlatformAuthorization()` — registra handlers de autorização

**Testes** (≥ 10 casos):
- Token válido → autenticado com claims corretos
- Token expirado → 401
- Scope inválido → 403
- API Key ausente → 401

---

### 11. Implementar `MarcusPrado.Platform.AspNetCore.ProblemDetails` completo

**Dependências NuGet**: `Microsoft.AspNetCore.ProblemDetails`

**Implementar**:
- `ProblemDetailsFactory` — cria `ProblemDetails` a partir de `AppException`, `Result<T>` failure, ou status HTTP
- `ExceptionMapper` — mapeamento: `NotFoundException → 404`, `ConflictException → 409`, `UnauthorizedException → 401`, `ForbiddenException → 403`, `ValidationException → 422`, `DomainException → 422`, outros → 500
- `ProblemDetailsExtensions.AddPlatformProblemDetails()` — registra factory e extension com rfc9457 compliant

**Campos extras** no ProblemDetails:
- `traceId` — correlation ID do request
- `tenantId` — tenant atual (se multi-tenant)
- `errors` — lista de erros de validação (para 422)
- `code` — `ErrorCode` da plataforma

---

## 💾 Extensões de Persistência

### 12. Implementar `MarcusPrado.Platform.EfCore` completo

**Dependências NuGet**: `Microsoft.EntityFrameworkCore` (≥ 9.0)

**`AppDbContextBase`**:
- Salvar automaticamente `AuditRecord` em entidades que implementam `IAuditable`
- Dispatch de `IDomainEvent` após `SaveChangesAsync` via `IDomainEventPublisher`
- Suporte a soft-delete (filtra entidades com `DeletedAt != null`)
- Método `SaveChangesWithOutboxAsync` — persiste entidade + `OutboxMessage` em mesma transação

**`TenantDbContextDecorator`**:
- `QueryFilter` global por `TenantId` em todas as entidades que implementam `ITenantEntity`
- Define schema por tenant via `ITenantIsolationStrategy`

**`EfUnitOfWork`**:
- Implementa `IUnitOfWork` + `ITransaction`
- `BeginTransactionAsync` / `CommitAsync` / `RollbackAsync`
- Suporte a `SavepointAsync`

**`EfOutboxStore` / `EfInboxStore`**:
- Persistência de `OutboxMessage` / `InboxMessage` na mesma base de dados
- Índices otimizados: `State`, `ProcessedAt`, `ScheduledAt`

**`EfMigrationRunner`**:
- `RunMigrationsAsync()` usando `context.Database.MigrateAsync()`
- `GetPendingMigrationsAsync()` — lista migrações pendentes

**Testes** (≥ 15 casos, usando EF Core InMemory + TestContainers Postgres):
- Audit automático de created/updated
- Domain events dispatched após SaveChanges
- Tenant filter aplicado em queries
- Transação rollback em falha

**Equivalência Java**: `commons-adapters-persistence-jpa` com Spring Data JPA + Hibernate

---

### 13. Implementar `MarcusPrado.Platform.Postgres` completo

**Dependências NuGet**: `Npgsql.EntityFrameworkCore.PostgreSQL` + `Dapper`

**`PostgresConnectionFactory`**:
- Pool de conexões via `NpgsqlDataSource`
- Connection string builder type-safe
- Suporte a read replicas (connection routing)

**`PostgresHealthProbe`**:
- `IHealthProbe` que executa `SELECT 1` com timeout
- Reporta versão do Postgres + active connections

**`PostgresExtensions.AddPlatformPostgres()`**:
- Registra `NpgsqlDataSource`, `AppDbContext`, `PostgresHealthProbe`
- Configura snake_case naming (Npgsql)
- Registra `EfMigrationRunner`

**Equivalência Java**: `commons-adapters-persistence-postgres` com driver JDBC + Flyway

---

## 📨 Adaptadores de Mensageria

### 14. Implementar `MarcusPrado.Platform.Kafka` completo

**Dependências NuGet**: `Confluent.Kafka` (≥ 2.x)

**`KafkaProducer`**:
- Implementa `IMessagePublisher`
- Suporte a headers de `MessageMetadata` (correlation ID, tenant ID, schema version)
- Serialização via `IMessageSerializer` (JSON padrão, plugável)
- Fire-and-forget vs confirmed delivery (`Produce` vs `ProduceAsync`)
- Retry automático com `DecorrelatedJitterBackoff`

**`KafkaConsumer`**:
- Implementa `IMessageConsumer`
- Consumer group management via `ConsumerGroup`
- Commit manual (at-least-once semantics)
- Dead Letter Queue: mensagens com erro → topic DLQ via `IDeadLetterSink`
- Graceful shutdown com `CancellationToken`
- Hosted Service integration (`IHostedService`)

**`KafkaMessageSerializer`**:
- JSON padrão com `System.Text.Json`
- Suporte a schema registry ready (interface plugável para Confluent Schema Registry)

**Testes** (≥ 12 casos, `KafkaContainer` do TestKit):
- Publish → consume round-trip
- Dead letter após N retries
- Graceful shutdown sem perda de mensagens
- Headers propagados corretamente

**Equivalência Java**: `commons-adapters-messaging-kafka` com `spring-kafka`

---

### 15. Implementar `MarcusPrado.Platform.RabbitMq` completo

**Dependências NuGet**: `RabbitMQ.Client` (≥ 7.x)

**`RabbitProducer`**:
- Implementa `IMessagePublisher`
- Exchange/routing key via `TopicName`
- Publisher confirms para garantia de entrega
- Headers de `MessageMetadata`

**`RabbitConsumer`**:
- Implementa `IMessageConsumer`
- Binding automático por `ConsumerGroup`
- Ack/Nack manual
- DLQ via exchange `x-dead-letter-exchange`
- Backoff em Nack com TTL de mensagem

**Testes** (≥ 10 casos, `RabbitMqContainer` do TestKit):
- Publish/consume básico
- DLQ após falha
- Reconexão automática

**Equivalência Java**: `commons-adapters-messaging-rabbitmq` com `spring-amqp`

---

## 📊 Observabilidade

### 16. Implementar `MarcusPrado.Platform.OpenTelemetry` completo

**Dependências NuGet**: `OpenTelemetry.Extensions.Hosting`, `OpenTelemetry.Instrumentation.AspNetCore`, `OpenTelemetry.Instrumentation.Http`, `OpenTelemetry.Exporter.Otlp`, `OpenTelemetry.Exporter.Prometheus.AspNetCore`

**`OpenTelemetryConfigurator`**:
- `AddPlatformOpenTelemetry(IServiceCollection, OpenTelemetryOptions)` — registra Traces + Metrics + Logs
- **Tracing**: ASP.NET Core, HttpClient, EF Core, Kafka instrumentation automática; custom `ActivitySource` via `ITracing`
- **Metrics**: `IMeter` para `IBusinessMetrics`; HTTP request duration histogram; mensagens processadas counter
- **Logs**: OpenTelemetry log provider integrado ao `ILogger`
- **Semantic conventions**: `http.route`, `db.system`, `messaging.system`, `tenant.id`, `correlation.id`

**`SemanticConventions`** (classe com constantes):
- Atributos de span: `PlatformSpanAttributes` com `TenantId`, `CorrelationId`, `UserId`, `CommandName`, `EventName`
- Nomes de métricas: `PlatformMetricNames` com `CommandDurationMs`, `EventsPublished`, `EventsConsumed`, `DlqMessages`

**Testes** (≥ 8 casos usando `InMemorySpanCollector` e `InMemoryMetricCollector` do ObservabilityTestKit):
- Span criado para cada command handler
- Métricas incrementadas após publish de evento
- Correlation ID propagado nos spans

**Equivalência Java**: `commons-adapters-otel` com OpenTelemetry Java SDK

---

### 17. Implementar `MarcusPrado.Platform.Serilog` completo

**Dependências NuGet**: `Serilog.AspNetCore`, `Serilog.Sinks.Console`, `Serilog.Sinks.OpenTelemetry`, `Serilog.Enrichers.Thread`, `Serilog.Enrichers.Process`

**`SerilogConfigurator`**:
- `AddPlatformSerilog(IHostApplicationBuilder, SerilogOptions)` — configura logger com enrichers padrão
- Enrichers obrigatórios: `CorrelationId`, `TenantId`, `UserId`, `Environment`, `ApplicationName`, `MachineName`
- JSON output (produção) + colored console (desenvolvimento)
- `LogSanitizer` integrado — remove PII de mensagens antes de logar

**`RequestLoggingOptions`**:
- Paths excluídos de log: `/health`, `/ping`, `/metrics`
- Headers incluídos: `X-Correlation-ID`, `X-Tenant-ID`
- Body logging opt-in por rota

**Equivalência Java**: Logback + Logstash encoder + MDC enrichment

---

### 18. Implementar `MarcusPrado.Platform.HealthChecks` completo

**Dependências NuGet**: `Microsoft.AspNetCore.Diagnostics.HealthChecks`, `AspNetCore.HealthChecks.UI.Client`

**Checks implementados**:
- `LivenessCheck` — sempre `Healthy` se o processo está rodando
- `ReadinessCheck` — agrega `DependencyHealthProbe` de todos os serviços registrados
- `DependencyHealthProbe` — interface para Postgres, Redis, Kafka, RabbitMQ

**Extensions**:
- `HealthCheckExtensions.AddPlatformHealthChecks()` — registra `/health/live`, `/health/ready`, `/health/detail`
- Endpoint `/health/detail` retorna JSON detalhado (apenas em ambientes não-produção)

**Equivalência Java**: Spring Boot Actuator health checks

---

## 🛡️ Resiliência

### 19. Implementar `MarcusPrado.Platform.Resilience` completo

**Dependências NuGet**: `Polly` (≥ 8.x) + `Microsoft.Extensions.Http.Resilience`

**`RetryPolicy`**:
- `RetryOptions` com `MaxRetries`, `BackoffStrategy` (Fixed/Exponential/Jitter), `OnRetry` callback
- Integração com `IAppLogger` para log de tentativas

**`CircuitBreakerPolicy`**:
- `CircuitBreakerOptions` com `FailureThreshold`, `SamplingDuration`, `MinimumThroughput`, `BreakDuration`
- Eventos: `OnOpen`, `OnClose`, `OnHalfOpen`

**`HedgingPolicy`**:
- Dispara N requisições paralelas após delay; cancela as lentas quando a primeira responde
- `HedgingOptions` com `MaxHedgedAttempts`, `HedgingDelay`

**`AdaptiveConcurrencyLimiter`**:
- AIMD (Additive Increase / Multiplicative Decrease) algorithm
- `BackpressureSignal` para propagação de sobrecarga upstream

**`ResilientExecutor`**:
- Composição fluente de policies na ordem correta: Timeout → Retry → CircuitBreaker → Bulkhead → Execute
- `ExecuteAsync<T>(Func<CancellationToken, Task<Result<T>>> action, ResilienceContext context)`

**`RetryBehavior`** (pipeline behavior):
- Aplica `RetryPolicy` automaticamente em todos os command handlers com `[Retriable]`

**Testes** (≥ 15 casos):
- Retry com jitter não excede tempo máximo
- Circuit breaker abre após threshold
- Hedging retorna o mais rápido dos N
- `AdaptiveConcurrencyLimiter` rejeita sob sobrecarga extrema

**Equivalência Java**: `commons-app-resilience` com Resilience4j

---

## 📦 Extensões de Cache

### 20. Implementar `MarcusPrado.Platform.Redis` completo

**Dependências NuGet**: `StackExchange.Redis`, `Microsoft.Extensions.Caching.StackExchangeRedis`

**`RedisCache`**:
- Implementa `ICache` com `GetAsync<T>`, `SetAsync<T>`, `RemoveAsync`, `ExistsAsync`
- Serialização `System.Text.Json`
- Suporte a `IDistributedCache` (wrapper compatível)
- Cache lock (`SETNX` + expiry) para prevenção de cache stampede
- Prefixo por tenant via `TenantId`

**`RedisQuotaStore`**:
- Implementa `IQuotaStore` via `INCR` + `EXPIRE` (atomic)
- Scripts Lua para operações compostas atômicas
- Suporte a Fixed Window, Sliding Window, Token Bucket

**`RedisIdempotencyStore`**:
- Implementa `IIdempotencyStore` com TTL configurável
- Chave: `{tenantId}:{operationName}:{idempotencyKey}`

**Testes** (≥ 12 casos, `RedisContainer` do TestKit):
- Get/Set/Remove round-trip
- TTL expiration
- Quota atomic increment
- Idempotency key deduplication

**Equivalência Java**: `commons-adapters-cache-redis` com Lettuce/Jedis

---

## 🔐 Segurança e Secrets

### 21. Implementar adapters de secrets

**`MarcusPrado.Platform.Security.AzureKeyVault`**:
- Dependência: `Azure.Extensions.AspNetCore.Configuration.Secrets`, `Azure.Identity`
- Implementa `ISecretProvider` via Azure Key Vault
- Suporte a Managed Identity (`DefaultAzureCredential`)
- Cache local com TTL configurável para evitar rate limit

**`MarcusPrado.Platform.Security.AwsSecretsManager`**:
- Dependência: `AWSSDK.SecretsManager`
- Implementa `ISecretProvider` via AWS Secrets Manager
- Rotação automática de secrets (callback `OnRotation`)

**`MarcusPrado.Platform.Security.HashiCorpVault`**:
- Dependência: `VaultSharp`
- Implementa `ISecretProvider` via HashiCorp Vault
- Suporte a `AppRole` + `Kubernetes` auth methods

**Equivalência Java**: `commons-adapters-secrets-vault` / `commons-adapters-secrets-azure-keyvault` / `commons-adapters-secrets-aws-secretsmanager`

---

## 🧪 Testing

### 22. Implementar `MarcusPrado.Platform.TestKit` completo

**Dependências NuGet**: `Testcontainers` (≥ 3.x), `xunit`, `FluentAssertions`, `NSubstitute`

**Fixtures**:
- `IntegrationFixture` — gerencia lifecycle de containers; herda de `IAsyncLifetime`
- `ApiFixture<TProgram>` — `WebApplicationFactory<TProgram>` + `HttpClient` pré-configurado com correlation headers

**Containers** (via Testcontainers.Net):
- `PostgresContainer` — PostgreSQL latest, com banco isolado por teste
- `RedisContainer` — Redis latest, com `FlushAllAsync` no TearDown
- `KafkaContainer` — Confluent Kafka (Redpanda ou cp-kafka)
- `RabbitMqContainer` — RabbitMQ management

**Fakes**:
- `FakeClock` — implementa `IClock`; `SetNow(DateTimeOffset)`, `Advance(TimeSpan)`
- `FakeTenantContext` — implementa `ITenantContext`; configura `TenantId` manualmente
- `FakeUserContext` — implementa `IUserContext`; configura `UserId`, `Roles`, `Permissions`
- `FakeEventBus` — implementa `IEventBus`; captura eventos publicados para assertion

**Helpers**:
- `Eventually` — `public static async Task BecomesTrue(Func<bool> condition, TimeSpan timeout)` — polling assíncrono para assertions de estado eventual

**Testes do próprio TestKit** (≥ 8 casos):
- Container sobe e responde por conexão
- FakeClock avança o tempo corretamente
- FakeEventBus captura eventos publicados

**Equivalência Java**: `commons-test-kit` com Testcontainers Java + AssertJ

---

### 23. Implementar `MarcusPrado.Platform.ContractTestKit` completo

**Dependências NuGet**: `PactNet` (≥ 4.x)

**`PactVerifier`**:
- Provider-side verification de contratos HTTP (CDC - Consumer Driven Contracts)
- Integração com Pact Broker
- Configura `WebApplicationFactory` como provider

**`PactPublisher`**:
- Publica pacts para Pact Broker após testes de consumer
- Suporte a `GIT_BRANCH`, `GIT_COMMIT` nos metadados

**`AsyncContractVerifier`**:
- Verifica contratos de mensagens asíncronas (Kafka/RabbitMQ)
- Schema: `EventContractEnvelope` com `IEventContract`

**Equivalência Java**: `commons-test-contract` com Pact JVM

---

### 24. Implementar `MarcusPrado.Platform.ChaosKit` completo

**Dependências NuGet**: `Simmy` (Polly Chaos) ou implementação própria

**Faults**:
- `LatencyFault` — injeta delay artificial em execuções
- `ErrorFault` — injeta exceção especificada em % configurável de chamadas
- `PacketLossFault` — cancela operação simulando perda de rede

**`ChaosRunner`**:
- `RunWithChaos(config, action)` — executa ação com faults injetados
- Configuração por taxa de injeção (0–1.0)
- Integração com `ResilienceContext`

**Equivalência Java**: `commons-test-chaos` com Chaos Monkey for Spring Boot

---

### 25. Implementar `MarcusPrado.Platform.ObservabilityTestKit` completo

**Dependências NuGet**: `OpenTelemetry.Testing`

**Collectors**:
- `InMemorySpanCollector` — captura `Activity` geradas durante testes; `GetSpans()`, `GetSpanByName(name)`
- `InMemoryMetricCollector` — captura medições de `IMeter`; `GetMetric(name)`, `GetSum(name)`

**Assertions**:
- `MetricAssertions.HasCounter(name, expectedValue)` — FluentAssertions extension
- `TraceAssertions.HasSpan(name).WithAttribute(key, value)` — FluentAssertions extension

**Equivalência Java**: `commons-test-observability` com OpenTelemetry SDK testing

---

## 🌐 Adaptadores HTTP

### 26. Implementar adapter HTTP tipado

**Projeto**: `MarcusPrado.Platform.Http` (novo projeto em `src/extensions/`)

**Dependências NuGet**: `Microsoft.Extensions.Http`, `Polly`

**`TypedHttpClient<TClient>`**:
- Base class para HTTP clients tipados
- `IRequestContext` propagado automaticamente (correlation ID, tenant ID, auth token)
- Resilience: Retry + CircuitBreaker via `IHttpClientBuilder.AddResilienceHandler()`
- Logging estruturado de request/response

**`HttpClientFactoryExtensions.AddPlatformHttpClient<TClient>()`**:
- Registra `TypedHttpClient` com resilience defaults
- Configura `HttpMessageHandler` com enrichers de headers

**Equivalência Java**: `commons-adapters-http-okhttp` / `commons-adapters-http-webclient`

---

## 📤 Outbox + Inbox Pattern

### 27. Completar `MarcusPrado.Platform.OutboxInbox`

**`OutboxProcessor`** (implementação completa):
- `IHostedService` que processa `OutboxMessage` com `State = Pending`
- Polling configurável (padrão: 5s) com `IJobScheduler`
- Publica via `IOutboxPublisher` → `IMessagePublisher`
- Atualiza `State = Published` ou `State = Failed` com retry count
- Batch processing (configurable page size)
- Distributed lock via Redis para evitar duplo processamento em múltiplas instâncias

**`InboxProcessor`** (implementação completa):
- Deduplicação via `IIdempotencyStore` usando `MessageId`
- Dispatch para `IMessageHandler` correto via routing
- Atualiza `State = Processed` ou `State = Failed`

**`EfOutboxStore`** (no projeto EfCore):
- Query otimizada com índice em `(State, ScheduledAt)`
- Bulk update de status

**Testes** (≥ 10 casos):
- OutboxProcessor publica mensagem pendente
- InboxProcessor descarta duplicata
- Falha → tentativa com backoff
- Distributed lock previne duplo processamento

**Equivalência Java**: `commons-app-outbox` com Spring `@Scheduled` + JPA

---

## 🎨 Domain-Driven Design

### 28. Implementar pipeline CQRS completo

**Módulo**: `MarcusPrado.Platform.Application`

**`IDispatcher`** (implementação padrão `Dispatcher`):
- Resolve `ICommandHandler<TCommand, TResult>` via `IServiceProvider`
- Resolve `IQueryHandler<TQuery, TResult>` via `IServiceProvider`
- Executa pipeline de behaviors: Logging → Metrics → Tracing → Validation → Authorization → Idempotency → Transaction → Retry → Handler

**`ValidationBehavior`**:
- Resolve todos os `IValidator<TCommand>` registrados
- Executa todos em paralelo, agrega falhas
- Retorna `Result.Failure` com todos os erros de validação

**`IdempotencyBehavior`**:
- Commands marcados com `[Idempotent(TimeToLive)]` verificam `IIdempotencyStore`
- Retorna resposta cacheada se chave já existir

**`TransactionBehavior`**:
- Commands marcados com `[Transactional]` envolvidos em `IUnitOfWork.BeginTransactionAsync()`
- Commit em sucesso, Rollback (com log) em falha

**`MetricsBehavior`** + **`TracingBehavior`** + **`LoggingBehavior`**:
- Métricas: `command.duration_ms`, `command.success`, `command.failure` counter
- Trace: `Activity` com nome `{CommandType}`, atributos de contexto
- Log: request/response estruturado com `IAppLogger`

**Equivalência Java**: `commons-app-cqrs` com pipeline sem MediatR

---

## ⚙️ Configuração Runtime

### 29. Implementar `MarcusPrado.Platform.Runtime` completo

**`IAppConfiguration`** baseada em `IConfiguration`:
- Acesso type-safe a seções com validação via `ValidateDataAnnotations()`
- `ConfigurationKey<T>` — wrapper type-safe para chaves de configuração
- `EnvConfiguration` — sobrescreve valores via variáveis de ambiente (12-Factor)

**`GracefulShutdown`**:
- Registra handlers em `IHostApplicationLifetime.ApplicationStopping`
- Timeout configurável (padrão: 30s) para drenagem de requests em voo

**`InstanceInfo`**:
- Popula automaticamente: `ServiceName`, `ServiceVersion`, `Region`, `PodName`, `NodeName` (de env vars Kubernetes padrão: `POD_NAME`, `NODE_NAME`)

**Equivalência Java**: `commons-app-configuration` + Spring Boot `@ConfigurationProperties`

---

## 🔄 Governança e Contratos

### 30. Implementar `MarcusPrado.Platform.Governance` completo

**`IContractRegistry`** (implementação `InMemoryContractRegistry`):
- Registra `ContractMetadata` com nome, versão, schema hash, status (`Active`/`Deprecated`/`Retired`)
- `GetAll()`, `GetByName(name)`, `Register(metadata)`, `Deprecate(name, notice)`

**`ContractCompatibilityChecker`**:
- Compara dois schemas JSON (atual vs anterior)
- Detecta breaking changes: remoção de campo, mudança de tipo, renomeação
- Gera `CompatibilityReport` com lista de violações

**`DeprecationSchedule`**:
- Define janela de deprecação: `DeprecationDate` → `RetirementDate`
- `IsWithinDeprecationWindow(now)` — emite warning em logs/métricas
- `IsRetired(now)` — rejeita requests com 410 Gone

**`AdrRecord`**:
- `record AdrRecord(int Number, string Title, AdrStatus Status, DateOnly Date, IReadOnlyList<string> DecisionMakers, string Context, string Decision, string Consequences)`
- `IAdrStore` implementado com arquivos Markdown em `docs/architecture/adr/`

**Equivalência Java**: `commons-app-governance` (novo)

---

## 📦 Adaptadores Adicionais (Backlog Futuro)

### 31. Implementar adapter de Blockchain (Nethereum)

**Projetos**:
- `MarcusPrado.Platform.Abstractions.Blockchain` — `IBlockchainClient`, `IWalletManager`, `ISmartContractClient`
- `MarcusPrado.Platform.Nethereum` — Adapter com Nethereum 4.x

**Domain Models**: `BlockchainTransaction`, `TransactionReceipt`, `Wallet`, `SmartContract`

**Equivalência Java**: `commons-adapters-blockchain-web3j` (Web3j → Nethereum)

---

### 32. Implementar adapter de Pagamento (Stripe.net)

**Projetos**:
- `MarcusPrado.Platform.Abstractions.Payment` — `IPaymentService`, `ISubscriptionService`, `IRefundService`
- `MarcusPrado.Platform.Stripe` — Adapter com `Stripe.net` (≥ 46.x)

**Domain Models**: `Payment`, `PaymentMethod`, `Subscription`, `Refund`, `PaymentStatus` (enum)

**Implementar**: `StripePaymentService`, `StripeSubscriptionService`, `StripeRefundService`

**Equivalência Java**: `commons-adapters-payment-stripe`

---

### 33. Implementar adapter de Email

**Projetos**:
- `MarcusPrado.Platform.Abstractions.Email` — `IEmailSender`, `IEmailTemplateRenderer`
- `MarcusPrado.Platform.MailKit` — SMTP via MailKit
- `MarcusPrado.Platform.SendGrid` — HTTP API via SendGrid

**Domain Models**: `EmailMessage`, `EmailAttachment`, `EmailStatus`

**Equivalência Java**: `commons-adapters-email-smtp` / `commons-adapters-email-sendgrid`

---

### 34. Implementar adapter de SMS

**Projetos**:
- `MarcusPrado.Platform.Abstractions.Sms` — `ISmsService`
- `MarcusPrado.Platform.Twilio` — via Twilio SDK
- `MarcusPrado.Platform.AwsSns` — via AWSSDK.SimpleNotificationService

**Equivalência Java**: `commons-adapters-sms-twilio` / `commons-adapters-sms-aws-sns`

---

### 35. Implementar adapter de Search

**Projetos**:
- `MarcusPrado.Platform.Abstractions.Search` — `ISearchClient`, `IIndexManager`
- `MarcusPrado.Platform.Elasticsearch` — via `Elastic.Clients.Elasticsearch` (≥ 8.x)
- `MarcusPrado.Platform.OpenSearch` — via `OpenSearch.Client`

**Equivalência Java**: `commons-adapters-search-elasticsearch` / `commons-adapters-search-opensearch`

---

### 36. Implementar adapter gRPC

**Projetos**:
- `MarcusPrado.Platform.Grpc.Server` — Interceptors: `CorrelationInterceptor`, `AuthInterceptor`, `LoggingInterceptor`, `MetricsInterceptor`
- `MarcusPrado.Platform.Grpc.Client` — `GrpcClientFactory` com Polly resilience

**Dependências**: `Grpc.AspNetCore`

**Equivalência Java**: `commons-adapters-grpc-server` / `commons-adapters-grpc-client`

---

### 37. Implementar adapter GraphQL

**Projeto**: `MarcusPrado.Platform.HotChocolate`

**Dependências**: `HotChocolate.AspNetCore`

**Implementar**: `PlatformTypeInterceptor` (injeta auth, tenant, correlation), `PlatformErrorFilter`

**Equivalência Java**: `commons-adapters-graphql-server`

---

### 38. Implementar utilitários de documento

**`MarcusPrado.Platform.Pdf`**:
- `IPdfGenerator` + `QuestPdfGenerator` (via QuestPDF)
- Template-based generation

**`MarcusPrado.Platform.Excel`**:
- `IExcelWriter` / `IExcelReader` + `ClosedXmlExcelAdapter` (via ClosedXML)

**Equivalência Java**: `commons-adapters-pdf-itext` / `commons-adapters-excel-poi`

---

### 39. Implementar serialização Protobuf

**Projeto**: `MarcusPrado.Platform.Protobuf`

**Dependências**: `protobuf-net`

**Implementa**: `IMessageSerializer` usando protobuf-net

**Equivalência Java**: `commons-adapters-serialization-protobuf`

---

### 40. Implementar Feature Flags completo

**Módulo**: `MarcusPrado.Platform.FeatureFlags`

**Providers**:
- `InMemoryFeatureFlagProvider` — para testes
- `EnvironmentFeatureFlagProvider` — flags via environment variables
- `AzureAppConfigurationFeatureFlagProvider` — via Azure App Configuration Feature Management
- `LaunchDarklyFeatureFlagProvider` — via LaunchDarkly .NET SDK

**`FeatureFlagContext`**: `TenantId`, `UserId`, `Environment`, `Region`, custom attributes

**Rollout strategies**: boolean, percentage, tenant whitelist, user whitelist, canary

**Equivalência Java**: `commons-app-feature-flags`

---

### 41. Implementar Workflow Engine

**Módulo**: `MarcusPrado.Platform.Application.Workflow` (novo projeto em `src/core/`)

**`IWorkflowEngine`**:
- `StartWorkflowAsync(definitionId, initialContext) → Result<WorkflowInstance>`
- `SendEventAsync(workflowId, event, data) → Result<WorkflowInstance>`
- `CompensateAsync(workflowId) → Result<WorkflowInstance>` — Saga compensation
- `CancelAsync(workflowId, reason) → Result<WorkflowInstance>`

**`WorkflowDefinition`** + **`WorkflowInstance`** (C# records com `with` expressions)

**`DefaultWorkflowEngine`** (in-memory, thread-safe via `ConcurrentDictionary`)

**Equivalência Java**: `commons-app-workflow-engine`

---

### 42. Implementar Backup & Restore

**Módulo**: `MarcusPrado.Platform.Application.BackupRestore` (novo projeto em `src/core/`)

**`IBackupService`**: `CreateFullBackupAsync`, `CreateIncrementalBackupAsync`, `ListBackupsAsync`, `VerifyBackupAsync`, `DeleteBackupAsync`

**`IRestoreService`**: `RestoreAsync`, `RestorePointInTimeAsync`, `ValidateRestoreAsync`

**`FilesystemBackupService`**: ZIP via `System.IO.Compression`, SHA-256 checksum, incremental via `FileInfo.LastWriteTimeUtc`

**Equivalência Java**: `commons-app-backup-restore`

---

### 43. Implementar Multi-tenancy avançado

**Módulo**: `MarcusPrado.Platform.MultiTenancy`

**Strategies**:
- `SchemaPerTenantStrategy` — PostgreSQL schema switching via `SET search_path`
- `DatabasePerTenantStrategy` — connection string routing por `TenantId`
- `DiscriminatorStrategy` — EF Core global query filter

**`TenantQuota`** enforcement via `RateLimitingMiddleware` + `RedisQuotaStore`

**Equivalência Java**: `commons-app-multi-tenancy`

---

### 44. Implementar Audit Log

**Módulo**: `MarcusPrado.Platform.Application.AuditLog` (novo projeto em `src/core/`)

**`IAuditLogger`**: `LogAsync(AuditEntry)` — `Action`, `Resource`, `ResourceId`, `ActorId`, `TenantId`, `Timestamp`, `Changes` (JSON diff), `IpAddress`, `UserAgent`

**Sinks**: `EfCoreAuditSink` (DB), `SerilogAuditSink` (arquivo), `OpenTelemetryAuditSink` (spans)

**Equivalência Java**: `commons-app-audit-log`

---

### 45. Implementar Service Discovery

**Projetos**:
- `MarcusPrado.Platform.Abstractions.ServiceDiscovery` — `IServiceDiscovery`, `ServiceEndpoint`
- `MarcusPrado.Platform.Consul` — `IServiceDiscovery` via `Consul` NuGet

**Equivalência Java**: `commons-adapters-service-discovery-consul`

---

## 🔧 Tooling Avançado

### 46. Completar Roslyn Analyzers

**Regras a implementar** (projeto `MarcusPrado.Platform.Analyzers`):

- `PLATFORM001` — `NoEfCoreInDomainAnalyzer`: detecta uso de `DbContext`, `DbSet`, `IQueryable` em projetos com namespace `*.Domain`
- `PLATFORM002` — `NoAspNetInDomainAnalyzer`: detecta `IActionResult`, `ControllerBase`, `HttpContext` no Domain
- `PLATFORM003` — `DomainMustNotReferenceInfrastructureAnalyzer`: verifica referências de projeto no `.csproj` do Domain
- `PLATFORM004` — `EnforceResultTypeAnalyzer`: command/query handlers que lançam exception em vez de retornar `Result<T>`
- `PLATFORM005` — `EnforceIdempotencyKeyUsageAnalyzer`: commands `[Idempotent]` sem propriedade `IdempotencyKey`

**Code Fixes**:
- `AddResultWrapperCodeFix` — wraps `return value` em `return Result.Success(value)` automaticamente

**Testes** (usando `Microsoft.CodeAnalysis.Testing`):
- Cada analyzer com casos positivos e negativos
- Code fix com snapshot do before/after

**Equivalência Java**: Checkstyle rules + ErrorProne annotations

---

### 47. Completar `Directory.Packages.props` (Central Package Management)

**Arquivo**: `/Directory.Packages.props`

Centralizar versões de todos os NuGets utilizados na solução:

```xml
<Project>
  <PropertyGroup>
    <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
  </PropertyGroup>
  <ItemGroup>
    <PackageVersion Include="Microsoft.EntityFrameworkCore" Version="9.0.0" />
    <PackageVersion Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="9.0.0" />
    <PackageVersion Include="StackExchange.Redis" Version="2.8.0" />
    <PackageVersion Include="Confluent.Kafka" Version="2.6.0" />
    <PackageVersion Include="RabbitMQ.Client" Version="7.0.0" />
    <PackageVersion Include="Serilog.AspNetCore" Version="9.0.0" />
    <PackageVersion Include="OpenTelemetry.Extensions.Hosting" Version="1.10.0" />
    <PackageVersion Include="Polly" Version="8.5.0" />
    <PackageVersion Include="Testcontainers" Version="3.10.0" />
    <PackageVersion Include="FluentAssertions" Version="7.0.0" />
    <PackageVersion Include="xunit" Version="2.9.0" />
    <PackageVersion Include="NSubstitute" Version="5.3.0" />
    <PackageVersion Include="NetArchTest.Rules" Version="1.4.0" />
    <PackageVersion Include="BenchmarkDotNet" Version="0.14.0" />
    <PackageVersion Include="PactNet" Version="4.6.0" />
    <PackageVersion Include="Stripe.net" Version="46.0.0" />
    <PackageVersion Include="Nethereum.Web3" Version="4.26.0" />
  </ItemGroup>
</Project>
```

**Equivalência Java**: `commons-bom/pom.xml` com `<dependencyManagement>`

---

### 48. Implementar templates `dotnet new`

**Projeto**: `src/tooling/MarcusPrado.Platform.Templates`

**Templates**:
- `platform-api` — Minimal API pré-configurado com todos os middlewares, OTel, Serilog, HealthChecks
- `platform-worker` — Worker Service com BackgroundJobs, OTel, Serilog
- `platform-domain` — Classe de domínio com `AggregateRoot`, events, specification
- `platform-command` — Command + Handler + Validator pré-wired

**Instalação**: `dotnet new install MarcusPrado.Platform.Templates`

**Equivalência Java**: `commons-platform-archetype` com Maven Archetype

---

## 🗄️ Adaptadores de Banco de Dados Adicionais

### 49. Implementar adapter MongoDB

**Projetos**:
- `MarcusPrado.Platform.Abstractions.Document` — `IDocumentRepository<T>`, `IDocumentSession`, `IDocumentFilter<T>`
- `MarcusPrado.Platform.MongoDb` — Adapter com `MongoDB.Driver` (≥ 3.x)

**Implementar**:
- `MongoDocumentRepository<T>` — `FindAsync`, `FindOneAsync`, `InsertAsync`, `ReplaceAsync`, `DeleteAsync`, `AggregateAsync`
- `MongoSession` — implementa `IUnitOfWork` com sessões e transações multi-documento
- `MongoPaginatedQuery<T>` — suporte a cursor-based e offset-based pagination
- `MongoTenantFilter` — query filter global por `TenantId` via `BsonDocument`
- `MongoHealthProbe` — `ping` command com timeout
- `MongoExtensions.AddPlatformMongoDB()` — registra `MongoClient`, `IMongoDatabase`, health probe

**Testes** (≥ 10 casos, `MongoDbContainer` do TestKit):
- CRUD round-trip
- Transação multi-documento rollback
- Tenant filter aplicado corretamente
- Paginação por cursor

---

### 50. Implementar adapter SQL Server (Dapper)

**Projeto**: `MarcusPrado.Platform.SqlServer`

**Dependências NuGet**: `Dapper`, `Microsoft.Data.SqlClient`

**Implementar**:
- `SqlServerConnectionFactory` — pool de conexões via `SqlConnection`
- `DapperRepository<T>` — implementa `IReadRepository<T>` com Dapper para leituras de alta performance
- `SqlServerHealthProbe` — `SELECT @@VERSION` com timeout
- `SqlServerExtensions.AddPlatformSqlServer()` — registra connection factory, health probe
- `BulkInsertExtensions` — `SqlBulkCopy` para inserções em massa

**Equivalência Java**: `commons-adapters-persistence-sqlserver` com JDBC + Dapper equivalent

---

### 51. Implementar adapter MySQL/MariaDB

**Projeto**: `MarcusPrado.Platform.MySql`

**Dependências NuGet**: `Pomelo.EntityFrameworkCore.MySql`

**Implementar**:
- `MySqlConnectionFactory` — pool de conexões via `MySqlConnection`
- `MySqlHealthProbe` — `SELECT 1` com timeout
- `MySqlExtensions.AddPlatformMySql()` — registra `MySqlDataSource`, `AppDbContext` com Pomelo provider

**Equivalência Java**: `commons-adapters-persistence-mysql`

---

## 📨 Adaptadores de Mensageria Avançados

### 52. Implementar adapter NATS

**Projetos**:
- `MarcusPrado.Platform.Nats` — Adapter com `NATS.Net` (≥ 2.x)

**Implementar**:
- `NatsPublisher` — implementa `IMessagePublisher`; suporte a JetStream (at-least-once)
- `NatsConsumer` — implementa `IMessageConsumer`; consumer groups via JetStream consumer
- `NatsHealthProbe` — `PING` com timeout
- `NatsExtensions.AddPlatformNats()`

**Testes** (≥ 8 casos, `NatsContainer` do TestKit):
- Publish/consume round-trip
- JetStream persistent delivery

---

### 53. Implementar adapter Azure Service Bus

**Projeto**: `MarcusPrado.Platform.AzureServiceBus`

**Dependências NuGet**: `Azure.Messaging.ServiceBus`

**Implementar**:
- `ServiceBusPublisher` — implementa `IMessagePublisher`; suporte a `Queue` e `Topic`
- `ServiceBusConsumer` — implementa `IMessageConsumer`; `ServiceBusProcessor` com lock renewal automático
- `ServiceBusDeadLetterSink` — implementa `IDeadLetterSink`
- `ServiceBusExtensions.AddPlatformServiceBus()` — suporte a `DefaultAzureCredential`

**Equivalência Java**: `commons-adapters-messaging-azure-servicebus`

---

### 54. Implementar adapter AWS SQS/SNS

**Projetos**:
- `MarcusPrado.Platform.AwsSqs` — via `AWSSDK.SQS`
- `MarcusPrado.Platform.AwsSns` (messaging) — via `AWSSDK.SimpleNotificationService`

**Implementar**:
- `SqsConsumer` — long polling, visibility timeout, DLQ automático
- `SqsPublisher` / `SnsPublisher` — implementam `IMessagePublisher`
- Fan-out pattern: SNS Topic → múltiplas filas SQS
- `AwsMessagingExtensions.AddPlatformAwsMessaging()`

**Equivalência Java**: `commons-adapters-messaging-sqs` / `commons-adapters-messaging-sns`

---

## 🌐 Adaptadores Web Avançados

### 55. Implementar adapter SignalR / WebSockets

**Projeto**: `MarcusPrado.Platform.SignalR`

**Dependências NuGet**: `Microsoft.AspNetCore.SignalR`

**Implementar**:
- `PlatformHub<T>` — base class com authenticação JWT, tenant isolation, correlation propagation
- `IRealtimePublisher` — publica eventos domain → clientes SignalR conectados
- `SignalRDomainEventSink` — listener de `IDomainEvent` → broadcast via hub
- `SignalRExtensions.AddPlatformSignalR()` — registra hubs e autenticação

**Equivalência Java**: `commons-adapters-websocket-stomp`

---

### 56. Implementar configuração CORS avançada

**Módulo**: `MarcusPrado.Platform.AspNetCore`

**Implementar**:
- `PlatformCorsPolicy` — política CORS configurável por ambiente: DevPermissive / StagingRestricted / ProductionLocked
- `TenantAwareCorsPolicy` — origins permitidas por tenant via `ITenantContext`
- `CorsExtensions.AddPlatformCors(options)` — regista policies e middleware em ordem correta

**Testes** (≥ 5 casos):
- Preflight request retorna headers corretos
- Origin não permitida → 403
- Tenant-specific origin aceita

---

### 57. Implementar convenções de Minimal API (Endpoint base class)

**Módulo**: `MarcusPrado.Platform.AspNetCore`

**Implementar**:
- `IEndpoint` — interface `MapEndpoints(IEndpointRouteBuilder app)`
- `EndpointGroupBase` — base class com `RouteGroupBuilder` e route prefix automático
- `EndpointDiscovery.MapPlatformEndpoints()` — descoberta automática de `IEndpoint` via reflection nos assemblies
- `ApiEnvelopeFilter` — `IEndpointFilter` que wrapa responses em `ApiEnvelope<T>`
- `ValidationFilter<TRequest>` — valida `TRequest` via `IValidator<T>` antes do handler

**Equivalência Java**: `commons-adapters-rest-endpoint-convention`

---

### 58. Implementar versionamento de API

**Módulo**: `MarcusPrado.Platform.AspNetCore`

**Dependências NuGet**: `Asp.Versioning.Http`, `Asp.Versioning.Mvc`

**Implementar**:
- `ApiVersioningExtensions.AddPlatformApiVersioning()` — URL + header + media-type versioning
- `DeprecationHeaderMiddleware` — adiciona `Deprecation` e `Sunset` headers via `DeprecationSchedule`
- `IApiVersionPolicy` com estratégias: `UrlSegment`, `Header`, `QueryString`
- `ApiVersionDiscovery` — lista versões ativas no `/api-versions` endpoint

**Equivalência Java**: `commons-adapters-rest-versioning` com Spring MVC versioning

---

### 59. Implementar OpenAPI / Scalar

**Módulo**: `MarcusPrado.Platform.AspNetCore`

**Dependências NuGet**: `Microsoft.AspNetCore.OpenApi`, `Scalar.AspNetCore`

**Implementar**:
- `OpenApiExtensions.AddPlatformOpenApi()` — configura OpenAPI 3.1 com autenticação JWT + API Key
- `PlatformOperationTransformer` — adiciona `X-Correlation-ID`, `X-Tenant-ID` nos headers de todos os endpoints
- `ProblemDetailsSchemaFilter` — documenta responses de erro conforme RFC 9457
- `ScalarExtensions.UseScalarApiReference()` — UI de documentação no `/scalar`

---

### 60. Implementar Rate Limiting ASP.NET Core

**Módulo**: `MarcusPrado.Platform.RateLimiting`

**Dependências NuGet**: `System.Threading.RateLimiting` (built-in .NET 7+)

**Implementar**:
- `RateLimitingExtensions.AddPlatformRateLimiting()` — registra policies: `global`, `per-tenant`, `per-user`, `per-ip`
- `TenantRateLimitPolicy` — `IPartitionedRateLimiter<HttpContext>` por `TenantId` via `RedisQuotaStore`
- `UserRateLimitPolicy` — por `UserId` com sliding window
- Middleware de resposta 429 com `Retry-After` header e `ProblemDetails` body

**Equivalência Java**: `commons-app-rate-limiting` com Bucket4j + Redis

---

### 61. Implementar middleware de Compressão de Response

**Módulo**: `MarcusPrado.Platform.AspNetCore`

**Implementar**:
- `ResponseCompressionExtensions.AddPlatformResponseCompression()` — Brotli (primário) + Gzip (fallback)
- Lista de MIME types comprimíveis: `application/json`, `text/plain`, `application/x-protobuf`
- Threshold mínimo configurável (padrão: 1KB)

---

### 62. Implementar middleware de Security Headers

**Módulo**: `MarcusPrado.Platform.AspNetCore`

**Implementar**:
- `SecurityHeadersMiddleware` — adiciona headers: `X-Content-Type-Options: nosniff`, `X-Frame-Options: DENY`, `X-XSS-Protection: 0`, `Referrer-Policy: no-referrer`, `Permissions-Policy`, `Content-Security-Policy`
- `HstsExtensions.AddPlatformHsts()` — HSTS com `max-age=31536000; includeSubDomains; preload` em produção
- `SecurityHeadersOptions` — configurável por ambiente (desabilitar CSP em dev)

---

### 63. Implementar middleware de IP Filtering

**Módulo**: `MarcusPrado.Platform.AspNetCore`

**Implementar**:
- `IpFilterMiddleware` — whitelist e blacklist de CIDRs configuráveis
- `IIpFilterStore` — interface; `InMemoryIpFilterStore` + `RedisIpFilterStore`
- `TenantIpPolicy` — política IP por tenant
- Resposta 403 com `ProblemDetails` body para IPs bloqueados
- Integração com `X-Forwarded-For` e `X-Real-IP` (reverse proxy aware)

---

### 64. Implementar middleware de Request Size Limiting

**Módulo**: `MarcusPrado.Platform.AspNetCore`

**Implementar**:
- `RequestSizeLimitAttribute` override — limite configurável por endpoint via atributo ou options
- `TenantRequestSizePolicy` — limites por tier de tenant (Free/Pro/Enterprise)
- Resposta 413 com `ProblemDetails` body para payloads acima do limite

---

## 📡 Observabilidade Avançada

### 65. Implementar propagação W3C TraceContext

**Módulo**: `MarcusPrado.Platform.Observability`

**Implementar**:
- `W3CTraceContextPropagator` — extrai/injeta `traceparent` e `tracestate` em headers HTTP e mensagens de fila
- `KafkaTracePropagator` — propaga contexto via `MessageHeaders` em mensagens Kafka
- `RabbitMqTracePropagator` — propaga via `IBasicProperties.Headers`
- `ActivityExtensions` — helpers: `SetTenantId`, `SetUserId`, `SetCorrelationId`, `SetErrorStatus`

**Equivalência Java**: `commons-adapters-otel-propagation`

---

### 66. Implementar Distributed Tracing para banco de dados

**Módulo**: `MarcusPrado.Platform.EfCore` / `MarcusPrado.Platform.Postgres`

**Implementar**:
- `EfCoreTracingInterceptor` — `IDbCommandInterceptor` que cria spans para cada query EF Core com SQL sanitizado
- `DapperTracingWrapper` — extension methods `QueryWithTraceAsync` / `ExecuteWithTraceAsync`
- Atributos de span: `db.system=postgresql`, `db.operation`, `db.statement` (sanitizado), `db.rows_affected`

---

### 67. Implementar métricas de negócio (Business Metrics)

**Módulo**: `MarcusPrado.Platform.Observability`

**Implementar**:
- `IBusinessMetrics` (implementação concreta `OtelBusinessMetrics`):
  - `RecordOrderPlaced(tenantId, value, currency)`
  - `RecordPaymentProcessed(tenantId, status, gateway)`
  - `RecordUserSignup(tenantId, plan)`
  - `RecordEventConsumed(topic, consumerGroup, latencyMs)`
- `BusinessMetricsExtensions.AddPlatformBusinessMetrics()` — registra todos os instrumentos
- Dashboard Grafana template (JSON) em `docs/dashboards/platform-business.json`

**Equivalência Java**: `commons-app-business-metrics`

---

### 68. Implementar SLO / Error Budget tracking

**Módulo**: `MarcusPrado.Platform.Observability`

**Implementar**:
- `ServiceLevelObjective` record — `Name`, `Target` (double 0–1), `Window` (TimeSpan), `MetricQuery`
- `ErrorBudgetCalculator` — `CalculateRemainingBudget(SLO slo, double currentAvailability)`
- `SloMetricsCollector` — coleta e expõe via OTel Metrics: `slo.availability`, `slo.error_budget_remaining`
- Alertas baseados em burn rate (4x, 14.4x, 1x)

---

## 🔐 Segurança Avançada

### 69. Implementar OAuth2 / OIDC Client Credentials

**Módulo**: `MarcusPrado.Platform.Security`

**Dependências NuGet**: `Microsoft.AspNetCore.Authentication.OpenIdConnect`, `Duende.AccessTokenManagement`

**Implementar**:
- `OidcClientService` — client credentials flow com token caching automático
- `TokenCache` — cache de access tokens com renovação proativa 30s antes do expiry
- `MachineToMachineHttpHandler` — `DelegatingHandler` que injeta token Bearer automaticamente
- `OidcExtensions.AddPlatformOidcClient()` — registra com `DefaultAzureCredential` ou Keycloak

**Equivalência Java**: `commons-adapters-security-oauth2-client`

---

### 70. Implementar criptografia de dados em repouso

**Módulo**: `MarcusPrado.Platform.Security`

**Implementar**:
- `IDataEncryption` — `EncryptAsync(plaintext)` / `DecryptAsync(ciphertext)`
- `AesGcmEncryption` — implementação com AES-256-GCM (nonce aleatório por operação)
- `EncryptedAttribute` — para marcar propriedades de entidades EF Core que devem ser criptografadas
- `EncryptingValueConverter` — `ValueConverter` do EF Core que aplica `IDataEncryption` automaticamente
- `KeyRotationService` — re-criptografia de dados ao rotacionar DEK (Data Encryption Key)

**Equivalência Java**: `commons-adapters-security-encryption`

---

### 71. Implementar Digital Signatures (RSA/ECDSA)

**Módulo**: `MarcusPrado.Platform.Security`

**Implementar**:
- `ISignatureService` — `SignAsync(payload)` / `VerifyAsync(payload, signature)`
- `RsaSignatureService` — RSA-PSS com SHA-256
- `EcdsaSignatureService` — ECDSA com P-256
- `WebhookSignatureMiddleware` — verifica assinatura HMAC-SHA256 em webhooks recebidos (`X-Signature` header)
- `SignedPayloadEnvelope<T>` — wrapper para payloads assinados com timestamp anti-replay

**Equivalência Java**: `commons-adapters-security-signature`

---

### 72. Implementar PII Masking completo

**Módulo**: `MarcusPrado.Platform.Security`

**Implementar**:
- `[PiiData]` attribute — marca propriedades como PII no modelo
- `PiiClassifier` — identifica campos PII por nome (email, cpf, phone, ssn) e por atributo
- `PiiRedactor` — mascara valores: `email@domain.com → e***@d***.com`, `123.456.789-00 → ***.***.***-00`
- `SerilogPiiDestructuringPolicy` — policy do Serilog que aplica `PiiRedactor` antes de logar
- `GdprComplianceReport` — lista todos os campos PII em entidades registradas no `DbContext`

**Equivalência Java**: `commons-app-pii-masking`

---

### 73. Implementar mTLS support

**Módulo**: `MarcusPrado.Platform.AspNetCore.Auth`

**Implementar**:
- `MtlsAuthenticationHandler` — valida certificado cliente via `IClientCertificateFeature`
- `CertificateTenantResolver` — extrai `TenantId` do Subject Alternative Name (SAN) do certificado
- `CertificateRevocationChecker` — verifica CRL / OCSP em certificados cliente
- `MtlsExtensions.AddPlatformMtls()` — configura Kestrel para exigir certificado cliente

**Equivalência Java**: `commons-adapters-security-mtls`

---

### 74. Implementar sanitização de input / prevenção XSS

**Módulo**: `MarcusPrado.Platform.Security`

**Dependências NuGet**: `HtmlSanitizer`

**Implementar**:
- `IInputSanitizer` — `Sanitize(input)` / `SanitizeHtml(html)` / `StripTags(html)`
- `HtmlSanitizerAdapter` — implementa `IInputSanitizer` via HtmlSanitizer
- `SanitizingModelBinder` — model binder ASP.NET Core que aplica sanitização automaticamente em `string` inputs marcados com `[SanitizeInput]`
- `SqlInjectionDetector` — detecta padrões comuns em strings antes de usar em queries dapper

**Equivalência Java**: `commons-adapters-security-input-sanitization`

---

## 🔄 Event Sourcing e CQRS Avançado

### 75. Implementar Event Sourcing

**Módulo**: `MarcusPrado.Platform.Application.EventSourcing` (novo projeto em `src/core/`)

**Implementar**:
- `IEventStore` — `AppendAsync(streamId, events, expectedVersion)` / `ReadAsync(streamId, fromVersion)`
- `EventStoreDbAdapter` — implementa `IEventStore` via EventStoreDB (kurrent); alternativa PostgreSQL via `Marten`
- `EventSourcedRepository<T>` — reconstitui aggregate a partir dos events
- `EventSnapshot<T>` — snapshot a cada N eventos para performance
- `AggregateEventReplayer` — replay de events para reconstruir estado em ponto no tempo

**Dependências NuGet**: `EventStore.Client.Grpc.Streams` ou `Marten`

**Equivalência Java**: `commons-app-event-sourcing` com Axon Framework / EventStoreDB

---

### 76. Implementar Projeções / Read Models

**Módulo**: `MarcusPrado.Platform.Application.Projections` (novo projeto em `src/core/`)

**Implementar**:
- `IProjection<TEvent, TReadModel>` — interface de projeção
- `ProjectionEngine` — processa eventos e atualiza read models via `IReadModelStore`
- `IReadModelStore<T>` — `GetAsync(id)`, `UpsertAsync(model)`, `QueryAsync(filter)`
- `EfReadModelStore<T>` — implementa via EF Core
- `RedisReadModelStore<T>` — implementa via Redis para leituras ultra-rápidas
- `ProjectionRebuildJob` — job que reconstrói projeções do zero a partir do event store

**Equivalência Java**: `commons-app-projections` com CQRS read side

---

### 77. Implementar Saga Orchestration

**Módulo**: `MarcusPrado.Platform.Application.Saga` (novo projeto em `src/core/`)

**Implementar**:
- `ISaga<TState>` — interface com `Handle(event)` e `Compensate(failedStep)`
- `SagaOrchestrator` — executa steps em sequência, persiste estado entre steps
- `ISagaStore` — `SaveAsync(saga)` / `LoadAsync(sagaId)`
- `EfSagaStore` — persiste state via EF Core
- `SagaCompensationHandler` — handler automático de compensação em falha
- `SagaStep<TCommand>` — step tipado: `Execute`, `Compensate`, `Timeout`

**Equivalência Java**: `commons-app-saga` com Axon Saga

---

### 78. Implementar Domain Event Router

**Módulo**: `MarcusPrado.Platform.Application`

**Implementar**:
- `DomainEventRouter` — registra handlers por tipo de evento; resolve via DI
- `IDomainEventHandler<TEvent>` — interface análoga a `ICommandHandler` para domain events
- `DomainEventDispatcher` — despacha domain events acumulados no aggregate após `SaveChanges`
- `CrossBoundaryEventBridge` — converte `IDomainEvent` → `IEventContract` para publicar no barramento externo (Kafka/RabbitMQ)
- `EventHandlerPipeline` — pipeline de behaviors para domain event handlers (logging, retry, metrics)

**Equivalência Java**: `commons-app-domain-event-dispatcher`

---

## ⏰ Background Jobs Avançados

### 79. Implementar adapter Quartz.NET

**Projeto**: `MarcusPrado.Platform.QuartzNet`

**Dependências NuGet**: `Quartz.Extensions.Hosting`

**Implementar**:
- `QuartzJobScheduler` — implementa `IJobScheduler` via Quartz.NET
- `PlatformJobFactory` — factory que resolve `IJobHandler<T>` via DI
- `QuartzClusterStore` — `AdoJobStore` com PostgreSQL para clustering
- `JobTriggerBuilder` — fluent API para criar triggers CRON / Simple / Calendar
- `QuartzExtensions.AddPlatformQuartz()` — registra com clustering habilitado por padrão

**Equivalência Java**: `commons-app-scheduled-jobs` com Quartz (Java)

---

### 80. Implementar adapter Hangfire

**Projeto**: `MarcusPrado.Platform.Hangfire`

**Dependências NuGet**: `Hangfire.AspNetCore`, `Hangfire.PostgreSql`

**Implementar**:
- `HangfireJobScheduler` — implementa `IJobScheduler` via Hangfire
- `HangfireRecurringJobRegistrar` — auto-registra jobs com `[RecurringJob(cron)]` via reflection
- `HangfireExtensions.AddPlatformHangfire()` — registra dashboard, PostgreSQL storage, retry policy

---

### 81. Implementar fila de reprocessamento DLQ

**Módulo**: `MarcusPrado.Platform.Messaging`

**Implementar**:
- `DlqReprocessingJob` — `IJob` que lê mensagens da DLQ e tenta reprocessar
- `DlqInspectorEndpoints` — Minimal API endpoints: `GET /dlq/{topic}`, `POST /dlq/{topic}/reprocess/{messageId}`, `DELETE /dlq/{topic}/{messageId}`
- `IDlqMetrics` — contador de mensagens em DLQ por topic, métricas via OTel
- `DlqAlertRule` — emite alerta quando DLQ ultrapassa threshold configurável

---

## ⚙️ Configuração Avançada

### 82. Implementar hot reload de configuração

**Módulo**: `MarcusPrado.Platform.Runtime`

**Implementar**:
- `IOptionsHotReload<T>` — wrapper sobre `IOptionsMonitor<T>` com callback tipado `OnChange(T newValue)`
- `ConfigurationChangeLogger` — loga toda alteração de configuração em produção (auditoria)
- `ConfigurationValidator<T>` — valida nova configuração antes de aplicar via `DataAnnotations` + custom rules
- `HotReloadExtensions.AddPlatformOptionsHotReload<T>()` — helper de registro

---

### 83. Implementar provider de configuração encriptada

**Módulo**: `MarcusPrado.Platform.Runtime`

**Implementar**:
- `EncryptedJsonConfigurationProvider` — `IConfigurationProvider` que descriptografa valores marcados com `ENC(...)` usando `IDataEncryption`
- `EncryptedEnvironmentVariableProvider` — idem para variáveis de ambiente
- `ConfigCipherTool` — CLI tool (`dotnet platform-config encrypt`) para encriptar valores antes de commitar

---

### 84. Implementar Startup Verification

**Módulo**: `MarcusPrado.Platform.Runtime`

**Implementar**:
- `IStartupVerification` — interface: `VerifyAsync(CancellationToken)` retorna `StartupVerificationResult`
- `DatabaseConnectivityVerification` — verifica conexão com DB antes do app aceitar tráfego
- `RequiredSecretsVerification` — verifica que secrets obrigatórios estão presentes no `ISecretProvider`
- `StartupVerificationHostedService` — roda todas as verificações antes de `IHostApplicationLifetime.ApplicationStarted`
- Em falha: loga detalhes e chama `IHostApplicationLifetime.StopApplication()`

**Equivalência Java**: `commons-app-startup-verification` com Spring Boot `ApplicationRunner`

---

## 🧪 Testing Avançado

### 85. Implementar Test Data Builders (Bogus)

**Projeto**: `MarcusPrado.Platform.TestKit`

**Dependências NuGet**: `Bogus`

**Implementar**:
- `FakerExtensions` — `Faker<T>` pré-configurados para entidades da plataforma
- `EntityFaker<T>` — base class para builders de entidades de domínio
- `CommandFaker<T>` — builder para commands com dados válidos por padrão
- `TestDataScenarios` — cenários nomeados reutilizáveis: `ValidUser`, `PremiumTenant`, `ExpiredSubscription`

**Equivalência Java**: `commons-test-data-builders` com JavaFaker

---

### 86. Implementar Approval Testing (Verify)

**Projeto**: `MarcusPrado.Platform.TestKit`

**Dependências NuGet**: `Verify.Xunit`

**Implementar**:
- `PlatformVerifySettings` — configuração global: scrubbers para `DateTimeOffset`, `Guid`, `CorrelationId`
- `ApiResponseVerifier` — snapshot do response HTTP completo (status + headers + body)
- `DomainEventVerifier` — snapshot de domain events emitidos por um aggregate
- `SqlQueryVerifier` — snapshot das queries SQL geradas pelo EF Core

---

### 87. Implementar Performance Testing Kit (NBomber)

**Projeto**: `MarcusPrado.Platform.PerformanceTestKit` (novo projeto em `src/kits/`)

**Dependências NuGet**: `NBomber`, `NBomber.Http`

**Implementar**:
- `PlatformLoadTest` — base class para cenários de carga pré-configurados
- `CommandThroughputScenario` — mede throughput do pipeline CQRS sob carga
- `ApiEndpointScenario` — mede latência de endpoints com P50/P95/P99
- `MessagingThroughputScenario` — mede throughput de publish/consume Kafka/RabbitMQ
- `LoadTestReport` — HTML report gerado automaticamente via NBomber

**Equivalência Java**: `commons-test-performance` com Gatling

---

### 88. Implementar Mutation Testing (Stryker.NET)

**Projeto**: `src/tooling/MarcusPrado.Platform.MutationTests`

**Dependências**: `dotnet-stryker` (global tool)

**Implementar**:
- `stryker-config.json` — configuração por projeto: thresholds (break: 60, low: 70, high: 80)
- Script `run-mutation.sh` — roda Stryker em todos os projetos core
- Badge de mutation score no `README.md`
- CI step opcional (não bloqueia build principal)

---

### 89. Implementar Integration Test Environment

**Projeto**: `MarcusPrado.Platform.TestKit`

**Implementar**:
- `PlatformTestEnvironment` — configura todos os containers (Postgres, Redis, Kafka, RabbitMQ) em paralelo
- `TestNetworkBuilder` — cria rede Docker compartilhada entre containers do mesmo teste
- `SnapshotRestorer` — cria e restaura snapshot do banco de dados entre testes para isolamento
- `TestEnvironmentHealthCheck` — aguarda todos os containers estarem `healthy` antes de rodar testes

---

## 📦 NuGet e Release Management

### 90. Implementar automação de release

**Arquivo**: `.github/workflows/release.yml`

**Implementar**:
- `MinVer` ou `GitVersion` para versão semântica automática a partir de tags Git
- Geração automática de `CHANGELOG.md` via `git-cliff`
- GitHub Release com release notes
- Push para NuGet.org com `NUGET_API_KEY` secret
- GitHub Packages como feed espelho

**Arquivo**: `Directory.Build.props` (additions):
```xml
<PackageProjectUrl>https://github.com/marcusPrado02/csharp-commons</PackageProjectUrl>
<RepositoryUrl>https://github.com/marcusPrado02/csharp-commons</RepositoryUrl>
<PackageLicense>MIT</PackageLicense>
<PackageIcon>icon.png</PackageIcon>
```

---

### 91. Implementar geração de changelog de API

**Projeto**: `src/tooling/MarcusPrado.Platform.ApiChangelog`

**Implementar**:
- `ApiSurfaceExtractor` — extra assinaturas públicas de assemblies via Reflection
- `ApiDiffEngine` — compara dois snapshots: detecta breaking changes (remoção/renomeação), adições, deprecações
- `ChangelogRenderer` — gera `API-CHANGELOG.md` com seções `### Breaking Changes`, `### New`, `### Deprecated`
- Integração CI: roda em PRs e comenta no pull request com diff da API pública

**Equivalência Java**: `commons-tooling-api-changelog` com japicmp

---

## 🏥 Confiabilidade e Operações

### 92. Implementar Circuit Breaker Dashboard

**Projeto**: `MarcusPrado.Platform.Resilience`

**Implementar**:
- `CircuitBreakerRegistry` — registry central de todos os circuit breakers ativos
- `CircuitBreakerEndpoints` — Minimal API: `GET /circuit-breakers` (lista estado), `POST /circuit-breakers/{name}/reset`
- `CircuitBreakerMetrics` — OTel Metrics: `circuit_breaker.state` (gauge: 0=closed, 1=open, 2=half-open), `circuit_breaker.failures_total`

---

### 93. Implementar Structured Error Catalog

**Módulo**: `MarcusPrado.Platform.Domain` / `MarcusPrado.Platform.Contracts`

**Implementar**:
- `ErrorCatalog` — static class com todos os `Error` instances por domínio: `ErrorCatalog.Payment.NotFound`, `ErrorCatalog.Auth.TokenExpired`
- `IErrorTranslator` — traduz códigos de erro para mensagens localizadas via `IStringLocalizer`
- `ErrorDocumentationGenerator` — gera `docs/errors/error-catalog.md` com todos os erros e seus contextos
- Validação em ArchTests: todo `Error` deve estar registrado no `ErrorCatalog`

---

### 94. Implementar Graceful Degradation

**Módulo**: `MarcusPrado.Platform.Resilience`

**Implementar**:
- `IDegradationMode` — `None`, `PartiallyDegraded`, `ReadOnly`, `Maintenance`
- `DegradationController` — gerencia modo atual; persiste em Redis
- `DegradationMiddleware` — verifica modo antes de processar request; retorna 503 em manutenção
- `FeatureFlagDegradation` — degrada features individualmente via `IFeatureFlagProvider`
- `DegradationEndpoints` — `GET /degradation/status`, `POST /degradation/mode`

---

### 95. Implementar Distributed Lock

**Módulo**: `MarcusPrado.Platform.Redis`

**Implementar**:
- `IDistributedLock` — `AcquireAsync(key, ttl, retry)` retorna `IAsyncDisposable`
- `RedisDistributedLock` — Redlock algorithm com fencing token
- `PostgresAdvisoryLock` — `pg_try_advisory_xact_lock` para locks em transação DB
- `DistributedLockExtensions.WithLockAsync(key, action)` — helper fluente

**Equivalência Java**: `commons-adapters-distributed-lock`

---

### 96. Implementar Cache Stampede Prevention

**Módulo**: `MarcusPrado.Platform.Redis`

**Implementar**:
- `StampedeProtectedCache` — wraps `ICache` com `IDistributedLock` por chave
- `ProbabilisticEarlyExpiry` — algoritmo XFetch: recomputa cache com probabilidade crescente próximo do expiry
- `CacheWarmupService` — `IHostedService` que aquece cache de dados críticos na inicialização

---

### 97. Implementar Health Check avançado com degradação

**Módulo**: `MarcusPrado.Platform.HealthChecks`

**Implementar**:
- `DegradedHealthCheck` — retorna `Degraded` (não falha, mas sinaliza degradação parcial)
- `MemoryPressureHealthCheck` — monitora GC pressure; reporta `Degraded` acima de threshold
- `ThreadPoolStarvationHealthCheck` — detecta thread pool starvation via `ThreadPool.GetAvailableThreads`
- `ExternalDependencyHealthCheck` — testa dependências externas (terceiros) com timeout agressivo (1s)
- Health check custom endpoint `/health/extended` com JSON detalhado + histórico das últimas N verificações

---

## 🌍 Internacionalização e Localização

### 98. Implementar i18n e l10n

**Módulo**: `MarcusPrado.Platform.AspNetCore`

**Dependências NuGet**: `Microsoft.Extensions.Localization`

**Implementar**:
- `AcceptLanguageMiddleware` — extrai locale de `Accept-Language` header, popula `IRequestContext.Culture`
- `PlatformLocalizationExtensions.AddPlatformLocalization()` — configura `RequestLocalizationMiddleware` com fallback para `en-US`
- `IErrorTranslator` implementação `LocalizedErrorTranslator` — traduz erros por cultura
- `ValidationMessageLocalizer` — mensagens de validação traduzidas (en-US, pt-BR, es-ES)
- Recursos de localização em `src/core/MarcusPrado.Platform.Contracts/Resources/`

**Equivalência Java**: `commons-app-i18n` com Spring MessageSource

---

## 🔍 Developer Experience

### 99. Implementar Exception Enrichment e Debugging

**Módulo**: `MarcusPrado.Platform.AspNetCore`

**Implementar**:
- `DeveloperExceptionPageEnricher` — em Development, adiciona ao ProblemDetails: stack trace, inner exceptions, request body, query params, ambient context (tenantId, userId, correlationId)
- `ExceptionFingerprinter` — gera hash determinístico da exception para deduplicação em alertas
- `ExceptionGrouper` — agrupa exceptions similares via fingerprint no `IAppLogger`
- Integração opcional com Sentry: `SentryExtensions.AddPlatformSentry()`

---

### 100. Implementar Platform CLI (dotnet tool)

**Projeto**: `src/tooling/MarcusPrado.Platform.Cli`

**Dependências NuGet**: `System.CommandLine`

**Comandos**:
- `platform scaffold api <name>` — scaffolda Minimal API com todos os middlewares
- `platform scaffold worker <name>` — scaffolda Worker Service
- `platform scaffold domain <name>` — scaffolda aggregate + command + handler + validator
- `platform config encrypt <value>` — encripta valor de configuração
- `platform catalog errors` — lista todos os erros do `ErrorCatalog` em formato tabular
- `platform arch validate` — roda ArchTests e exibe resultado no terminal
- `platform dlq inspect <topic>` — lista mensagens na DLQ de um tópico
- `platform health <service-url>` — chama `/health/extended` e exibe resultado formatado

**Instalação**: `dotnet tool install -g MarcusPrado.Platform.Cli`

**Equivalência Java**: `commons-cli` com Picocli

---

## 🎯 Pendências Priorizadas

### Ordem de implementação sugerida

| # | Item | Módulo | Justificativa |
|---|------|--------|---------------|
| 1 | 2 | `Result<T>` + Erros | Blocker: usado por tudo |
| 2 | 3 | Primitivos de Domain | Blocker: Entity, ValueObject |
| 3 | 47 | `Directory.Packages.props` | Build: versões centralizadas |
| 4 | 28 | Pipeline CQRS | Core value: dispatcher + behaviors |
| 5 | 8 | ArchTests (NetArchTest) | Qualidade: enforce architecture |
| 6 | 9 | `AspNetCore` middlewares | Essencial para qualquer API |
| 7 | 57 | Minimal API Endpoint conventions | Dev experience: base class uniforme |
| 8 | 58 | API Versioning | Contratos versionados |
| 9 | 59 | OpenAPI / Scalar | Documentação pública |
| 10 | 12 | EF Core base context | Persistência básica |
| 11 | 13 | Postgres extension | Persistência básica |
| 12 | 20 | Redis (cache + quota) | Cache + Rate limit + Idempotency |
| 13 | 60 | Rate Limiting ASP.NET Core | Proteção por tenant/user |
| 14 | 14 | Kafka | Event-driven core |
| 15 | 15 | RabbitMQ | Mensageria alternativa |
| 16 | 16 | OpenTelemetry | Observabilidade em produção |
| 17 | 17 | Serilog | Logging estruturado |
| 18 | 65 | W3C TraceContext propagation | Distributed tracing end-to-end |
| 19 | 67 | Business Metrics | KPIs de negócio |
| 20 | 19 | Resilience / Polly | Produção-ready |
| 21 | 27 | OutboxInbox processor | Eventual consistency |
| 22 | 95 | Distributed Lock | Coordenação entre instâncias |
| 23 | 18 | HealthChecks | Kubernetes liveness/readiness |
| 24 | 97 | Health check avançado | Degradação parcial visível |
| 25 | 22 | TestKit completo | Habilita integration tests |
| 26 | 85 | Test Data Builders (Bogus) | Dados realistas em testes |
| 27 | 86 | Approval Testing (Verify) | Snapshot regression tests |
| 28 | 46 | Roslyn Analyzers | Enforcement automático |
| 29 | 4 | Static analysis config | Qualidade de código |
| 30 | 5 | CI/CD pipeline | Automação de build e deploy |
| 31 | 90 | Release automation | NuGet + changelog automático |
| 32 | 10 | Auth JWT + API Key | Segurança básica |
| 33 | 69 | OAuth2 / OIDC client | M2M auth |
| 34 | 70 | Criptografia em repouso | Dados sensíveis |
| 35 | 72 | PII Masking completo | LGPD/GDPR compliance |
| 36 | 74 | Input sanitization / XSS | Segurança de input |
| 37 | 43 | MultiTenancy avançado | Feature diferencial |
| 38 | 40 | FeatureFlags | Rollout gradual |
| 39 | 41 | Workflow Engine | Saga pattern |
| 40 | 77 | Saga Orchestration | Transações distribuídas |
| 41 | 75 | Event Sourcing | Audit trail completo |
| 42 | 76 | Projeções / Read Models | CQRS read side |
| 43 | 78 | Domain Event Router | Desacoplamento cross-boundary |
| 44 | 93 | Structured Error Catalog | Erros padronizados e documentados |
| 45 | 98 | i18n / l10n | Produto multilíngue |
| 46 | 94 | Graceful Degradation | Resiliência operacional |
| 47 | 30 | Governance (contratos ADR) | Evolução controlada de contratos |
| 48 | 79 | Quartz.NET adapter | Jobs robustos com clustering |
| 49 | 53 | Azure Service Bus | Mensageria cloud-native |
| 50 | 54 | AWS SQS/SNS | Mensageria AWS |
| 51 | 52 | NATS adapter | Mensageria de baixa latência |
| 52 | 49 | MongoDB adapter | Document database |
| 53 | 91 | API Changelog generator | Breaking changes detectados no CI |
| 54 | 99 | Exception Enrichment | Dev experience: debugging |
| 55 | 100 | Platform CLI | Developer experience |

---

## 📌 Notas

### Mapeamento Java → C#

| Java / Spring Boot | C# / .NET |
|--------------------|-----------|
| `pom.xml` | `*.csproj` + `Directory.Packages.props` |
| `commons-bom` | `Directory.Packages.props` (CPM) |
| Spring Boot AutoConfiguration | `IServiceCollection` extension methods |
| `@Configuration` | Static `Add*()` / `Use*()` extensions |
| `@Component` / `@Service` | Registrado no DI container |
| Java `record` (imutável) | C# `record` / `record struct` com `with` |
| Builder fluent | `with` expressions + constructors |
| Factory method estático | Static `Create()` method |
| `Result<T>` (custom) | `Result<T>` (custom ou OneOf) |
| `slf4j` / Logback | `ILogger<T>` / Serilog |
| JaCoCo | Coverlet + ReportGenerator |
| Spotless | CSharpier + `dotnet format` |
| ArchUnit | NetArchTest.Rules |
| Testcontainers Java | Testcontainers.Net |
| JMH benchmarks | BenchmarkDotNet |
| Maven Archetype | `dotnet new` templates |
| Spring Retry | Polly |
| Resilience4j | Polly v8 |
| Web3j | Nethereum |
| Stripe Java SDK | Stripe.net |
| gRPC Spring | Grpc.AspNetCore |
| GraphQL Spring | Hot Chocolate |
| Thymeleaf | Razor Pages / Fluid |
| iText / PDFBox | QuestPDF / iTextSharp |
| Apache POI | ClosedXML / EPPlus |
| Protobuf Java | protobuf-net |
| Consul Java client | Consul NuGet |
| Jaeger exporter | OpenTelemetry.Exporter.Jaeger |
| Prometheus JVM | prometheus-net / OTel Prometheus Exporter |
| AWS SDK Java | AWSSDK.* |
| Azure SDK Java | Azure.* |
| `@Scheduled` | `IHostedService` + `PeriodicTimer` |
| `@Transactional` | `[Transactional]` attribute + `TransactionBehavior` |

### Convenções de Nomenclatura

- Interfaces: `IXxx` (ex: `ICommandHandler<T, R>`)
- Records imutáveis: PascalCase sem sufixo (ex: `PaymentMethod`, `TenantQuota`)
- Enums: PascalCase (ex: `PaymentStatus.Succeeded`)
- Extension methods DI: `Add*()` para serviços, `Use*()` para middleware
- Namespaces: file-scoped (`namespace Foo.Bar;`)
- Códigos de erro: `"DOMAIN.ERROR_CODE"` (ex: `"PAYMENT.NOT_FOUND"`)
- Testes: `[Fact]` / `[Theory]` via xUnit; padrão AAA; `FluentAssertions`

### Regras de Arquitetura Enforceadas

```
Abstractions         ← sem dependências internas
Domain               ← Abstractions
Application          ← Domain, Abstractions
Contracts            ← Abstractions
Extensions/*         ← Core/* (nunca entre Extensions entre si)
Kits/*               ← qualquer coisa (apenas para testes)
Tooling/Analyzers    ← netstandard2.0 (sem referências internas de plataforma)
Tooling/ArchTests    ← qualquer coisa (valida as regras acima)
Samples/*            ← Extensions (apenas demonstração)
```
