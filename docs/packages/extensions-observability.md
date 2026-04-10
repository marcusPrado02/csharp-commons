# Observability Extensions

> `MarcusPrado.Platform.OpenTelemetry` · `MarcusPrado.Platform.Serilog` · `MarcusPrado.Platform.Observability`

OpenTelemetry traces, metrics, and logs configured in one call. Serilog with structured enrichers, PII redaction, and sink routing. Business metrics, SLO tracking, and Circuit Breaker dashboard included.

## Install

```bash
dotnet add package MarcusPrado.Platform.OpenTelemetry
dotnet add package MarcusPrado.Platform.Serilog
dotnet add package MarcusPrado.Platform.Observability
```

## One-Call Setup

```csharp
// Program.cs
builder.AddPlatformTelemetry(options =>
{
    options.ServiceName    = "order-service";
    options.ServiceVersion = "1.0.0";
    options.OtlpEndpoint   = new Uri("http://otel-collector:4317");
    options.EnableTracing  = true;
    options.EnableMetrics  = true;
    options.EnableLogs     = true;
});

builder.Host.UsePlatformSerilog(options =>
{
    options.MinimumLevel     = LogEventLevel.Information;
    options.WriteToConsole   = true;       // structured JSON in production
    options.WriteToSeq       = true;       // optional
    options.SeqUrl           = "http://seq:5341";
    options.RedactPii        = true;       // masks emails, CPF, phone numbers
    options.EnrichWithTenant = true;       // adds TenantId to every log entry
});
```

## Business Metrics

```csharp
// Inject IBusinessMetrics to record domain-level counters
public class OrderService(IBusinessMetrics metrics)
{
    public async Task<Result<OrderId>> PlaceOrderAsync(PlaceOrderCommand cmd, CancellationToken ct)
    {
        var result = await _handler.HandleAsync(cmd, ct);
        if (result.IsSuccess)
            metrics.RecordOrderPlaced(cmd.CustomerId, result.Value);
        return result;
    }
}

// Registration
builder.Services.AddPlatformBusinessMetrics();

// OTel counters emitted:
// platform.orders.placed        (labels: tenant_id)
// platform.payments.processed   (labels: tenant_id, currency)
// platform.users.signed_up      (labels: tenant_id, plan)
// platform.events.consumed      (labels: topic, consumer_group)
```

## SLO / Error Budget Tracking

```csharp
builder.Services.AddPlatformSlo(options =>
{
    options.TargetAvailability = 0.999; // 99.9%
    options.WindowDays         = 30;
});

// OTel gauges emitted:
// slo.availability              (current 30-day rolling availability)
// slo.error_budget_remaining    (fraction of error budget remaining, 0.0–1.0)
```

## Circuit Breaker Dashboard

```csharp
// Registration
builder.Services.AddPlatformCircuitBreakers();

// Exposes management endpoints:
// GET  /circuit-breakers              — list all breakers with state + failure counts
// POST /circuit-breakers/{name}/reset — manually reset a breaker to Closed

// OTel metrics:
// platform.circuit_breaker.state          (0=Closed, 1=Open, 2=HalfOpen)
// platform.circuit_breaker.failures_total  (cumulative failure count per breaker)
```

## Correlation Enrichment

`CorrelationMiddleware` sets `CorrelationId` on `ICorrelationContext` from the incoming `traceparent` / `X-Correlation-ID` header. Serilog and OTel both read from the ambient context, so every log entry and span automatically carries the same correlation ID that flows across service calls.

```csharp
// Enabled by default when calling UsePlatformMiddlewares()
app.UsePlatformMiddlewares();

// Log output includes: "CorrelationId": "01-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-01"
```

## Distributed Tracing Configuration

```csharp
// Sources registered by AddPlatformTelemetry():
// - "MarcusPrado.Platform.*"    — all platform spans
// - "Microsoft.AspNetCore"      — HTTP server spans
// - "Npgsql"                    — PostgreSQL spans
// - "Confluent.Kafka"           — Kafka producer/consumer spans

// Sampling — configurable per environment
options.Sampler = new TraceIdRatioBasedSampler(0.1); // 10% in production
```

## Key Types

| Type | Package | Purpose |
|------|---------|---------|
| `AddPlatformTelemetry()` | OpenTelemetry | Configures OTel SDK with traces, metrics, logs |
| `PlatformMeter` | OpenTelemetry | Named `Meter` for platform instruments |
| `OtelHealthCheckPublisher` | OpenTelemetry | Publishes health check results as OTel gauges |
| `UsePlatformSerilog()` | Serilog | Configures Serilog with enrichers and sinks |
| `SerilogPiiDestructuringPolicy` | Serilog | Masks PII values in structured logs |
| `IBusinessMetrics` | Observability | Domain-level business event counters |
| `OtelBusinessMetrics` | Observability | OTel implementation of `IBusinessMetrics` |
| `ServiceLevelObjective` | Observability | SLO record (target, window, current availability) |
| `ErrorBudgetCalculator` | Observability | Computes remaining error budget |
| `CircuitBreakerRegistry` | Observability | Central registry for all circuit breakers |
| `CorrelationContext` | Observability | Ambient correlation ID propagation |
| `CorrelationEnricher` | Observability | Serilog enricher for correlation ID |
| `LogSanitizer` | Observability | Strips sensitive fields from log events |

## Source

- [`src/extensions/MarcusPrado.Platform.OpenTelemetry`](../../src/extensions/MarcusPrado.Platform.OpenTelemetry)
- [`src/extensions/MarcusPrado.Platform.Serilog`](../../src/extensions/MarcusPrado.Platform.Serilog)
- [`src/core/MarcusPrado.Platform.Observability`](../../src/core/MarcusPrado.Platform.Observability)
