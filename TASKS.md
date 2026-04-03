# TASKS — MarcusPrado Platform Commons

> Itens pendentes para que a biblioteca atinja plena funcionalidade como plataforma de engenharia nível staff para .NET.
>
> **Legenda**: ⚠️ abstração pronta, adapter concreto pendente · 🆕 não iniciado
>
> **Progresso atual**: 36 itens concluídos · 579 testes passando · 0 falhas (03/03/2026)

---

## 🔌 Adapters com Abstração Pronta (Quick Wins)

> Contratos `IXxx` já definidos — apenas conectar ao provider externo.

- [ ] **T-01** ⚠️ Concluir `MarcusPrado.Platform.Nethereum` — implementar `NethereumBlockchainClient`, `NethereumWalletManager`, `NethereumSmartContractClient` (Nethereum 4.x); cobrir com ≥ 10 testes unitários com mocks do provider
- [ ] **T-02** ⚠️ Concluir `MarcusPrado.Platform.Stripe` — implementar `StripePaymentService`, `StripeSubscriptionService`, `StripeRefundService`; mapear exceções Stripe → `Result<T>` failure; ≥ 10 testes com `StripeMockServer`
- [ ] **T-03** ⚠️ Concluir `MarcusPrado.Platform.MailKit` — completar `MailKitEmailSender` com suporte a anexos, templates HTML e retry SMTP; implementar `SimpleTemplateRenderer` com Scriban ou Fluid; ≥ 8 testes
- [ ] **T-04** ⚠️ Implementar `MarcusPrado.Platform.SendGrid` — adapter `SendGridEmailSender` implementando `IEmailSender` via `SendGrid` SDK; suporte a templates dinâmicos SendGrid; ≥ 8 testes
- [ ] **T-05** ⚠️ Concluir `MarcusPrado.Platform.Twilio` — implementar `TwilioSmsService` implementando `ISmsService`; suporte a status callbacks; ≥ 6 testes com Twilio test credentials
- [ ] **T-06** ⚠️ Implementar `MarcusPrado.Platform.AwsSns` (SMS) — adapter `SnsSmsService` implementando `ISmsService` via `AWSSDK.SimpleNotificationService`; ≥ 6 testes
- [ ] **T-07** ⚠️ Concluir `MarcusPrado.Platform.Elasticsearch` — completar `ElasticsearchSearchClient` implementando `ISearchClient` + `IIndexManager` via `Elastic.Clients.Elasticsearch` 8.x; suporte a paginação cursor; ≥ 10 testes com `ElasticsearchContainer`
- [ ] **T-08** ⚠️ Implementar `MarcusPrado.Platform.OpenSearch` — adapter `OpenSearchSearchClient` implementando `ISearchClient` via `OpenSearch.Client`; ≥ 8 testes com `OpenSearchContainer`
- [ ] **T-09** ⚠️ Concluir `MarcusPrado.Platform.HotChocolate` — implementar `PlatformTypeInterceptor` (injeta auth, tenant, correlation no schema), `PlatformErrorFilter` que converte `Result<T>` failures em erros GraphQL padronizados; ≥ 8 testes de integração
- [ ] **T-10** ⚠️ Concluir `MarcusPrado.Platform.Pdf` — implementar `QuestPdfGenerator` via QuestPDF com template-based generation; suporte a header/footer, paginação e marca d'água; ≥ 6 testes
- [ ] **T-11** ⚠️ Concluir `MarcusPrado.Platform.Excel` — completar `ClosedXmlExcelReader` / `ClosedXmlExcelWriter` com suporte a estilos, validação de células e streaming de arquivos grandes; ≥ 8 testes
- [ ] **T-12** ⚠️ Concluir `MarcusPrado.Platform.Consul` — completar `ConsulServiceDiscovery` implementando `IServiceDiscovery`; suporte a health check TTL, service deregistration e watch de mudanças; ≥ 8 testes com `ConsulContainer`
- [ ] **T-13** ⚠️ Concluir `MarcusPrado.Platform.MongoDb` — implementar `MongoDocumentRepository<T>`, `MongoSession` (transações multi-documento), `MongoPaginatedQuery<T>`, `MongoTenantFilter`, `MongoHealthProbe`, `MongoExtensions`; ≥ 10 testes com `MongoDbContainer`

