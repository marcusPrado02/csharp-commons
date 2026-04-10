# ADR-012 — W3C TraceContext Propagation (Open Standard, Not Proprietary Header)

| Field | Value |
|-------|-------|
| **Status** | Accepted |
| **Date** | 2026-03-01 |
| **Deciders** | Marcus Prado Silva |

---

## Context

Distributed tracing requires propagating a trace context (trace ID + span ID + flags) across service boundaries via HTTP headers. Several formats exist:

1. **`X-B3-TraceId` / `X-B3-SpanId` (Zipkin B3)** — widely adopted but Zipkin-specific.
2. **`X-Request-ID`** — no standard encoding; every team invents their own.
3. **`traceparent` / `tracestate` (W3C TraceContext, RFC 9525)** — vendor-neutral open standard adopted by OpenTelemetry, Azure, AWS X-Ray, Google Cloud Trace, Datadog.

The platform is designed to be deployable on any cloud. Choosing a proprietary propagation format creates lock-in and breaks compatibility when integrating with services from other teams that use a different APM vendor.

---

## Decision

Use **W3C TraceContext** (`traceparent` + `tracestate` headers) exclusively.

`CorrelationMiddleware` reads `traceparent` from incoming requests and sets it on `ICorrelationContext`. Outgoing HTTP requests via `TypedHttpClient` and message headers (Kafka, RabbitMQ, NATS) carry `traceparent` / `tracestate` automatically via OpenTelemetry's `TextMapPropagator`.

If no `traceparent` header is present (e.g., the request originates from a client that does not support tracing), a new trace ID is generated and attached to `ICorrelationContext`.

---

## Consequences

**Positive:**
- Traces span service boundaries correctly when integrated with any W3C-compliant APM: Jaeger, Zipkin (via compatibility bridge), Tempo, Datadog, Azure Monitor.
- No vendor lock-in — switching from Jaeger to Tempo requires only changing the OTLP exporter endpoint.
- `ICorrelationContext.CorrelationId` is always the W3C trace ID; consistent across logs, traces, and custom metrics.

**Negative:**
- Older internal services that use `X-B3-*` or `X-Request-ID` require an adapter shim if they must participate in the same trace. A `B3ToW3CPropagatorAdapter` is provided in `MarcusPrado.Platform.Observability` for this case.

**Neutral:**
- `tracestate` is propagated transparently but the platform does not write vendor-specific entries. Services may add their own `tracestate` entries via `Activity.Current?.TraceStateString`.
