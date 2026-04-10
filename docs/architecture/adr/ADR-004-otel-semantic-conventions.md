# ADR-004 — OpenTelemetry Semantic Conventions for all telemetry

> **Summary**: All telemetry emitted by platform components (traces, metrics,
> logs) must use OpenTelemetry Semantic Conventions for attribute and metric
> names, with platform-custom attributes namespaced under `marcusprado.*`.
> Attribute constants are centralised in `MarcusPrado.Platform.Observability`.

| Field | Value |
|-------|-------|
| **Status** | Accepted |
| **Date** | 2026-03-01 |
| **Author** | Marcus Prado Silva (Platform Architect) |
| **Tags** | observability, opentelemetry, metrics, tracing, standards |
| **Supersedes** | — |
| **Superseded by** | — |

---

## Context

Distributed tracing, metrics, and logs are only useful at scale when attribute
names and metric names are **consistent across all services**. Without a
standard, every team invents its own naming conventions:

```
# Real examples collected from pre-platform services:
Team A: http.url       Team B: request.url     Team C: url
Team A: db.duration_ms  Team B: db_query_time  Team C: (nothing)
Team A: tenant         Team B: tenantId        Team C: X-Tenant-ID
```

This makes cross-service dashboards, alert thresholds, and trace correlation
impossible without per-team mapping rules in the observability backend — a
maintenance burden that grows with every new service.

