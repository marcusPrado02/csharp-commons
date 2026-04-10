# ADR-013 — AddStandardResilienceHandler() Over Raw Polly

| Field | Value |
|-------|-------|
| **Status** | Accepted |
| **Date** | 2026-03-05 |
| **Deciders** | Marcus Prado Silva |

---

## Context

HTTP clients making calls to external services need resilience: retry on transient errors, circuit breaker to stop hammering a failing service, timeout to bound latency. There are two main paths in the .NET ecosystem:

1. **Raw Polly v8** — maximum flexibility; requires writing pipeline composition (`ResiliencePipelineBuilder`) manually for every client.
2. **`Microsoft.Extensions.Http.Resilience`** — Microsoft's opinionated wrapper over Polly 8. `AddStandardResilienceHandler()` registers a pre-composed pipeline: retry (exponential backoff + jitter) → circuit breaker → attempt timeout, with OpenTelemetry integration built in.

The trade-off is configurability vs. convention. Most services in the target use-case need sensible defaults quickly; they rarely need to tune each parameter individually.

---

## Decision

Use **`AddStandardResilienceHandler()`** from `Microsoft.Extensions.Http.Resilience` as the default resilience configuration for `AddPlatformHttpClient<T>()`.

Advanced customization is available via `AddResilienceHandler()` for clients that need non-standard behaviour (e.g., a payment gateway client where the circuit breaker threshold must be much lower than the default).

```csharp
// Default — sensible retry + circuit breaker + timeout
builder.Services.AddPlatformHttpClient<OrderServiceClient>(opt => {
    opt.BaseAddress = new Uri("https://order-service/");
});

// Advanced override
builder.Services.AddPlatformHttpClient<PaymentClient>(opt => {
    opt.BaseAddress = new Uri("https://payment-gateway/");
}).AddResilienceHandler("payment", pipeline =>
{
    pipeline.AddCircuitBreaker(new CircuitBreakerStrategyOptions
    {
        FailureRatio           = 0.1,   // trip at 10% failure
        SamplingDuration       = TimeSpan.FromSeconds(30),
        MinimumThroughput      = 10,
        BreakDuration          = TimeSpan.FromSeconds(60),
    });
});
```

---

## Consequences

**Positive:**
- Zero boilerplate for the 90% case: retry + circuit breaker + timeout configured in one line.
- OpenTelemetry metrics for retry attempts, circuit breaker state transitions, and request duration are emitted automatically.
- `IHttpClientFactory` lifetime management is handled correctly — no `HttpClient` socket exhaustion.

**Negative:**
- `AddStandardResilienceHandler` defaults change between minor versions of `Microsoft.Extensions.Http.Resilience`. The platform pins major versions via `Directory.Packages.props`.
- The abstraction hides Polly v8 concepts; developers unfamiliar with the standard handler defaults (3 retries, 30s sampling window for CB) may be surprised by behaviour under load.

**Neutral:**
- `CorrelationHeaderHandler`, `TenantHeaderHandler`, and `AuthTokenHandler` are added as `DelegatingHandler` chains before the resilience handler, ensuring correlation/tenant/auth headers survive retries.
