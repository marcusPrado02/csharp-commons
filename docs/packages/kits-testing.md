# Testing Kits

> `MarcusPrado.Platform.TestKit` · `MarcusPrado.Platform.ContractTestKit` · `MarcusPrado.Platform.ChaosKit` · `MarcusPrado.Platform.ApprovalTestKit` · `MarcusPrado.Platform.PerformanceTestKit`

Platform-specific testing utilities for integration tests (real containers), consumer-driven contract tests (Pact), chaos engineering, snapshot-based approval tests, and load testing with P50/P95/P99 reporting.

## Install

```bash
dotnet add package MarcusPrado.Platform.TestKit
dotnet add package MarcusPrado.Platform.ContractTestKit
dotnet add package MarcusPrado.Platform.ChaosKit
dotnet add package MarcusPrado.Platform.ApprovalTestKit
dotnet add package MarcusPrado.Platform.PerformanceTestKit
```

## TestKit — Integration Test Infrastructure

```csharp
// Single fixture brings up all containers in parallel and manages their lifecycle
public class OrderServiceTests : IClassFixture<PlatformTestEnvironment>
{
    private readonly HttpClient _http;
    private readonly ICache _cache;

    public OrderServiceTests(PlatformTestEnvironment env)
    {
        _http  = env.CreateHttpClient();    // WebApplicationFactory with real DbContext
        _cache = env.GetRequiredService<ICache>();
    }

    [Fact]
    public async Task PlaceOrder_Returns201_WhenValid()
    {
        var response = await _http.PostAsJsonAsync("/api/v1/orders", new
        {
            customerId = Guid.NewGuid(),
            lines      = new[] { new { productId = Guid.NewGuid(), quantity = 2 } }
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }
}

// Fakes for unit tests
var clock  = new FakeClock(DateTimeOffset.Parse("2026-01-15T10:00:00Z"));
var tenant = new FakeTenantContext(TenantId.Parse("acme"));

// Eventually utility for async assertions
await Eventually.TrueAsync(
    condition: async () => (await _repo.CountPendingAsync()) == 0,
    timeout:   TimeSpan.FromSeconds(5));
```

### Snapshot Isolation

```csharp
// SnapshotRestorer ensures each test gets a clean database state
public class OrderRepositoryTests(PlatformTestEnvironment env) : IAsyncLifetime
{
    private IAsyncDisposable? _snapshot;

    public async Task InitializeAsync() =>
        _snapshot = await env.SnapshotRestorer.CreateSnapshotAsync();

    public async Task DisposeAsync() =>
        await _snapshot!.DisposeAsync(); // restores DB to pre-test state
}
```

## ContractTestKit — Consumer-Driven Contract Tests

```csharp
// Verify your service honors a Pact contract published by a consumer
public class OrderApiContractTests(PactVerifierFixture pact) : IClassFixture<PactVerifierFixture>
{
    [Fact]
    public Task VerifyOrderApiContract() =>
        pact.VerifyAsync<Program>(options =>
        {
            options.PactBrokerUrl      = "https://pact.example.com";
            options.ConsumerName       = "checkout-service";
            options.ProviderName       = "order-service";
            options.PublishVerification = true;
            options.ProviderVersion    = ThisAssembly.Git.CommitId;
        });
}
```

## ChaosKit — Fault Injection

```csharp
// Inject faults to verify resilience behavior
await ChaosRunner.RunWithChaosAsync(
    faults:
    [
        new LatencyFault(injectionRate: 0.3, delay: TimeSpan.FromMilliseconds(500)),
        new ErrorFault(injectionRate: 0.1, exception: new HttpRequestException("timeout"))
    ],
    action: async ct =>
    {
        // Code under test — should handle faults gracefully
        Result<OrderDto> result = await _client.GetOrderAsync(orderId, ct);
        result.IsSuccess.Should().BeTrue(); // circuit breaker should absorb errors
    });
```

## ApprovalTestKit — Snapshot Testing

