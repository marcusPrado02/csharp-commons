#!/usr/bin/env python3
"""Injects <Description> and <PackageTags> into each publishable .csproj."""

import xml.etree.ElementTree as ET
import os

ET.register_namespace("", "")

METADATA: dict[str, tuple[str, str]] = {
    # ── Core ──────────────────────────────────────────────────────────────────
    "MarcusPrado.Platform.Abstractions": (
        "Core contracts: Result<T>, typed errors, CQRS bus interfaces (ICommandBus, IQueryBus, IDispatcher), "
        "context propagation (ITenantContext, ICorrelationContext, IUserContext). Zero infrastructure dependencies.",
        "dotnet;platform;result;cqrs;errors;abstractions;microservices",
    ),
    "MarcusPrado.Platform.Abstractions.Blockchain": (
        "Blockchain abstractions: IBlockchainClient, IWalletManager, ISmartContractClient.",
        "dotnet;platform;blockchain;abstractions",
    ),
    "MarcusPrado.Platform.Abstractions.Documents": (
        "Document generation abstractions: IPdfGenerator, IExcelReader, IExcelWriter.",
        "dotnet;platform;pdf;excel;documents;abstractions",
    ),
    "MarcusPrado.Platform.Abstractions.Email": (
        "Email sending abstractions: IEmailSender, ITemplateRenderer with HTML and attachment support.",
        "dotnet;platform;email;abstractions",
    ),
    "MarcusPrado.Platform.Abstractions.GraphQL": (
        "GraphQL abstractions: IGraphQLClient and schema convention contracts.",
        "dotnet;platform;graphql;abstractions",
    ),
    "MarcusPrado.Platform.Abstractions.Payment": (
        "Payment processing abstractions: IPaymentService, ISubscriptionService, IRefundService.",
        "dotnet;platform;payment;stripe;abstractions",
    ),
    "MarcusPrado.Platform.Abstractions.Search": (
        "Search abstractions: ISearchClient and IIndexManager with cursor-based pagination support.",
        "dotnet;platform;search;elasticsearch;abstractions",
    ),
    "MarcusPrado.Platform.Abstractions.ServiceDiscovery": (
        "Service discovery abstractions: IServiceDiscovery, IHealthCheckRegistrar.",
        "dotnet;platform;service-discovery;consul;abstractions",
    ),
    "MarcusPrado.Platform.Abstractions.Sms": (
        "SMS abstractions: ISmsService with delivery status callback support.",
        "dotnet;platform;sms;abstractions",
    ),
    "MarcusPrado.Platform.Abstractions.Storage": (
        "Object storage abstractions: IObjectStorage, IFileUploader.",
        "dotnet;platform;storage;abstractions",
    ),
    "MarcusPrado.Platform.Application": (
        "CQRS pipeline with ICommandBus, IQueryBus, and eight built-in behaviors: "
        "validation, tracing, metrics, logging, authorization, idempotency, transaction, retry.",
        "dotnet;platform;cqrs;pipeline;commands;queries;behaviors;microservices",
    ),
    "MarcusPrado.Platform.AuditLog": (
        "Structured audit log contracts and in-memory store for tracking entity state changes.",
        "dotnet;platform;audit;audit-log",
    ),
    "MarcusPrado.Platform.BackgroundJobs": (
        "Background job abstractions: IJob, IJobHandler, IJobScheduler for Quartz.NET and Hangfire adapters.",
        "dotnet;platform;background-jobs;scheduler;abstractions",
    ),
    "MarcusPrado.Platform.BackupRestore": (
        "Backup and restore abstractions for database and object storage snapshots.",
        "dotnet;platform;backup;restore;abstractions",
    ),
    "MarcusPrado.Platform.Contracts": (
        "API envelope types (ApiEnvelope<T>), ProblemDetails model (RFC 9457), "
        "event contract interfaces, versioning policies, and compatibility rules.",
        "dotnet;platform;contracts;api;problem-details;versioning",
    ),
    "MarcusPrado.Platform.Domain": (
        "Domain-model primitives: Entity<TId>, AggregateRoot<TId> with optimistic concurrency, "
        "ValueObject, DomainEvent, Specification<T> with composition, IBusinessRule.",
        "dotnet;platform;domain;ddd;aggregate;entity;value-object;specification",
    ),
    "MarcusPrado.Platform.ErrorCatalog": (
        "Typed error constants catalog with IErrorTranslator and ErrorDocumentationGenerator "
        "for generating docs/errors/error-catalog.md.",
        "dotnet;platform;errors;error-catalog",
    ),
    "MarcusPrado.Platform.FeatureFlags": (
        "Feature flag evaluation: IFeatureFlagProvider, FeatureFlagContext, "
        "rollout by tenant, user, percentage, and canary strategies.",
        "dotnet;platform;feature-flags;feature-toggles;rollout",
    ),
    "MarcusPrado.Platform.Governance": (
        "Platform governance: contract registry, API compatibility checker, "
        "deprecation schedules, ADR store, platform standards enforcement.",
        "dotnet;platform;governance;contracts;compatibility",
    ),
    "MarcusPrado.Platform.Messaging": (
        "Broker-agnostic messaging contracts: IMessagePublisher, IMessageConsumer, "
        "MessageEnvelope<T>, Outbox/Inbox abstractions, IDeadLetterSink, DLQReprocessor.",
        "dotnet;platform;messaging;events;outbox;inbox;dead-letter;microservices",
    ),
    "MarcusPrado.Platform.MultiTenancy": (
        "Multi-tenancy primitives: ITenantResolver, ITenantIsolationStrategy "
        "(schema-per-tenant, database-per-tenant, discriminator), TenantQuota, QuotaExceededException.",
        "dotnet;platform;multi-tenancy;saas;tenant",
    ),
    "MarcusPrado.Platform.Observability": (
        "Observability abstractions: IBusinessMetrics, ITracing, IHealthProbe, "
        "ServiceLevelObjective, ErrorBudget, CorrelationContext, LogSanitizer.",
        "dotnet;platform;observability;opentelemetry;metrics;tracing;slo",
    ),
    "MarcusPrado.Platform.OutboxInbox": (
        "Transactional Outbox/Inbox pattern for at-least-once messaging guarantees. "
        "OutboxProcessor and InboxProcessor hosted services. In-memory and EF Core store implementations.",
        "dotnet;platform;outbox;inbox;messaging;at-least-once;transactional",
    ),
    "MarcusPrado.Platform.Persistence": (
        "Persistence abstractions: IRepository, IReadRepository, IWriteRepository, "
        "IUnitOfWork with transaction support, IAuditWriter, IConcurrencyToken.",
        "dotnet;platform;persistence;repository;unit-of-work;ddd",
    ),
    "MarcusPrado.Platform.RateLimiting": (
        "Rate limiting abstractions: IRateLimitPolicy, IQuotaStore with "
        "fixed-window, sliding-window, and token-bucket policy implementations.",
        "dotnet;platform;rate-limiting;quota",
    ),
    "MarcusPrado.Platform.Resilience": (
        "Resilience primitives: RetryPolicy, CircuitBreakerPolicy, TimeoutPolicy, "
        "BulkheadPolicy, HedgingPolicy, ResilientExecutor with exponential backoff and jitter.",
        "dotnet;platform;resilience;circuit-breaker;retry;polly",
    ),
    "MarcusPrado.Platform.Runtime": (
        "Runtime abstractions: IAppConfiguration, IHostedLifecycle hooks, "
        "DeploymentEnvironment, Region, InstanceInfo, GracefulShutdown.",
        "dotnet;platform;runtime;configuration;lifecycle",
    ),
    "MarcusPrado.Platform.Security": (
        "Security abstractions: ITokenValidator, ITokenIntrospector, IPolicyAuthorizer, "
        "ISecretProvider, ISecurityAuditSink, PiiClassifier, RedactionRule.",
        "dotnet;platform;security;auth;pii;secrets;abstractions",
    ),
    "MarcusPrado.Platform.Workflow": (
        "Workflow abstractions: step execution, state management, compensation, and workflow store.",
        "dotnet;platform;workflow;saga;abstractions",
    ),
    # ── Extensions ────────────────────────────────────────────────────────────
    "MarcusPrado.Platform.AspNetCore": (
        "ASP.NET Core integration: correlation/tenant/exception middleware, Minimal API conventions "
        "(IEndpoint, EndpointGroupBase, EndpointDiscovery), CQRS registration, security headers, "
        "CORS, rate limiting, IP filtering, localization, request/response sanitization.",
        "dotnet;platform;aspnetcore;middleware;minimal-api;cqrs;microservices",
    ),
    "MarcusPrado.Platform.AspNetCore.Auth": (
        "JWT and API Key authentication handlers for ASP.NET Core with platform context propagation.",
        "dotnet;platform;aspnetcore;jwt;api-key;authentication",
    ),
    "MarcusPrado.Platform.AspNetCore.ProblemDetails": (
        "RFC 9457 ProblemDetails factory and exception-to-ProblemDetails mapper for ASP.NET Core.",
        "dotnet;platform;aspnetcore;problem-details;rfc9457;errors",
    ),
    "MarcusPrado.Platform.AwsSqs": (
        "AWS SQS/SNS messaging adapter: SqsPublisher, SqsConsumer with long-polling and automatic DLQ, "
        "SnsPublisher for fan-out. Implements IMessagePublisher / IMessageConsumer.",
        "dotnet;platform;aws;sqs;sns;messaging;microservices",
    ),
    "MarcusPrado.Platform.AzureServiceBus": (
        "Azure Service Bus messaging adapter: ServiceBusPublisher, ServiceBusConsumer with automatic "
        "lock renewal, ServiceBusDeadLetterSink. Supports DefaultAzureCredential.",
        "dotnet;platform;azure;service-bus;messaging;microservices",
    ),
    "MarcusPrado.Platform.Configuration": (
        "Configuration extensions: IOptionsHotReload<T> with change auditing, "
        "EncryptedJsonConfigurationProvider that decrypts ENC(...) values, ConfigurationValidator<T>.",
        "dotnet;platform;configuration;hot-reload;encrypted-config",
    ),
    "MarcusPrado.Platform.Consul": (
        "HashiCorp Consul adapter for IServiceDiscovery with health check TTL, "
        "service deregistration, and watch-based change notification.",
        "dotnet;platform;consul;service-discovery",
    ),
    "MarcusPrado.Platform.DataAccess": (
        "Distributed tracing for data access: EfCoreTracingInterceptor (IDbCommandInterceptor with "
        "sanitized SQL) and DapperTracingWrapper (QueryWithTraceAsync / ExecuteWithTraceAsync) "
        "with OpenTelemetry semantic conventions.",
        "dotnet;platform;data-access;tracing;opentelemetry;efcore;dapper",
    ),
    "MarcusPrado.Platform.Degradation": (
        "Graceful degradation: four named operating modes (None / PartiallyDegraded / ReadOnly / Maintenance), "
        "DegradationMiddleware, IDegradationController, management endpoints, OTel metrics.",
        "dotnet;platform;degradation;resilience;maintenance;aspnetcore",
    ),
    "MarcusPrado.Platform.DistributedLock": (
        "Distributed locking: RedisDistributedLock (Redlock algorithm with fencing token) and "
        "PostgresAdvisoryLock (pg_try_advisory_xact_lock). Implements IDistributedLock.",
        "dotnet;platform;distributed-lock;redis;redlock;postgres",
    ),
    "MarcusPrado.Platform.DlqReprocessing": (
        "Dead-letter queue management: Minimal API endpoints (list / reprocess / discard), "
        "IDlqStore, DlqReprocessingJob, OTel depth/reprocessed/discarded metrics.",
        "dotnet;platform;dlq;dead-letter;messaging;reprocessing",
    ),
    "MarcusPrado.Platform.EfCore": (
        "EF Core integration: AppDbContextBase with automatic audit filling, domain event dispatch "
        "after SaveChanges, multi-tenant query filters, outbox/inbox tables, EfUnitOfWork with "
        "savepoint support, EfMigrationRunner.",
        "dotnet;platform;efcore;entity-framework;audit;outbox;multi-tenancy",
    ),
    "MarcusPrado.Platform.Elasticsearch": (
        "Elasticsearch adapter for ISearchClient and IIndexManager "
        "via Elastic.Clients.Elasticsearch 8.x with cursor-based pagination.",
        "dotnet;platform;elasticsearch;search",
    ),
    "MarcusPrado.Platform.EventRouting": (
        "Domain event routing: DomainEventRouter, IDomainEventHandler<TEvent>, "
        "DomainEventDispatcher, CrossBoundaryEventBridge, EventHandlerPipeline.",
        "dotnet;platform;events;domain-events;routing;cqrs",
    ),
    "MarcusPrado.Platform.EventSourcing": (
        "Event sourcing infrastructure: IEventStore, EventSourcedRepository<T> with snapshots "
        "every N events, AggregateEventReplayer, optimistic concurrency. "
        "Includes saga orchestration (ISaga, SagaOrchestrator) and projection engine.",
        "dotnet;platform;event-sourcing;cqrs;saga;projections;ddd",
    ),
    "MarcusPrado.Platform.Excel": (
        "Excel adapter: ClosedXmlExcelReader and ClosedXmlExcelWriter with "
        "styles, cell validation, and streaming support for large files.",
        "dotnet;platform;excel;closedxml;spreadsheet",
    ),
    "MarcusPrado.Platform.ExceptionEnrichment": (
        "Exception enrichment for ASP.NET Core: ExceptionFingerprinter (deterministic hash), "
        "ExceptionGrouper, DeveloperExceptionPageEnricher with full context in Development.",
        "dotnet;platform;exceptions;enrichment;aspnetcore",
    ),
    "MarcusPrado.Platform.Grpc": (
        "gRPC integration helpers and interceptors for ASP.NET Core services.",
        "dotnet;platform;grpc;aspnetcore",
    ),
    "MarcusPrado.Platform.Hangfire": (
        "Hangfire adapter for IJobScheduler with PostgreSQL storage and "
        "recurring job registration via [RecurringJob] reflection.",
        "dotnet;platform;hangfire;background-jobs;scheduler",
    ),
    "MarcusPrado.Platform.HealthChecks": (
        "Advanced health checks: liveness (/health/live) and readiness (/health/ready) endpoints, "
        "MemoryPressureHealthCheck, ThreadPoolStarvationHealthCheck, ExternalDependencyHealthCheck, "
        "health history, startup verification, DegradedHealthCheck.",
        "dotnet;platform;health-checks;liveness;readiness;aspnetcore",
    ),
    "MarcusPrado.Platform.HotChocolate": (
        "HotChocolate GraphQL integration: PlatformTypeInterceptor (injects auth, tenant, correlation) "
        "and PlatformErrorFilter (converts Result<T> failures to typed GraphQL errors).",
        "dotnet;platform;graphql;hotchocolate",
    ),
    "MarcusPrado.Platform.Http": (
        "HTTP client extensions: TypedHttpClient base class with automatic "
        "correlation/tenant/auth header propagation and AddStandardResilienceHandler() defaults "
        "(retry + circuit breaker + timeout) via Microsoft.Extensions.Http.Resilience.",
        "dotnet;platform;http;httpclient;resilience;correlation;microservices",
    ),
    "MarcusPrado.Platform.Kafka": (
        "Apache Kafka messaging adapter: KafkaProducer implementing IMessagePublisher, "
        "KafkaConsumer<T> BackgroundService base, OTel W3C TraceContext propagation.",
        "dotnet;platform;kafka;messaging;microservices",
    ),
    "MarcusPrado.Platform.MailKit": (
        "Email adapter for IEmailSender via MailKit with HTML templates (Scriban/Fluid), "
        "file attachments, and SMTP retry with exponential backoff.",
        "dotnet;platform;email;mailkit;smtp",
    ),
    "MarcusPrado.Platform.MongoDb": (
        "MongoDB adapter: MongoDocumentRepository<T>, MongoSession for multi-document transactions, "
        "MongoPaginatedQuery<T>, multi-tenant filter, MongoHealthProbe.",
        "dotnet;platform;mongodb;nosql;repository",
    ),
    "MarcusPrado.Platform.MySql": (
        "MySQL adapter: MySqlConnectionFactory, MySqlHealthProbe, "
        "AddPlatformMySql() via Pomelo EF Core provider.",
        "dotnet;platform;mysql;pomelo;efcore",
    ),
    "MarcusPrado.Platform.Nats": (
        "NATS JetStream messaging adapter: NatsPublisher implementing IMessagePublisher "
        "and NatsConsumer<T> BackgroundService base with at-least-once delivery.",
        "dotnet;platform;nats;jetstream;messaging;microservices",
    ),
    "MarcusPrado.Platform.Nethereum": (
        "Blockchain adapter: NethereumBlockchainClient, NethereumWalletManager, "
        "NethereumSmartContractClient via Nethereum 4.x.",
        "dotnet;platform;blockchain;nethereum;ethereum;web3",
    ),
    "MarcusPrado.Platform.Observability": (
        "OpenTelemetry + Serilog one-call setup: AddPlatformTelemetry(), OtelBusinessMetrics, "
        "SLO/error budget OTel gauges, circuit breaker registry with management endpoints, "
        "correlation enrichment, log sanitization.",
        "dotnet;platform;observability;opentelemetry;serilog;metrics;slo;circuit-breaker",
    ),
    "MarcusPrado.Platform.OpenTelemetry": (
        "OpenTelemetry SDK configuration with traces, metrics, and logs. "
        "OTLP exporter, platform activity sources, OtelHealthCheckPublisher.",
        "dotnet;platform;opentelemetry;otel;tracing;metrics;logs",
    ),
    "MarcusPrado.Platform.Pdf": (
        "PDF generation adapter: QuestPdfGenerator via QuestPDF with "
        "header/footer, pagination, and watermark support.",
        "dotnet;platform;pdf;questpdf",
    ),
    "MarcusPrado.Platform.Postgres": (
        "PostgreSQL adapter: PostgresConnectionFactory, PostgresHealthProbe, AddPlatformPostgres().",
        "dotnet;platform;postgres;postgresql;npgsql",
    ),
    "MarcusPrado.Platform.Protobuf": (
        "Protobuf serialization helpers for platform messaging contracts and gRPC services.",
        "dotnet;platform;protobuf;grpc;serialization",
    ),
    "MarcusPrado.Platform.Quartz": (
        "Quartz.NET adapter for IJobScheduler with clustered AdoJobStore (PostgreSQL), "
        "PlatformJobFactory via DI, and fluent trigger builder.",
        "dotnet;platform;quartz;background-jobs;scheduler;clustering",
    ),
    "MarcusPrado.Platform.RabbitMq": (
        "RabbitMQ messaging adapter: RabbitProducer implementing IMessagePublisher, "
        "RabbitConsumer<T> BackgroundService base with publisher confirms and auto-reconnect.",
        "dotnet;platform;rabbitmq;messaging;microservices",
    ),
    "MarcusPrado.Platform.Redis": (
        "Redis adapter: ICache implementation, RedisDistributedLock (Redlock), "
        "IQuotaStore for rate limiting, IIdempotencyStore, StampedeProtectedCache (XFetch algorithm).",
        "dotnet;platform;redis;cache;distributed-lock;rate-limiting;idempotency",
    ),
    "MarcusPrado.Platform.Secrets": (
        "Secrets management adapters: Azure Key Vault, AWS Secrets Manager (with rotation callback), "
        "HashiCorp Vault (AppRole + Kubernetes auth). Local TTL cache included.",
        "dotnet;platform;secrets;keyvault;aws-secrets-manager;vault",
    ),
    "MarcusPrado.Platform.Security": (
        "Security extensions: OidcClientService with proactive TokenCache renewal, "
        "AesGcmEncryption (AES-256-GCM), KeyRotationService, RSA/ECDSA digital signatures, "
        "PII masking, mTLS with CertificateTenantResolver, IInputSanitizer.",
        "dotnet;platform;security;oidc;encryption;aes-gcm;signatures;pii;mtls",
    ),
    "MarcusPrado.Platform.Serilog": (
        "Serilog configuration with structured enrichers (correlation ID, tenant ID), "
        "PII redaction, configurable sink routing (console, Seq), and log sanitization.",
        "dotnet;platform;serilog;logging;structured-logging",
    ),
    "MarcusPrado.Platform.SignalR": (
        "SignalR integration: PlatformHub<T> with JWT auth, tenant isolation, and "
        "correlation propagation. IRealtimePublisher, SignalRDomainEventSink.",
        "dotnet;platform;signalr;realtime;websockets",
    ),
    "MarcusPrado.Platform.Stripe": (
        "Stripe payment adapter: StripePaymentService, StripeSubscriptionService, StripeRefundService. "
        "Maps Stripe exceptions to Result<T> failures.",
        "dotnet;platform;stripe;payments;subscriptions",
    ),
    "MarcusPrado.Platform.Twilio": (
        "Twilio SMS adapter: TwilioSmsService implementing ISmsService with status callbacks.",
        "dotnet;platform;twilio;sms",
    ),
    # ── Kits ──────────────────────────────────────────────────────────────────
    "MarcusPrado.Platform.ApprovalTestKit": (
        "Snapshot testing for .NET: PlatformVerifySettings with scrubbers for DateTimeOffset, "
        "Guid, CorrelationId. ApiResponseVerifier, DomainEventVerifier, SqlQueryVerifier.",
        "dotnet;platform;testing;approval-tests;snapshot;verify",
    ),
    "MarcusPrado.Platform.ChaosKit": (
        "Chaos engineering for .NET: LatencyFault, ErrorFault, PacketLossFault "
        "with configurable injection rates (0–1.0). ChaosRunner integrates with ResilienceContext.",
        "dotnet;platform;chaos;fault-injection;testing;resilience",
    ),
    "MarcusPrado.Platform.ContractTestKit": (
        "Consumer-driven contract testing: PactVerifier via WebApplicationFactory + Pact Broker, "
        "PactPublisher with Git metadata, AsyncContractVerifier for messaging contracts.",
        "dotnet;platform;testing;pact;cdc;contract-testing",
    ),
    "MarcusPrado.Platform.IntegrationTestEnvironment": (
        "Integration test infrastructure: PlatformTestEnvironment starts all containers "
        "(Postgres, Redis, Kafka, RabbitMQ, NATS, MongoDB) in parallel. TestNetworkBuilder, "
        "SnapshotRestorer, TestEnvironmentHealthCheck.",
        "dotnet;platform;testing;testcontainers;integration-tests",
    ),
    "MarcusPrado.Platform.ObservabilityTestKit": (
        "Observability testing: InMemoryMetricCollector, InMemorySpanCollector, "
        "MetricAssertions, TraceAssertions for unit testing OTel instrumentation.",
        "dotnet;platform;testing;opentelemetry;metrics;tracing",
    ),
    "MarcusPrado.Platform.PerformanceTestKit": (
        "Load testing: PlatformLoadTest via NBomber, CommandThroughputScenario, "
        "ApiEndpointScenario with P50/P95/P99 latency reporting and error rate thresholds.",
        "dotnet;platform;testing;load-testing;performance;nbomber",
    ),
    "MarcusPrado.Platform.TestKit": (
        "Integration test base: IntegrationFixture, ApiFixture (WebApplicationFactory), "
        "FakeClock, FakeTenantContext, SnapshotRestorer, Eventually utility, "
        "Postgres/Redis/Kafka/RabbitMQ container fixtures.",
        "dotnet;platform;testing;integration-tests;testcontainers;fakes",
    ),
    # ── Tooling ───────────────────────────────────────────────────────────────
    "MarcusPrado.Platform.Analyzers": (
        "Custom Roslyn analyzers: PLATFORM001 (NoEfCoreInDomain), PLATFORM002 (NoAspNetInDomain), "
        "PLATFORM003 (DomainNoInfraReference), PLATFORM004 (EnforceResultType), "
        "PLATFORM005 (EnforceIdempotencyKey). AddResultWrapperCodeFix included.",
        "dotnet;platform;roslyn;analyzers;architecture;ddd",
    ),
    "MarcusPrado.Platform.ApiChangelog": (
        "API changelog CLI: ApiSurfaceExtractor (Reflection), ApiDiffEngine (breaking changes / additions), "
        "ChangelogRenderer (Markdown). CI integration exits with code 2 on breaking changes.",
        "dotnet;platform;tooling;api-changelog;breaking-changes",
    ),
    "MarcusPrado.Platform.Cli": (
        "Platform dotnet tool: scaffold api/worker/domain/command, config encrypt, "
        "catalog errors, arch validate, dlq inspect, health <url> commands.",
        "dotnet;platform;cli;tooling;scaffold",
    ),
    "MarcusPrado.Platform.Templates": (
        "dotnet new templates: platform-api, platform-worker, platform-domain, platform-command. "
        "Install via: dotnet new install MarcusPrado.Platform.Templates",
        "dotnet;platform;templates;dotnet-new;scaffold",
    ),
    "MarcusPrado.Platform.ArchTests": (
        "Architecture tests via NetArchTest: domain dependency rules, layering rules, "
        "naming conventions, contract compatibility rules.",
        "dotnet;platform;architecture;arch-tests;netarchtest",
    ),
}


