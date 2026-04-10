# Resilience Extensions

> `MarcusPrado.Platform.DistributedLock` · `MarcusPrado.Platform.Degradation` · `MarcusPrado.Platform.HealthChecks` · `MarcusPrado.Platform.Configuration` · `MarcusPrado.Platform.Resilience`

Distributed locking, graceful degradation, advanced health checks, startup verification, hot-reload configuration, and resilience policies (retry, circuit breaker, bulkhead, rate limiter, hedging).

## Install

```bash
dotnet add package MarcusPrado.Platform.DistributedLock
dotnet add package MarcusPrado.Platform.Degradation
dotnet add package MarcusPrado.Platform.HealthChecks
dotnet add package MarcusPrado.Platform.Configuration
dotnet add package MarcusPrado.Platform.Resilience
```

## Distributed Lock

```csharp
builder.Services.AddPlatformDistributedLock(); // uses Redis by default

// Usage
public class InventoryService(IDistributedLock distributedLock)
{
    public async Task ReserveStockAsync(ProductId id, int qty, CancellationToken ct)
    {
        await using var handle = await distributedLock.AcquireAsync(
            key:    $"inventory:{id}",
            expiry: TimeSpan.FromSeconds(30),
            ct:     ct);

        if (handle is null)
            throw new ConflictException("Stock.LockTimeout", "Could not acquire inventory lock.");

        // Critical section — only one instance at a time across all pods
        await _repo.DecrementAsync(id, qty, ct);
    }
}
```

PostgreSQL advisory lock is also available as a dependency-free alternative:
```csharp
builder.Services.AddPlatformDistributedLock(options => options.UsePostgres = true);
```

## Graceful Degradation

Four explicit operating modes prevent the system from partially failing silently.

```csharp
builder.Services.AddPlatformDegradation();
app.UsePlatformDegradation();

// Switch mode via management endpoint
// POST /degradation/mode  { "mode": "ReadOnly" }

// Modes:
// None              — normal operation
// PartiallyDegraded — non-critical features disabled; core path continues
// ReadOnly          — write operations return 405 Method Not Allowed
// Maintenance       — all requests return 503 Service Unavailable

// Example: proactively enter ReadOnly before a database maintenance window
var controller = app.Services.GetRequiredService<IDegradationController>();
await controller.SetModeAsync(DegradationMode.ReadOnly);
```

## Advanced Health Checks

```csharp
builder.Services.AddPlatformHealthChecks(options =>
{
    options.IncludeMemoryPressure    = true;  // GC pressure check
    options.IncludeThreadPoolHealth  = true;  // thread pool starvation check
    options.HistoryDepth             = 20;    // last N check results per probe
});

// Implement IDependencyHealthProbe for custom checks
public class PaymentGatewayHealthProbe : IDependencyHealthProbe
{
    public string Name => "payment-gateway";

    public async Task<HealthStatus> CheckAsync(CancellationToken ct)
    {
        var response = await _http.GetAsync("/health", ct);
        return response.IsSuccessStatusCode ? HealthStatus.Healthy : HealthStatus.Unhealthy;
    }
}

// Endpoints exposed:
// GET /health/live    — liveness (always Healthy if process is running)
// GET /health/ready   — readiness (aggregates all IDependencyHealthProbe)
// GET /health/detail  — full details with history
```

## Startup Verification

Runs synchronous checks before the application begins serving traffic. If any check fails, `IHostApplicationLifetime.StopApplication()` is called — the pod exits cleanly and Kubernetes restarts it.

```csharp
builder.Services.AddStartupVerification(options =>
{
    options.AddDatabaseConnectivityVerification<OrderDbContext>();
    options.AddRequiredSecretsVerification(["ConnectionStrings:Orders", "Jwt:Secret"]);
    options.AddCustomVerification<MyCustomVerification>();
});
```

## Configuration Hot Reload