OpenTelemetry publishes **[Semantic Conventions](https://opentelemetry.io/docs/specs/semconv/)**
— a versioned, open specification for attribute names, metric names, and span
names covering HTTP, databases, messaging, RPC, exceptions, and more. These
conventions are understood natively by all major OTLP-compatible backends
(Grafana, Datadog, Honeycomb, Jaeger, Zipkin, Azure Monitor).

---

## Decision

All telemetry emitted by platform components (behaviors, middleware, adapters)
**must** use OTel Semantic Conventions for attribute and metric names.
Platform-custom attributes (tenant ID, correlation ID, command type) must be
namespaced under `marcusprado.*` and documented in this codebase.

Attribute and metric name constants are centralised in
`MarcusPrado.Platform.Observability` to ensure consistency and enable
find-all-usages refactoring when conventions change.

### Centralised constants

```csharp
namespace MarcusPrado.Platform.Observability;

/// <summary>Span names — verb.noun pattern from OTel conventions.</summary>
public static class TraceNames
{
    public const string CommandHandle    = "command.handle";
    public const string QueryHandle      = "query.handle";
    public const string MessagePublish   = "messaging.publish";
    public const string MessageProcess   = "messaging.process";
    public const string DlqReprocess     = "dlq.reprocess";
}

/// <summary>
/// Span attribute keys. Standard keys follow OTel Semantic Conventions 1.27+;
/// platform-custom keys are prefixed with <c>marcusprado.</c>.
/// </summary>
public static class SpanAttributes
{
    // ── Standard OTel (HTTP) ─────────────────────────────────────────────────
    public const string HttpMethod      = "http.request.method";
    public const string HttpStatusCode  = "http.response.status_code";
    public const string HttpRoute       = "http.route";

    // ── Standard OTel (Database) ─────────────────────────────────────────────
    public const string DbSystem        = "db.system";
    public const string DbName          = "db.namespace";
    public const string DbStatement     = "db.query.text";
    public const string DbOperation     = "db.operation.name";

    // ── Standard OTel (Messaging) ────────────────────────────────────────────
    public const string MessagingSystem    = "messaging.system";
    public const string MessagingDest      = "messaging.destination.name";
    public const string MessagingOperation = "messaging.operation.type";
    public const string MessagingMessageId = "messaging.message.id";

    // ── Standard OTel (Exceptions) ───────────────────────────────────────────
    public const string ExceptionType    = "exception.type";
    public const string ExceptionMessage = "exception.message";

    // ── Platform-custom ──────────────────────────────────────────────────────
    public const string TenantId         = "marcusprado.tenant.id";
    public const string CorrelationId    = "marcusprado.correlation.id";
    public const string CommandType      = "marcusprado.command.type";
    public const string QueryType        = "marcusprado.query.type";
    public const string HandlerType      = "marcusprado.handler.type";
    public const string IdempotencyKey   = "marcusprado.idempotency.key";
    public const string ErrorCode        = "marcusprado.error.code";
    public const string ErrorCategory    = "marcusprado.error.category";
}

/// <summary>
/// Metric names — dot-notation, OTel convention.
/// Standard names match OTel Semantic Conventions where applicable.
/// </summary>
public static class MetricNames
{
    // ── Standard OTel (HTTP Server) ──────────────────────────────────────────
    public const string HttpServerDuration   = "http.server.request.duration";

    // ── Platform CQRS ────────────────────────────────────────────────────────
    public const string CommandDuration      = "marcusprado.command.duration";
    public const string CommandTotal         = "marcusprado.command.total";
    public const string QueryDuration        = "marcusprado.query.duration";
    public const string QueryTotal           = "marcusprado.query.total";

    // ── Platform Messaging ───────────────────────────────────────────────────
    public const string MessagesPublished    = "marcusprado.messaging.published";
    public const string MessagesConsumed     = "marcusprado.messaging.consumed";
    public const string MessagesDeadLettered = "marcusprado.messaging.dead_lettered";

    // ── Platform Resilience ──────────────────────────────────────────────────
    public const string RetryAttempts        = "marcusprado.retry.attempts";
    public const string CircuitBreakerState  = "marcusprado.circuit_breaker.state";
}
```

### Span lifecycle — TracingBehavior example

```csharp
// src/core/MarcusPrado.Platform.Application/Behaviors/TracingBehavior.cs
internal sealed class TracingBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
{
    private static readonly ActivitySource _source =
        new("MarcusPrado.Platform", PlatformVersion.Current);

    public async Task<TResponse> HandleAsync(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        var spanName = request is ICommand
            ? TraceNames.CommandHandle
            : TraceNames.QueryHandle;

        using var activity = _source.StartActivity(
            spanName, ActivityKind.Internal);

        activity?.SetTag(SpanAttributes.CommandType, typeof(TRequest).Name);
        activity?.SetTag(SpanAttributes.TenantId, _tenantContext.TenantId);
        activity?.SetTag(SpanAttributes.CorrelationId, _correlationContext.CorrelationId);

        var response = await next();

        // Result<T> → span status: no exceptions thrown for expected failures
        if (response is IResult { IsFailure: true } result)
        {
            activity?.SetStatus(ActivityStatusCode.Error, result.Error.Message);
            activity?.SetTag(SpanAttributes.ErrorCode, result.Error.Code);
            activity?.SetTag(SpanAttributes.ErrorCategory, result.Error.Category.ToString());
        }
        else
        {
            activity?.SetStatus(ActivityStatusCode.Ok);
        }

        return response;
    }
}
```

### Baggage vs. attributes

| Concern | Mechanism | Propagated across services? |
|---------|-----------|:---:|
| `CorrelationId` | Span attribute + W3C Baggage | Yes (via `traceparent` + `baggage` headers) |
| `TenantId` | Span attribute + W3C Baggage | Yes |
| `IdempotencyKey` | Span attribute only | No — request-local |
| `ErrorCode` | Span attribute only | No — leaf-span only |

### Backend mapping

| Platform attribute | Grafana Tempo | Datadog APM | Jaeger |
|-------------------|--------------|-------------|--------|
| `marcusprado.tenant.id` | `resource.marcusprado.tenant.id` | `@marcusprado.tenant.id` | tag `tenant.id` |
| `marcusprado.correlation.id` | correlates via `trace_id` | `@x-correlation-id` | tag `correlation.id` |
| `marcusprado.error.code` | span attribute filter | facet `@error.code` | tag `error.code` |
| `marcusprado.command.type` | span name breakdown | operation dimension | process tag |

---

## Consequences

### Positive

- **Cross-service dashboards work out of the box** — same attribute and metric
  names across all services; a Grafana dashboard written for Service A works
  for Service B.
- **Alert portability** — `http.server.request.duration` alert thresholds apply
  to every service using the platform, with no per-service configuration.
- **Vendor agnosticism** — switching from Jaeger to Tempo to Datadog requires
  no platform code changes; OTLP backends understand OTel attribute names.
- **Future-proof** — OTel Semantic Conventions are versioned and stable; the
  platform tracks them in one place (`MarcusPrado.Platform.Observability`).
- **Consistent `Result<T>` integration** — expected failures set span status
  to `Error` without throwing exceptions, keeping span error counts meaningful
  and not inflated by legitimate business failures.

### Negative / Trade-offs

- **Attribute name evolution** — OTel Semantic Conventions are still maturing
  (e.g., `http.url` → `url.full` in v1.21, `db.statement` → `db.query.text`
  in v1.26). The platform must track these renames and provide a migration
  guide for existing dashboards.
- **Verbosity** — OTel attribute names (`http.request.method`) are longer
  than legacy shorthand (`http.method`). This is an intentional OTel design
  trade-off for unambiguity.
- **Baggage propagation overhead** — W3C Baggage adds a small HTTP header to
  every outbound request. This is negligible in practice but worth noting
  for very high-fanout architectures.

---

## Alternatives Considered

| Alternative | Reason rejected |
|-------------|-----------------|
| No standard — each service team chooses attribute names | Leads to divergent dashboards and cross-team trace correlation failures |
| Custom platform convention (non-OTel) | Requires mapping rules in every OTLP backend; not portable; becomes a private standard to maintain |
| Rely entirely on auto-instrumentation | Auto-instrumentation covers HTTP/DB but misses domain-level spans (command/query/event); no platform-custom attributes |
| Use `ILogger` structured properties as the sole correlation mechanism | Works for log correlation but does not integrate with distributed traces or Prometheus-style metrics |

---

## References

- [OpenTelemetry Semantic Conventions v1.27.0](https://opentelemetry.io/docs/specs/semconv/)
- [OTel General Attribute Guidelines](https://opentelemetry.io/docs/specs/semconv/general/attributes/)
- [OTel HTTP Conventions](https://opentelemetry.io/docs/specs/semconv/http/)
- [OTel Messaging Conventions](https://opentelemetry.io/docs/specs/semconv/messaging/)
- [OTel Database Conventions](https://opentelemetry.io/docs/specs/semconv/database/)
- [W3C Trace Context — Baggage](https://www.w3.org/TR/baggage/)
- ADR-002 — `TracingBehavior` is the #3 behavior in the CQRS pipeline
- `src/core/MarcusPrado.Platform.Observability/` — `MetricNames`, `TraceNames`, `SpanAttributes`
- `src/core/MarcusPrado.Platform.Application/Behaviors/TracingBehavior.cs` — usage
- `src/core/MarcusPrado.Platform.Application/Behaviors/MetricsBehavior.cs` — OTel metrics
- `src/extensions/MarcusPrado.Platform.OpenTelemetry/` — OTLP exporter wiring
