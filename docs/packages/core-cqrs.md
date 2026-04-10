# Core CQRS Pipeline

> `MarcusPrado.Platform.Application`

A zero-MediatR CQRS pipeline built on top of the platform's `ICommandBus` / `IQueryBus` abstractions. Handlers are registered via DI; cross-cutting behaviors (validation, tracing, idempotency, transactions) compose as an ordered pipeline. Each behavior has a single responsibility and can be added or removed independently.

## Install

```bash
dotnet add package MarcusPrado.Platform.Application
```

## Registration

```csharp
// Program.cs / Startup.cs
builder.Services.AddPlatformCqrs(options =>
{
    options.AddValidationBehavior();    // FluentValidation → Result.Failure
    options.AddLoggingBehavior();       // structured request/response logs
    options.AddTracingBehavior();       // OTel span per command/query
    options.AddMetricsBehavior();       // counter + histogram per handler
    options.AddAuthorizationBehavior(); // IAuthorizationHandler gate
    options.AddIdempotencyBehavior();   // deduplication via IIdempotencyStore
    options.AddTransactionBehavior();   // IUnitOfWork wrapping write commands
    options.AddRetryBehavior();         // transient-error retry
});
```

## Defining Commands and Handlers

```csharp
// Command — carries the intent (write operation)
public record PlaceOrderCommand(
    CustomerId CustomerId,
    IReadOnlyList<LineItem> Lines,
    [IdempotencyKey] string IdempotencyKey  // activates IdempotencyBehavior
) : ICommand<OrderId>;

// Handler — single method, depends on domain + infrastructure abstractions
public sealed class PlaceOrderHandler(
    IOrderRepository orders,
    IUnitOfWork uow,
    IDomainEventPublisher publisher)
    : ICommandHandler<PlaceOrderCommand, OrderId>
{
    public async Task<Result<OrderId>> HandleAsync(PlaceOrderCommand cmd, CancellationToken ct)
    {
        Result<Order> result = Order.Create(cmd.CustomerId, cmd.Lines);
        if (result.IsFailure) return result.Error;

        await orders.AddAsync(result.Value, ct);
        await uow.SaveChangesAsync(ct);          // dispatches domain events post-commit
        return result.Value.Id;
    }
}
```

## Defining Queries and Handlers

```csharp
// Query — read-only, no side effects
public record GetOrderSummaryQuery(OrderId OrderId) : IQuery<OrderSummaryDto>;

public sealed class GetOrderSummaryHandler(IReadRepository<Order> repo)
    : IQueryHandler<GetOrderSummaryQuery, OrderSummaryDto>
{
    public async Task<Result<OrderSummaryDto>> HandleAsync(
        GetOrderSummaryQuery query, CancellationToken ct)
    {
        var order = await repo.FindAsync(query.OrderId, ct);
        if (order is null)
            return Error.NotFound("Order.NotFound", $"Order {query.OrderId} not found.");

        return new OrderSummaryDto(order.Id, order.Status, order.Total);
    }
}
```

## Custom Pipeline Behavior

```csharp
public sealed class AuditBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ICommand
{
    public async Task<TResponse> HandleAsync(
        TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        var response = await next();
        // post-processing: write audit log
        return response;
    }
}
```

## Built-in Behaviors

| Behavior | Trigger | What it does |
|----------|---------|--------------|
| `ValidationBehavior` | `IValidator<TRequest>` in DI | Runs FluentValidation; returns `Result.Failure` on invalid |
| `LoggingBehavior` | Always | Structured log on entry + exit with duration |
| `TracingBehavior` | Always | OTel span per handler with `command.type` attribute |
| `MetricsBehavior` | Always | `platform.cqrs.requests` counter + `platform.cqrs.duration` histogram |
| `AuthorizationBehavior` | `[Authorize]` attribute on request | Calls `IAuthorizationHandler`; returns `Forbidden` on denial |
| `IdempotencyBehavior` | `[IdempotencyKey]` property on request | Checks `IIdempotencyStore`; replays cached result for duplicates |
| `TransactionBehavior` | `ICommand` (not `IQuery`) | Wraps handler in `IUnitOfWork` transaction |
| `RetryBehavior` | Transient exceptions | Exponential backoff with jitter |

## Exception Types

| Type | HTTP mapping | When to throw |
|------|-------------|---------------|
| `ValidationException` | 422 Unprocessable | Invalid input format |
| `NotFoundException` | 404 Not Found | Resource does not exist |
| `ConflictException` | 409 Conflict | State conflict |
| `UnauthorizedException` | 401 Unauthorized | Missing/invalid auth |
| `ForbiddenException` | 403 Forbidden | Insufficient permissions |
| `AppException` | 500 Internal | Unexpected application error |

> These exceptions are automatically mapped to `ProblemDetails` by `ExceptionMiddleware` in `MarcusPrado.Platform.AspNetCore`.

## Source

[`src/core/MarcusPrado.Platform.Application`](../../src/core/MarcusPrado.Platform.Application)