def inject(csproj_path: str, name: str) -> None:
    meta = METADATA.get(name)
    if meta is None:
        print(f"  SKIP  {name} (no metadata defined)")
        return

    description, tags = meta

    tree = ET.parse(csproj_path)
    root = tree.getroot()

    # Find or create the first PropertyGroup
    pg = root.find("PropertyGroup")
    if pg is None:
        pg = ET.SubElement(root, "PropertyGroup")

    # Remove existing Description / PackageTags to avoid duplicates
    for tag in ("Description", "PackageTags"):
        existing = pg.find(tag)
        if existing is not None:
            pg.remove(existing)

    desc_el = ET.SubElement(pg, "Description")
    desc_el.text = description

    tags_el = ET.SubElement(pg, "PackageTags")
    tags_el.text = tags

    # Write back with consistent formatting
    ET.indent(tree, space="  ")
    tree.write(csproj_path, encoding="unicode", xml_declaration=False)

    # Ensure the SDK attribute line is preserved (ET strips it sometimes — add it back)
    with open(csproj_path, "r+") as f:
        content = f.read()
        if not content.startswith('<Project Sdk='):
            f.seek(0)
            f.write(f'<Project Sdk="Microsoft.NET.Sdk">\n')
            f.write(content.split("<Project>", 1)[-1] if "<Project>" in content else content)
            f.truncate()

    print(f"  OK    {name}")


def main() -> None:
    repo_root = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
    src_dirs = [
        os.path.join(repo_root, "src", "core"),
        os.path.join(repo_root, "src", "extensions"),
        os.path.join(repo_root, "src", "kits"),
        os.path.join(repo_root, "src", "tooling"),
    ]

    for src_dir in src_dirs:
        if not os.path.isdir(src_dir):
            continue
        for entry in sorted(os.scandir(src_dir), key=lambda e: e.name):
            if not entry.is_dir():
                continue
            csproj = os.path.join(entry.path, f"{entry.name}.csproj")
            if os.path.exists(csproj):
                inject(csproj, entry.name)


if __name__ == "__main__":
    main()