```csharp
// Inject IOptionsHotReload<T> instead of IOptions<T>
public class FeatureService(IOptionsHotReload<FeatureOptions> options)
{
    public bool IsEnabled(string feature) =>
        options.CurrentValue.EnabledFeatures.Contains(feature);
    // CurrentValue picks up changes written to appsettings.json without restart
}

// Audit log: every configuration change is logged with old/new value and timestamp
// Validation: invalid configs are rejected before being applied
```

## Cache Stampede Prevention

```csharp
builder.Services.AddPlatformCacheStampedeProtection();

// StampedeProtectedCache wraps ICache with a distributed lock + probabilistic early expiry
public class ProductCacheService(StampedeProtectedCache cache)
{
    public Task<ProductDto> GetAsync(ProductId id, CancellationToken ct) =>
        cache.GetOrSetAsync(
            key:     $"product:{id}",
            factory: () => _repo.GetDtoAsync(id, ct),
            expiry:  TimeSpan.FromMinutes(30),
            ct:      ct);
    // XFetch algorithm: begins refreshing early with increasing probability
    // as expiry approaches, preventing thundering-herd on high-traffic keys.
}
```

## Resilience Policies

```csharp
// Built-in policies from MarcusPrado.Platform.Resilience
var executor = new ResilientExecutor(new ResilienceContext
{
    RetryPolicy        = RetryPolicy.ExponentialBackoff(maxAttempts: 3),
    CircuitBreakerPolicy = CircuitBreakerPolicy.Defaults(),
    TimeoutPolicy      = TimeoutPolicy.Fixed(TimeSpan.FromSeconds(5)),
});

await executor.ExecuteAsync(async ct =>
{
    await _externalService.CallAsync(ct);
}, cancellationToken);
```

## Key Types

| Type | Package | Purpose |
|------|---------|---------|
| `IDistributedLock` | DistributedLock | `AcquireAsync(key, expiry, ct)` — returns `IAsyncDisposable?` |
| `RedisDistributedLock` | DistributedLock | Redlock algorithm with fencing token |
| `PostgresAdvisoryLock` | DistributedLock | PostgreSQL `pg_try_advisory_xact_lock` |
| `IDegradationController` | Degradation | `SetModeAsync()` / `GetModeAsync()` |
| `DegradationMode` | Degradation | None / PartiallyDegraded / ReadOnly / Maintenance |
| `DegradationMiddleware` | Degradation | Enforces operating mode on every request |
| `IDependencyHealthProbe` | HealthChecks | Custom health check interface |
| `IStartupVerification` | HealthChecks | Pre-startup verification contract |
| `StartupVerificationHostedService` | HealthChecks | Runs verifications before app starts serving |
| `MemoryPressureHealthCheck` | HealthChecks | GC pressure health check |
| `ThreadPoolStarvationHealthCheck` | HealthChecks | Thread pool queue depth health check |
| `IOptionsHotReload<T>` | Configuration | Wraps `IOptionsMonitor<T>` with change auditing |
| `EncryptedJsonConfigurationProvider` | Configuration | Decrypts `ENC(...)` config values at startup |
| `StampedeProtectedCache` | Resilience | XFetch probabilistic early expiry + lock |
| `ResilientExecutor` | Resilience | Compose retry + circuit breaker + timeout |

## Source

- [`src/extensions/MarcusPrado.Platform.DistributedLock`](../../src/extensions/MarcusPrado.Platform.DistributedLock)
- [`src/extensions/MarcusPrado.Platform.Degradation`](../../src/extensions/MarcusPrado.Platform.Degradation)
- [`src/extensions/MarcusPrado.Platform.HealthChecks`](../../src/extensions/MarcusPrado.Platform.HealthChecks)
- [`src/extensions/MarcusPrado.Platform.Configuration`](../../src/extensions/MarcusPrado.Platform.Configuration)
- [`src/core/MarcusPrado.Platform.Resilience`](../../src/core/MarcusPrado.Platform.Resilience)