---

## 🌐 Adaptadores HTTP e Web

- [ ] **T-14** 🆕 Implementar `MarcusPrado.Platform.Http` — `TypedHttpClient<TClient>` com propagação automática de correlation ID / tenant / auth token; resilience defaults (retry + circuit breaker) via `IHttpClientBuilder.AddResilienceHandler()`; logging estruturado de request/response; ≥ 10 testes
- [ ] **T-15** 🆕 Implementar `MarcusPrado.Platform.SignalR` — `PlatformHub<T>` com JWT auth, isolamento por tenant e propagação de correlação; `IRealtimePublisher`; `SignalRDomainEventSink` que converte `IDomainEvent` → broadcast; ≥ 6 testes de integração
- [ ] **T-16** 🆕 Implementar convenções de Minimal API — `IEndpoint`, `EndpointGroupBase`, `EndpointDiscovery.MapPlatformEndpoints()` via reflection, `ApiEnvelopeFilter`, `ValidationFilter<TRequest>`; ≥ 8 testes com `WebApplicationFactory`
- [ ] **T-17** 🆕 Implementar versionamento de API — `AddPlatformApiVersioning()` (URL + header + media-type), `DeprecationHeaderMiddleware` com headers `Deprecation` e `Sunset`, `ApiVersionDiscovery` endpoint; ≥ 6 testes
- [ ] **T-18** 🆕 Implementar OpenAPI / Scalar — `AddPlatformOpenApi()` com auth JWT + API Key, `PlatformOperationTransformer` para headers de contexto, `ProblemDetailsSchemaFilter` RFC 9457, `UseScalarApiReference()`; ≥ 6 testes de snapshot
- [ ] **T-19** 🆕 Implementar CORS avançado — `PlatformCorsPolicy` (DevPermissive / StagingRestricted / ProductionLocked), `TenantAwareCorsPolicy` por `ITenantContext`, `AddPlatformCors()`; ≥ 5 testes de preflight
- [ ] **T-20** 🆕 Implementar Rate Limiting ASP.NET Core — `AddPlatformRateLimiting()` com policies por tenant/user/IP via `RedisQuotaStore`, resposta 429 com `Retry-After` e `ProblemDetails`; ≥ 8 testes
- [ ] **T-21** 🆕 Implementar middleware de Security Headers — `SecurityHeadersMiddleware` (`X-Content-Type-Options`, `X-Frame-Options`, `Referrer-Policy`, `Content-Security-Policy`), `AddPlatformHsts()` em produção; ≥ 5 testes
- [ ] **T-22** 🆕 Implementar middleware de IP Filtering — `IpFilterMiddleware` com whitelist/blacklist CIDR, `IIpFilterStore` (in-memory + Redis), `TenantIpPolicy`, suporte a `X-Forwarded-For`; ≥ 6 testes
- [ ] **T-23** 🆕 Implementar Response Compression — `AddPlatformResponseCompression()` com Brotli (primário) + Gzip (fallback), threshold mínimo configurável; ≥ 4 testes
- [ ] **T-24** 🆕 Implementar Request Size Limiting — `TenantRequestSizePolicy` por tier (Free/Pro/Enterprise), resposta 413 com `ProblemDetails`; ≥ 4 testes

---

## 📨 Mensageria Adicional

