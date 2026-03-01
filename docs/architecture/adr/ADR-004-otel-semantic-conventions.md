# ADR-004 — OpenTelemetry Semantic Conventions for all telemetry

| Field | Value |
|-------|-------|
| **Status** | Accepted |
| **Date** | 2026-03-01 |
| **Deciders** | Platform team |
| **Technical story** | Platform Commons items #9 (Application behaviors), #21 (OpenTelemetry extension) |

---

## Context

Distributed tracing, metrics, and logs are only useful at scale when the
attribute names and metric names are consistent across all services. Without a
standard, every team invents their own names:

- Team A: `http.url`, Team B: `request.url`, Team C: `url`
- Team A: `db.query.duration`, Team B: `db_duration_ms`, Team C: (nothing)

This makes cross-service dashboards, alerts, and trace correlation impossible
without per-team mapping rules.

OpenTelemetry publishes **[Semantic Conventions](https://opentelemetry.io/docs/specs/semconv/)**
— a versioned specification for attribute names, metric names, and span names
covering HTTP, database, messaging, RPC, exceptions, and more.

---

## Decision

All telemetry emitted by platform components (behaviors, middleware, adapters)
**must** use OpenTelemetry Semantic Conventions for attribute and metric names.
Custom attributes must be namespaced under `marcusprado.*` and documented.

### Implementation

Attribute and metric name constants are centralised in
`MarcusPrado.Platform.Observability`:

```csharp
// Span names (verb.noun pattern from OTel conventions)
public static class TraceNames
{
    public const string CommandHandle   = "command.handle";
    public const string QueryHandle     = "query.handle";
    public const string MessagePublish  = "messaging.publish";
    public const string MessageProcess  = "messaging.process";
}

// Attribute keys (snake_case, OTel convention)
public static class SpanAttributes
{
    // Standard OTel
    public const string HttpMethod      = "http.request.method";
    public const string HttpStatusCode  = "http.response.status_code";
    public const string DbSystem        = "db.system";
    public const string DbStatement     = "db.query.text";
    public const string MessagingSystem = "messaging.system";
    public const string MessagingDest   = "messaging.destination.name";
    // Platform-custom
    public const string TenantId        = "marcusprado.tenant.id";
    public const string CorrelationId   = "marcusprado.correlation.id";
    public const string CommandType     = "marcusprado.command.type";
    public const string QueryType       = "marcusprado.query.type";
}

// Metric names (dot-notation, OTel convention)
public static class MetricNames
{
    public const string CommandDuration = "marcusprado.command.duration";
    public const string QueryDuration   = "marcusprado.query.duration";
    public const string MessagePublished = "marcusprado.messaging.published";
    public const string MessageConsumed  = "marcusprado.messaging.consumed";
    public const string HttpServerDuration = "http.server.request.duration";
}
```

### Mapping to OTLP backends

| Platform attribute | Grafana | Datadog | Jaeger |
|-------------------|---------|---------|--------|
| `marcusprado.tenant.id` | `resource.marcusprado.tenant_id` | `@marcusprado.tenant.id` | tag `tenant.id` |
| `marcusprado.correlation.id` | correlates with `trace.id` | `@http.headers.x-correlation-id` | tag `correlation.id` |

---

## Consequences

### Positive

- **Cross-service dashboards** work out of the box — same attribute names
  across all services.
- **Alert portability** — alerting rules defined on `http.server.request.duration`
  apply to every service using the platform.
- **Vendor agnosticism** — OTel-compliant backends (Grafana, Datadog, Honeycomb,
  Jaeger, Zipkin) understand standard attribute names without mapping.
- **Future-proof** — as OTel Semantic Conventions stabilise, platform constants
  are updated in one place.

### Negative / Trade-offs

- **Attribute name changes** — OTel Semantic Conventions are still evolving
  (many attributes moved from experimental to stable in 1.x). The platform
  must track these changes and version accordingly.
- **Verbosity** — OTel attribute names (`http.request.method`) are longer
  than legacy names (`http.method`). This is a deliberate OTel design choice.

---

## Alternatives Considered

| Alternative | Reason rejected |
|-------------|-----------------|
| No standard — each team chooses attribute names | Leads to inconsistent dashboards; cross-team observability impossible |
| Custom platform convention (non-OTel) | Requires mapping rules for every backend; not portable |
| Rely on auto-instrumentation only | Auto-instrumentation covers HTTP/DB but not domain-level (command/query/event) spans |

---

## References

- [OpenTelemetry Semantic Conventions v1.27.0](https://opentelemetry.io/docs/specs/semconv/)
- [OTel General Attributes](https://opentelemetry.io/docs/specs/semconv/general/attributes/)
- [OTel HTTP conventions](https://opentelemetry.io/docs/specs/semconv/http/)
- [OTel Messaging conventions](https://opentelemetry.io/docs/specs/semconv/messaging/)
- [OTel Database conventions](https://opentelemetry.io/docs/specs/semconv/database/)
- `src/core/MarcusPrado.Platform.Observability/` — `MetricNames`, `TraceNames`, `SpanAttributes`
- `src/core/MarcusPrado.Platform.Application/Behaviors/TracingBehavior.cs` — usage
