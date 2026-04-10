# Core Abstractions

> `MarcusPrado.Platform.Abstractions`

The foundation of the platform. Defines the core contracts — `Result<T>`, typed errors, CQRS interfaces, context propagation, and primitive utilities — without any runtime dependency other than the BCL.

## Install

```bash
dotnet add package MarcusPrado.Platform.Abstractions
```

## Result\<T\> and Error

All operations that can fail return `Result<T>` — a readonly struct that is either a success value or a typed `Error`. There are no exceptions for expected failures.

```csharp
// Returning a result from a repository or service
public async Task<Result<Order>> GetOrderAsync(OrderId id, CancellationToken ct)
{
    var order = await _db.Orders.FindAsync(id, ct);
    if (order is null)
        return Error.NotFound("Order.NotFound", $"Order {id} does not exist.");

    return order; // implicit conversion T → Result<T>
}

// Consuming a result with Match
Result<Order> result = await _orderService.GetOrderAsync(orderId, ct);

string response = result.Match(
    onSuccess: order => $"Order {order.Id} — {order.Status}",
    onFailure: error  => $"[{error.Category}] {error.Code}: {error.Message}"
);

// Chaining with Bind / Map
Result<OrderDto> dto = result
    .Bind(order => ValidateCanRead(order, currentUserId))
    .Map(order => _mapper.ToDto(order));
```

### Error factory methods

```csharp
Error.Validation("Order.InvalidAmount", "Amount must be positive.");
Error.NotFound("Customer.NotFound", $"Customer {id} not found.");
Error.Conflict("Order.AlreadyShipped", "Order has already been shipped.");
Error.Unauthorized("Auth.MissingToken", "Bearer token is required.");
Error.Forbidden("Order.AccessDenied", "You do not own this order.");
Error.Technical("Db.Timeout", "Database call timed out.").WithSeverity(ErrorSeverity.Critical);
Error.External("Stripe.ApiDown", "Payment provider is unavailable.");
```

## CQRS Interfaces

```csharp
// Commands (write side) — dispatched via ICommandBus
public record CreateOrderCommand(CustomerId CustomerId, IReadOnlyList<LineItem> Lines)
    : ICommand<OrderId>;

// Queries (read side) — dispatched via IQueryBus
public record GetOrderByIdQuery(OrderId Id) : IQuery<OrderDto>;

// Injecting the dispatcher (implements both buses)
public class OrdersController(IDispatcher dispatcher)
{
    public async Task<IResult> Post(CreateOrderRequest req, CancellationToken ct)
    {
        Result<OrderId> result = await dispatcher.SendAsync<CreateOrderCommand, OrderId>(
            new CreateOrderCommand(req.CustomerId, req.Lines), ct);

        return result.Match(
            onSuccess: id  => Results.Created($"/orders/{id}", id),
            onFailure: err => err.ToProblemDetails());
    }
}
```

## Context Interfaces

| Interface | Purpose |
|-----------|---------|
| `ICorrelationContext` | Carries `CorrelationId` across service boundaries via W3C TraceContext |
| `ITenantContext` | `TenantId` for multi-tenant isolation |
| `IUserContext` | `UserId`, `Claims`, `IsAuthenticated` |
| `IRequestContext` | Aggregate: correlation + tenant + user + culture |

## Primitive Utilities

| Type | Purpose |
|------|---------|
| `IClock` | Abstracts `DateTimeOffset.UtcNow` for testability |
| `IGuidFactory` | `NewGuid()` — mockable in deterministic tests |
| `IJsonSerializer` | `Serialize<T>` / `Deserialize<T>` — decoupled from `System.Text.Json` |
| `IHasher` | SHA-256/HMAC hashing |
| `IEncryption` | Encrypt / Decrypt bytes |
| `ICompression` | Compress / Decompress streams |

## Key Types

| Type | Package | Purpose |
|------|---------|---------|
| `Result<T>` | Abstractions | Discriminated union for success or typed error |
| `Result` | Abstractions | Non-generic variant for void operations |
| `Error` | Abstractions | Immutable error with code, message, category, severity |
| `ErrorCategory` | Abstractions | Validation / NotFound / Conflict / Unauthorized / Forbidden / Technical / External / Timeout |
| `ICommandBus` | Abstractions | Fire-and-forget or return-valued command dispatch |
| `IQueryBus` | Abstractions | Read-only query dispatch |
| `IDispatcher` | Abstractions | Combines `ICommandBus` + `IQueryBus` |
| `IUnitOfWork` | Abstractions | Transaction boundary abstraction |

## Source

[`src/core/MarcusPrado.Platform.Abstractions`](../../src/core/MarcusPrado.Platform.Abstractions)