- [ ] **T-25** 🆕 Implementar `MarcusPrado.Platform.Nats` — `NatsPublisher`, `NatsConsumer` com JetStream (at-least-once), `NatsHealthProbe`, `NatsExtensions`; ≥ 8 testes com `NatsContainer`
- [ ] **T-26** 🆕 Implementar `MarcusPrado.Platform.AzureServiceBus` — `ServiceBusPublisher`, `ServiceBusConsumer` com lock renewal automático, `ServiceBusDeadLetterSink`, suporte a `DefaultAzureCredential`; ≥ 8 testes
- [ ] **T-27** 🆕 Implementar `MarcusPrado.Platform.AwsSqs` / `AwsSns` — `SqsConsumer` com long polling e DLQ automático, `SqsPublisher`, `SnsPublisher`, fan-out SNS → SQS; ≥ 8 testes
- [ ] **T-28** 🆕 Implementar fila de reprocessamento DLQ — `DlqReprocessingJob`, endpoints Minimal API `GET /dlq/{topic}` / `POST /dlq/{topic}/reprocess/{id}` / `DELETE`, `IDlqMetrics` OTel, alerta por threshold; ≥ 8 testes

---

## 🗄️ Banco de Dados Adicional

- [ ] **T-29** 🆕 Implementar `MarcusPrado.Platform.MySql` — `MySqlConnectionFactory`, `MySqlHealthProbe`, `AddPlatformMySql()` via Pomelo EF Core provider; ≥ 6 testes com `MySqlContainer`
- [ ] **T-30** 🆕 Implementar Distributed Tracing para banco — `EfCoreTracingInterceptor` (`IDbCommandInterceptor` com SQL sanitizado), `DapperTracingWrapper` (`QueryWithTraceAsync` / `ExecuteWithTraceAsync`), atributos OTel semânticos; ≥ 6 testes

---

## 🔐 Segurança

- [ ] **T-31** 🆕 Implementar adapters de secrets — `ISecretProvider` via Azure Key Vault (`DefaultAzureCredential`), AWS Secrets Manager (com callback `OnRotation`) e HashiCorp Vault (`AppRole` + `Kubernetes`); cache local com TTL; ≥ 6 testes por adapter
- [ ] **T-32** 🆕 Implementar OAuth2 / OIDC Client Credentials — `OidcClientService`, `TokenCache` com renovação proativa, `MachineToMachineHttpHandler`, `AddPlatformOidcClient()`; ≥ 8 testes
- [ ] **T-33** 🆕 Implementar criptografia de dados em repouso — `IDataEncryption`, `AesGcmEncryption` (AES-256-GCM), `EncryptedAttribute`, `EncryptingValueConverter` EF Core, `KeyRotationService`; ≥ 8 testes
- [ ] **T-34** 🆕 Implementar Digital Signatures — `ISignatureService`, `RsaSignatureService` (RSA-PSS SHA-256), `EcdsaSignatureService` (P-256), `WebhookSignatureMiddleware` (HMAC-SHA256), `SignedPayloadEnvelope<T>` anti-replay; ≥ 8 testes
- [ ] **T-35** 🆕 Implementar PII Masking completo — `[PiiData]` attribute, `PiiClassifier`, `PiiRedactor` (email/CPF/phone masking), `SerilogPiiDestructuringPolicy`, `GdprComplianceReport`; ≥ 8 testes
- [ ] **T-36** 🆕 Implementar mTLS support — `MtlsAuthenticationHandler`, `CertificateTenantResolver` (extrai TenantId do SAN), `CertificateRevocationChecker` (CRL/OCSP), `AddPlatformMtls()`; ≥ 6 testes
- [ ] **T-37** 🆕 Implementar sanitização de input / prevenção XSS — `IInputSanitizer`, `HtmlSanitizerAdapter` (via HtmlSanitizer), `SanitizingModelBinder` com `[SanitizeInput]`, `SqlInjectionDetector`; ≥ 8 testes

---

## 🔄 Event Sourcing e CQRS Avançado

