# Web Extensions

> `MarcusPrado.Platform.AspNetCore` · `MarcusPrado.Platform.AspNetCore.Auth` · `MarcusPrado.Platform.AspNetCore.ProblemDetails` · `MarcusPrado.Platform.Http`

ASP.NET Core extensions for Minimal API convention, API versioning, OpenAPI, CORS, rate limiting, security headers, IP filtering, request sizing, and response compression. All follow the `AddPlatformXxx` / `UsePlatformXxx` pattern and compose into a single fluent setup call.

## Install

```bash
dotnet add package MarcusPrado.Platform.AspNetCore
dotnet add package MarcusPrado.Platform.AspNetCore.Auth
dotnet add package MarcusPrado.Platform.AspNetCore.ProblemDetails
dotnet add package MarcusPrado.Platform.Http
```

## Canonical Program.cs

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddPlatformCore()                  // IClock, IGuidFactory, IJsonSerializer, contexts
    .AddPlatformCqrs()                  // CQRS pipeline + behaviors
    .AddPlatformProblemDetails()        // RFC 9457 ProblemDetails factory
    .AddPlatformOpenApi()               // Scalar UI + JWT/ApiKey auth schemes
    .AddPlatformApiVersioning()         // URL + header + media-type versioning
    .AddPlatformCors(CorsProfile.DevPermissive)
    .AddPlatformRateLimiting()          // tenant/user/IP policies via Redis
    .AddPlatformSecurityHeaders()
    .AddPlatformResponseCompression()
    .AddPlatformHealthChecks();

var app = builder.Build();

app.UsePlatformMiddlewares()           // correlation, tenant, exception, logging
   .UsePlatformDegradation()
   .UsePlatformRateLimiting()
   .UseSecurityHeaders()
   .UseResponseCompression()
   .UsePlatformHealthChecks();

app.MapPlatformEndpoints();            // auto-discovers IEndpoint implementations
app.Run();
```

## Minimal API Endpoints

```csharp
// Implement IEndpoint — auto-discovered by MapPlatformEndpoints()
public sealed class OrdersEndpoints : EndpointGroupBase
{
    public override void Map(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v{version:apiVersion}/orders")
            .WithApiVersionSet(VersionSets.V1)
            .RequireAuthorization();

        group.MapPost("/", CreateOrderAsync)
            .AddEndpointFilter<ValidationFilter<CreateOrderRequest>>()
            .AddEndpointFilter<ApiEnvelopeFilter>();

        group.MapGet("/{id}", GetOrderAsync);
    }

    private static async Task<IResult> CreateOrderAsync(
        CreateOrderRequest req, IDispatcher dispatcher, CancellationToken ct)
    {
        Result<OrderId> result = await dispatcher.SendAsync<CreateOrderCommand, OrderId>(
            req.ToCommand(), ct);

        return result.Match(
            onSuccess: id  => Results.Created($"/orders/{id}", new { id }),
            onFailure: err => err.ToProblemDetails());
    }
}
```

## HTTP Client with Propagation

```csharp
// Register a typed client with automatic correlation/tenant/auth header injection
builder.Services.AddPlatformHttpClient<OrderServiceClient>(options =>
{
    options.BaseAddress = new Uri("https://order-service/");
    options.Timeout = TimeSpan.FromSeconds(10);
});

// The typed client base class handles the rest
public class OrderServiceClient(HttpClient http) : TypedHttpClient(http)
{
    public Task<Result<OrderDto>> GetAsync(OrderId id, CancellationToken ct) =>
        GetAsync<OrderDto>($"/orders/{id}", ct);
}
```

## API Versioning

```csharp
// Declare version set per endpoint group
var versionSet = app.NewApiVersionSet()
    .HasApiVersion(new ApiVersion(1))
    .HasDeprecatedApiVersion(new ApiVersion(0, 9))
    .ReportApiVersions()
    .Build();

// Deprecation header is added automatically by DeprecationHeaderMiddleware
// Response includes: Deprecation: true, Sunset: <date>
```

## CORS Profiles

| Profile | Use case |
|---------|---------|
| `DevPermissive` | Local development — allows any origin |
| `StagingRestricted` | Staging — allows configured allow-list |
| `ProductionLocked` | Production — strict origin, method, header enforcement |

## Rate Limiting

```csharp
builder.Services.AddPlatformRateLimiting(options =>
{
    options.TenantPolicy   = new FixedWindowOptions { PermitLimit = 1000, Window = TimeSpan.FromMinutes(1) };
    options.UserPolicy     = new SlidingWindowOptions { PermitLimit = 100, Window = TimeSpan.FromSeconds(10) };
    options.IpPolicy       = new TokenBucketOptions  { TokenLimit = 50, ReplenishmentPeriod = TimeSpan.FromSeconds(1) };
    options.QuotaStore     = QuotaStoreType.Redis;  // or InMemory
});
// 429 responses include Retry-After header and RFC 9457 ProblemDetails
```

## Key Types

| Type | Purpose |
|------|---------|
| `IEndpoint` / `EndpointGroupBase` | Minimal API endpoint convention |
| `EndpointDiscovery.MapPlatformEndpoints()` | Auto-discovers and maps all `IEndpoint` implementations |
| `ApiEnvelopeFilter` | Wraps responses in `ApiEnvelope<T>` with metadata |
| `ValidationFilter<TRequest>` | Runs FluentValidation before handler; returns 422 on failure |
| `SecurityHeadersMiddleware` | X-Content-Type-Options, X-Frame-Options, CSP, Referrer-Policy |
| `IpFilterMiddleware` | CIDR-based IP allowlist/denylist with X-Forwarded-For support |
| `PlatformCorsPolicy` | Profile-based CORS configuration |
| `TypedHttpClient` | Base for typed HTTP clients with header propagation |
| `AuthTokenHandler` | DelegatingHandler that injects Bearer tokens |
| `CorrelationHeaderHandler` | DelegatingHandler that propagates correlation ID |
| `DeprecationHeaderMiddleware` | Adds `Deprecation` and `Sunset` headers |
| `PlatformOperationTransformer` | Adds platform context headers to OpenAPI spec |

## Source

- [`src/extensions/MarcusPrado.Platform.AspNetCore`](../../src/extensions/MarcusPrado.Platform.AspNetCore)
- [`src/extensions/MarcusPrado.Platform.Http`](../../src/extensions/MarcusPrado.Platform.Http)
