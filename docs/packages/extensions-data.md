# Data Extensions

> `MarcusPrado.Platform.EfCore` · `MarcusPrado.Platform.Postgres` · `MarcusPrado.Platform.MySql` · `MarcusPrado.Platform.Redis` · `MarcusPrado.Platform.MongoDb` · `MarcusPrado.Platform.DataAccess`

Persistence adapters for EF Core (PostgreSQL, MySQL), Redis, and MongoDB. Each provides a consistent DI extension method, health probes, and telemetry integration without leaking infrastructure concerns into domain or application layers.

## Install

```bash
dotnet add package MarcusPrado.Platform.EfCore
dotnet add package MarcusPrado.Platform.Postgres     # or MySql / MongoDb
dotnet add package MarcusPrado.Platform.Redis
dotnet add package MarcusPrado.Platform.DataAccess   # distributed tracing for queries
```

## EF Core — DbContext

```csharp
// Extend AppDbContextBase to get audit filling, domain event dispatch,
// outbox/inbox tables, and tenant isolation for free.
public class OrderDbContext(DbContextOptions<OrderDbContext> options, ITenantContext tenant)
    : AppDbContextBase(options, tenant)
{
    public DbSet<Order> Orders { get; set; } = null!;

    protected override void ConfigureModel(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(typeof(OrderDbContext).Assembly);
    }
}

// Program.cs
builder.Services
    .AddDbContext<OrderDbContext>(opt =>
        opt.UseNpgsql(builder.Configuration.GetConnectionString("Orders")))
    .AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<EfUnitOfWork<OrderDbContext>>())
    .AddScoped<EfUnitOfWork<OrderDbContext>>();
```

`AppDbContextBase` automatically:
- Fills `IAuditable` properties (`CreatedAt`, `UpdatedAt`, `CreatedBy`) on `SaveChanges`
- Dispatches `IDomainEvent`s to `IDomainEventPublisher` after commit
- Applies multi-tenant discriminator filters via `ITenantContext`
- Stores outbox and inbox rows in the same transaction

## EF Core — Unit of Work

```csharp
// Use IUnitOfWork for explicit transaction control
public class PlaceOrderHandler(IOrderRepository orders, IUnitOfWork uow)
{
    public async Task<Result<OrderId>> HandleAsync(PlaceOrderCommand cmd, CancellationToken ct)
    {
        await using var tx = await uow.BeginTransactionAsync(ct);
        try
        {
            var order = Order.Create(cmd.CustomerId, cmd.Lines).Value;
            await orders.AddAsync(order, ct);
            await uow.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
            return order.Id;
        }
        catch
        {
            await tx.RollbackAsync(ct);
            throw;
        }
    }
}
```

## Redis — Caching

```csharp
// Registration
builder.Services.AddPlatformRedis(options =>
{
    options.ConnectionString = builder.Configuration["Redis:ConnectionString"];
    options.KeyPrefix        = "myapp:";
    options.DefaultExpiry    = TimeSpan.FromMinutes(15);
});

// Usage via ICache
public class ProductCacheService(ICache cache, IProductRepository repo)
{
    public async Task<ProductDto?> GetAsync(ProductId id, CancellationToken ct)
    {
        string key = $"product:{id}";
        var cached = await cache.GetAsync<ProductDto>(key, ct);
        if (cached is not null) return cached;

        var product = await repo.FindAsync(id, ct);
        if (product is not null)
            await cache.SetAsync(key, ProductDto.From(product), TimeSpan.FromMinutes(5), ct);

        return product is null ? null : ProductDto.From(product);
    }
}
```

## DataAccess — Distributed Tracing

```csharp
// EF Core — add the tracing interceptor to DbContext
builder.Services.AddDbContext<OrderDbContext>(opt =>
    opt.UseNpgsql(connStr)
       .AddInterceptors(new EfCoreTracingInterceptor()));
       // Adds db.system, db.statement (sanitized), db.operation OTel attributes

// Dapper — use the tracing wrapper methods
public class OrderQueries(NpgsqlConnection db)
{
    public Task<IEnumerable<OrderRow>> GetByCustomerAsync(CustomerId id) =>
        db.QueryWithTraceAsync<OrderRow>(
            "SELECT * FROM orders WHERE customer_id = @id",
            new { id = id.Value });
}
```

## Key Types

| Type | Package | Purpose |
|------|---------|---------|
| `AppDbContextBase` | EfCore | Base DbContext with audit, events, outbox, tenant |
| `EfUnitOfWork<T>` | EfCore | IUnitOfWork implementation with savepoint support |
| `EfOutboxStore` | EfCore | IOutboxStore backed by EF Core |
| `EfInboxStore` | EfCore | IInboxStore backed by EF Core |
| `EfMigrationRunner` | EfCore | Programmatic migration runner for startup |
| `PostgresConnectionFactory` | Postgres | Factory for `NpgsqlConnection` with health probe |
| `MySqlConnectionFactory` | MySql | Factory for `MySqlConnection` (Pomelo) with health probe |
| `ICache` | Redis | `GetAsync`, `SetAsync`, `RemoveAsync`, `ExistsAsync` |
| `RedisCache` | Redis | Redis implementation of `ICache` |
| `RedisQuotaStore` | Redis | Atomic increment for rate-limiting quotas |
| `IIdempotencyStore` | Redis | Stores idempotency keys for command deduplication |
| `EfCoreTracingInterceptor` | DataAccess | `IDbCommandInterceptor` adding OTel spans per SQL |
| `DapperTracingWrapper` | DataAccess | `QueryWithTraceAsync`, `ExecuteWithTraceAsync` |

## Source

- [`src/extensions/MarcusPrado.Platform.EfCore`](../../src/extensions/MarcusPrado.Platform.EfCore)
- [`src/extensions/MarcusPrado.Platform.Postgres`](../../src/extensions/MarcusPrado.Platform.Postgres)
- [`src/extensions/MarcusPrado.Platform.Redis`](../../src/extensions/MarcusPrado.Platform.Redis)
- [`src/extensions/MarcusPrado.Platform.DataAccess`](../../src/extensions/MarcusPrado.Platform.DataAccess)