- [ ] **T-38** 🆕 Implementar Event Sourcing — `IEventStore`, `EventStoreDbAdapter` (EventStoreDB ou Marten/PostgreSQL), `EventSourcedRepository<T>`, `EventSnapshot<T>` a cada N eventos, `AggregateEventReplayer`; ≥ 10 testes
- [ ] **T-39** 🆕 Implementar Projeções / Read Models — `IProjection<TEvent, TReadModel>`, `ProjectionEngine`, `IReadModelStore<T>`, `EfReadModelStore<T>`, `RedisReadModelStore<T>`, `ProjectionRebuildJob`; ≥ 10 testes
- [ ] **T-40** 🆕 Implementar Saga Orchestration — `ISaga<TState>`, `SagaOrchestrator` (executa steps com compensação), `ISagaStore`, `EfSagaStore`, `SagaCompensationHandler`, `SagaStep<TCommand>` com timeout; ≥ 10 testes
- [ ] **T-41** 🆕 Implementar Domain Event Router — `DomainEventRouter`, `IDomainEventHandler<TEvent>`, `DomainEventDispatcher` (pós `SaveChanges`), `CrossBoundaryEventBridge` (converte `IDomainEvent` → `IEventContract`), `EventHandlerPipeline`; ≥ 8 testes

---

## ⏰ Background Jobs Avançados

- [ ] **T-42** 🆕 Implementar adapter Quartz.NET — `QuartzJobScheduler` implementando `IJobScheduler`, `PlatformJobFactory` via DI, `QuartzClusterStore` (AdoJobStore + PostgreSQL), `JobTriggerBuilder` fluent, `AddPlatformQuartz()` com clustering; ≥ 8 testes
- [ ] **T-43** 🆕 Implementar adapter Hangfire — `HangfireJobScheduler` implementando `IJobScheduler`, `HangfireRecurringJobRegistrar` via `[RecurringJob(cron)]` reflection, dashboard + PostgreSQL storage; ≥ 6 testes

---

## 📊 Observabilidade Avançada

- [ ] **T-44** 🆕 Implementar Business Metrics — `OtelBusinessMetrics` implementando `IBusinessMetrics` (`RecordOrderPlaced`, `RecordPaymentProcessed`, `RecordUserSignup`, `RecordEventConsumed`), `AddPlatformBusinessMetrics()`, template Grafana JSON; ≥ 8 testes com `InMemoryMetricCollector`
- [ ] **T-45** 🆕 Implementar SLO / Error Budget tracking — `ServiceLevelObjective` record, `ErrorBudgetCalculator`, `SloMetricsCollector` (expõe `slo.availability` e `slo.error_budget_remaining` via OTel), alertas baseados em burn rate; ≥ 6 testes
- [ ] **T-46** 🆕 Implementar Circuit Breaker Dashboard — `CircuitBreakerRegistry` central, endpoints Minimal API `GET /circuit-breakers` / `POST /circuit-breakers/{name}/reset`, métricas OTel `circuit_breaker.state` e `circuit_breaker.failures_total`; ≥ 6 testes

---

## 🔧 Confiabilidade e Operações

- [ ] **T-47** 🆕 Implementar Distributed Lock — `IDistributedLock`, `RedisDistributedLock` (Redlock algorithm com fencing token), `PostgresAdvisoryLock` (`pg_try_advisory_xact_lock`), `WithLockAsync(key, action)` fluente; ≥ 8 testes
- [ ] **T-48** 🆕 Implementar Cache Stampede Prevention — `StampedeProtectedCache` (wrap de `ICache` + `IDistributedLock`), `ProbabilisticEarlyExpiry` (algoritmo XFetch), `CacheWarmupService` (`IHostedService`); ≥ 6 testes
- [ ] **T-49** 🆕 Implementar Graceful Degradation — `IDegradationMode` (None / PartiallyDegraded / ReadOnly / Maintenance), `DegradationController` (persiste em Redis), `DegradationMiddleware`, `FeatureFlagDegradation`, endpoints `GET /degradation/status` e `POST /degradation/mode`; ≥ 8 testes
- [ ] **T-50** 🆕 Implementar Health Check avançado — `DegradedHealthCheck`, `MemoryPressureHealthCheck` (GC pressure), `ThreadPoolStarvationHealthCheck`, `ExternalDependencyHealthCheck` (timeout 1s), endpoint `/health/extended` com histórico das últimas N verificações; ≥ 8 testes
- [ ] **T-51** 🆕 Implementar Startup Verification — `IStartupVerification`, `DatabaseConnectivityVerification`, `RequiredSecretsVerification`, `StartupVerificationHostedService` (roda antes de `ApplicationStarted`; falha → `StopApplication()`); ≥ 6 testes