```csharp
// Scrubbers eliminate non-deterministic values from snapshots
[UsesVerify]
public class OrderApiSnapshotTests
{
    [Fact]
    public async Task GetOrder_MatchesApprovedSnapshot()
    {
        var response = await _http.GetAsync($"/api/v1/orders/{_knownOrderId}");
        var json     = await response.Content.ReadAsStringAsync();

        await Verify(json, PlatformVerifySettings.Default);
        // PlatformVerifySettings scrubs: DateTimeOffset, Guid, CorrelationId, ETag
    }
}

// Domain event verification
await DomainEventVerifier.VerifyAsync(
    aggregate:  order,
    eventIndex: 0,
    verify:     evt => Verify(evt, PlatformVerifySettings.Default));

// SQL query verification (EF Core)
await SqlQueryVerifier.VerifyAsync<OrderDbContext>(
    ctx:     dbContext,
    action:  ctx => ctx.Orders.Where(o => o.Status == OrderStatus.Pending).ToListAsync(),
    verify:  sql => Verify(sql, PlatformVerifySettings.Default));
```

## PerformanceTestKit — Load Testing

```csharp
// Define a load test scenario
var scenario = ApiEndpointScenario.Create("POST /orders", async http =>
{
    var response = await http.PostAsJsonAsync("/api/v1/orders", TestData.ValidOrderRequest());
    return response.IsSuccessStatusCode;
});

var report = await PlatformLoadTest.RunAsync(new LoadTestOptions
{
    Scenario          = scenario,
    ConcurrentUsers   = 50,
    Duration          = TimeSpan.FromSeconds(30),
    WarmupDuration    = TimeSpan.FromSeconds(5),
    TargetRps         = 200,
});

report.P50.Should().BeLessThan(TimeSpan.FromMilliseconds(50));
report.P95.Should().BeLessThan(TimeSpan.FromMilliseconds(150));
report.P99.Should().BeLessThan(TimeSpan.FromMilliseconds(500));
report.ErrorRate.Should().BeLessThan(0.01); // < 1% error rate
```

## Key Types

| Type | Package | Purpose |
|------|---------|---------|
| `PlatformTestEnvironment` | TestKit | All containers (Postgres, Redis, Kafka, RabbitMQ) in parallel |
| `IntegrationFixture` | TestKit | WebApplicationFactory + real DB wiring |
| `FakeClock` | TestKit | Deterministic `IClock` for time-sensitive tests |
| `FakeTenantContext` | TestKit | Fixed `ITenantContext` for multi-tenant test isolation |
| `SnapshotRestorer` | TestKit | Per-test database state isolation |
| `Eventually` | TestKit | Async condition polling with configurable timeout |
| `PactVerifierFixture` | ContractTestKit | Verifies Pact contracts via `WebApplicationFactory` |
| `AsyncContractVerifier` | ContractTestKit | Messaging contract verification |
| `ChaosRunner` | ChaosKit | Executes actions with configurable fault injection |
| `LatencyFault` | ChaosKit | Adds artificial latency at given injection rate |
| `ErrorFault` | ChaosKit | Throws exceptions at given injection rate |
| `PacketLossFault` | ChaosKit | Simulates network packet loss |
| `PlatformVerifySettings` | ApprovalTestKit | Pre-configured Verify scrubbers for platform types |
| `ApiResponseVerifier` | ApprovalTestKit | Full HTTP response snapshot (status + headers + body) |
| `DomainEventVerifier` | ApprovalTestKit | Snapshot testing for domain events |
| `SqlQueryVerifier` | ApprovalTestKit | Snapshot testing for EF Core–generated SQL |
| `PlatformLoadTest` | PerformanceTestKit | NBomber-based load test runner |
| `ApiEndpointScenario` | PerformanceTestKit | HTTP endpoint load scenario with P50/P95/P99 |
| `CommandThroughputScenario` | PerformanceTestKit | CQRS command throughput scenario |
| `LoadTestReport` | PerformanceTestKit | Results with percentiles, RPS, error rate, HTML report |

## Source

- [`src/kits/MarcusPrado.Platform.TestKit`](../../src/kits/MarcusPrado.Platform.TestKit)
- [`src/kits/MarcusPrado.Platform.ContractTestKit`](../../src/kits/MarcusPrado.Platform.ContractTestKit)
- [`src/kits/MarcusPrado.Platform.ChaosKit`](../../src/kits/MarcusPrado.Platform.ChaosKit)
- [`src/kits/MarcusPrado.Platform.ApprovalTestKit`](../../src/kits/MarcusPrado.Platform.ApprovalTestKit)
- [`src/kits/MarcusPrado.Platform.PerformanceTestKit`](../../src/kits/MarcusPrado.Platform.PerformanceTestKit)