---

## ⚙️ Configuração Avançada

- [ ] **T-52** 🆕 Implementar hot reload de configuração — `IOptionsHotReload<T>` (wrap de `IOptionsMonitor<T>`), `ConfigurationChangeLogger` (auditoria de mudanças em produção), `ConfigurationValidator<T>` (valida antes de aplicar), `AddPlatformOptionsHotReload<T>()`; ≥ 6 testes
- [ ] **T-53** 🆕 Implementar provider de configuração encriptada — `EncryptedJsonConfigurationProvider` (descriptografa valores `ENC(...)`), `EncryptedEnvironmentVariableProvider`, `ConfigCipherTool` (CLI `dotnet platform-config encrypt`); ≥ 6 testes

---

## 🌍 Internacionalização

- [ ] **T-54** 🆕 Implementar i18n / l10n — `AcceptLanguageMiddleware` (extrai locale → `IRequestContext.Culture`), `AddPlatformLocalization()` com fallback `en-US`, `LocalizedErrorTranslator`, `ValidationMessageLocalizer` (en-US, pt-BR, es-ES), recursos em `src/core/MarcusPrado.Platform.Contracts/Resources/`; ≥ 8 testes

---

## 🧪 Testing Avançado

- [ ] **T-55** 🆕 Implementar `MarcusPrado.Platform.ContractTestKit` — `PactVerifier` (CDC HTTP via Pact Broker + `WebApplicationFactory`), `PactPublisher` (com metadados Git), `AsyncContractVerifier` (contratos de mensagens Kafka/RabbitMQ via `EventContractEnvelope`); ≥ 8 testes
- [ ] **T-56** 🆕 Implementar `MarcusPrado.Platform.ChaosKit` — `LatencyFault`, `ErrorFault`, `PacketLossFault`, `ChaosRunner.RunWithChaos(config, action)` por taxa de injeção 0–1.0, integração com `ResilienceContext`; ≥ 8 testes
- [ ] **T-57** 🆕 Implementar Approval Testing — `PlatformVerifySettings` (scrubbers para `DateTimeOffset`, `Guid`, `CorrelationId`), `ApiResponseVerifier` (snapshot HTTP completo), `DomainEventVerifier`, `SqlQueryVerifier` (EF Core queries); ≥ 8 testes de snapshot
- [ ] **T-58** 🆕 Implementar `MarcusPrado.Platform.PerformanceTestKit` — `PlatformLoadTest`, `CommandThroughputScenario`, `ApiEndpointScenario` (P50/P95/P99), `MessagingThroughputScenario` (Kafka/RabbitMQ), `LoadTestReport` HTML via NBomber; ≥ 4 cenários funcionais
- [ ] **T-59** 🆕 Implementar Mutation Testing — `stryker-config.json` por projeto (thresholds break:60/low:70/high:80), script `run-mutation.sh`, badge de mutation score no `README.md`, CI step não-bloqueante; validar score ≥ 70% em todos os projetos core
- [ ] **T-60** 🆕 Implementar Integration Test Environment — `PlatformTestEnvironment` (todos os containers em paralelo), `TestNetworkBuilder` (rede Docker compartilhada), `SnapshotRestorer` (isolamento de banco por teste), `TestEnvironmentHealthCheck` (aguarda containers `healthy`); ≥ 6 testes de bootstrap

---

## 🔧 Tooling e Developer Experience

- [ ] **T-61** 🆕 Completar Roslyn Analyzers — `PLATFORM001` (NoEfCoreInDomain), `PLATFORM002` (NoAspNetInDomain), `PLATFORM003` (DomainNoInfraReference), `PLATFORM004` (EnforceResultType), `PLATFORM005` (EnforceIdempotencyKey), `AddResultWrapperCodeFix`; ≥ 2 testes de analyzer por regra (positivo + negativo)
- [ ] **T-62** 🆕 Implementar templates `dotnet new` — `platform-api`, `platform-worker`, `platform-domain`, `platform-command`; publicar como `MarcusPrado.Platform.Templates` (`dotnet new install`)
- [ ] **T-63** 🆕 Implementar geração de API Changelog — `ApiSurfaceExtractor` (assinaturas públicas via Reflection), `ApiDiffEngine` (breaking changes / adições / deprecações), `ChangelogRenderer` (`API-CHANGELOG.md`), integração CI com comentário automático em PRs; ≥ 6 testes
- [ ] **T-64** 🆕 Implementar Platform CLI (`dotnet tool`) — comandos: `scaffold api/worker/domain/command`, `config encrypt`, `catalog errors`, `arch validate`, `dlq inspect`, `health <url>`; publicar como `MarcusPrado.Platform.Cli` (`dotnet tool install -g`); ≥ 6 testes de integração do CLI
- [ ] **T-65** 🆕 Implementar Exception Enrichment — `DeveloperExceptionPageEnricher` (stack trace + contexto em Development), `ExceptionFingerprinter` (hash determinístico), `ExceptionGrouper` (agrupamento por fingerprint), integração opcional com Sentry; ≥ 6 testes

---

## 📦 Release e Qualidade

- [ ] **T-66** 🆕 Implementar automação de release — `MinVer` ou `GitVersion` para versão semântica automática, geração de `CHANGELOG.md` via `git-cliff`, GitHub Release com release notes, push automático para NuGet.org e GitHub Packages no CI (`.github/workflows/release.yml`)
- [ ] **T-67** 🆕 Implementar Structured Error Catalog — `ErrorCatalog` (static class com todos os `Error` por domínio: `ErrorCatalog.Payment.NotFound` etc.), `IErrorTranslator` com `IStringLocalizer`, `ErrorDocumentationGenerator` (`docs/errors/error-catalog.md`), regra ArchTest de completude; ≥ 8 testes

---

## 📋 Testes Faltantes em Módulos Concluídos

> Módulos marcados como ✅ no BACKLOG mas sem cobertura de teste suficiente nos projetos `tests/unit/`.

- [ ] **T-68** 🆕 Adicionar testes de integração para `MarcusPrado.Platform.Kafka` — publish→consume round-trip real, DLQ após N retries, graceful shutdown, propagação de headers; usar `KafkaContainer` via TestKit; ≥ 12 casos
- [ ] **T-69** 🆕 Adicionar testes de integração para `MarcusPrado.Platform.RabbitMq` — publish/consume básico, DLQ após falha, reconexão automática, publisher confirms; usar `RabbitMqContainer`; ≥ 10 casos
- [ ] **T-70** 🆕 Adicionar testes de integração para `MarcusPrado.Platform.EfCore` — audit automático, domain events pós-SaveChanges, tenant filter, transação rollback, outbox em mesma transação; usar `PostgresContainer`; ≥ 15 casos
- [ ] **T-71** 🆕 Adicionar testes de integração para `MarcusPrado.Platform.Redis` — Get/Set/Remove, TTL expiration, quota atomic increment, idempotency deduplication, stampede lock; usar `RedisContainer`; ≥ 12 casos
- [ ] **T-72** 🆕 Adicionar testes de integração para `MarcusPrado.Platform.OutboxInbox` — OutboxProcessor publica mensagem pendente, InboxProcessor descarta duplicata, distributed lock previne duplo processamento; usar `PostgresContainer` + `RedisContainer`; ≥ 10 casos
- [ ] **T-73** 🆕 Adicionar testes para `MarcusPrado.Platform.FeatureFlags` — `InMemoryProvider`, `EnvironmentProvider`, rollout por percentage/tenant/user/canary; ≥ 10 casos

---

*Gerado em 2026-04-03 com base na análise do estado atual do repositório e do `BACKLOG.md`.*
*Total: **73 tarefas** — 13 adapters com abstração pronta + 60 implementações novas.*
