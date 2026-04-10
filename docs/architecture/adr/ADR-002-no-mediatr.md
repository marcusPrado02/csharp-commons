# ADR-002 — Own CQRS pipeline instead of MediatR

> **Summary**: The platform ships its own command/query dispatcher and pipeline
> behavior chain rather than depending on MediatR, eliminating third-party
> versioning risk, enabling native `Result<T>` integration, and giving the
> platform full control over cross-cutting pipeline concerns.

| Field | Value |
|-------|-------|
| **Status** | Accepted |
| **Date** | 2026-03-01 |
| **Author** | Marcus Prado Silva (Platform Architect) |
| **Tags** | cqrs, application-layer, dependencies, pipeline |
| **Supersedes** | — |
| **Superseded by** | — |

---

## Context

MediatR is the de-facto standard mediator library for .NET CQRS. However, a
platform library must remain a *foundation* that teams build on top of —
coupling it to a specific third-party dispatcher transfers that library's
release cadence, licensing decisions, and API surface changes to every team
that adopts the platform.

### Why MediatR was ruled out

**Breaking change history**: MediatR v12 introduced breaking changes that
removed `IRequest<TResponse>` and unified handler registration patterns.
Services using the platform could not absorb this upgrade independently of
the platform; a coordinated, multi-team migration was required.

**Result<T> integration**: MediatR's `IRequest<TResponse>` returns raw `TResponse`.
Wrapping every handler return value in `Result<T>` requires custom behaviour
infrastructure that essentially replaces MediatR's core value proposition.

**Platform context propagation**: The pipeline needs first-class
`ICorrelationContext` and `ITenantContext` propagation at the behaviour level.
These are platform abstractions unknown to MediatR; wiring them in requires
additional infrastructure on top of MediatR, not alongside it.

**Idempotency behaviour**: `IdempotencyBehavior` must interact with
`IIdempotencyStore`, a platform abstraction that has no MediatR equivalent.
Implementing this on top of MediatR requires reimplementing much of MediatR's
pipeline anyway.

---

## Decision

Implement a first-party CQRS pipeline in `MarcusPrado.Platform.Application`
with no dependency on MediatR or any other third-party dispatcher.

### Core abstractions

```csharp
// ── Commands (side effects; return Result) ───────────────────────────────────

public interface ICommand { }
public interface ICommand<TResult> : ICommand { }

public interface ICommandHandler<TCommand>
    where TCommand : ICommand
{
    Task<Result> HandleAsync(TCommand command, CancellationToken ct);
}

public interface ICommandHandler<TCommand, TResult>
    where TCommand : ICommand<TResult>
{
    Task<Result<TResult>> HandleAsync(TCommand command, CancellationToken ct);
}

// ── Queries (read-only; return Result<T>) ────────────────────────────────────

public interface IQuery<TResult> { }

public interface IQueryHandler<TQuery, TResult>
    where TQuery : IQuery<TResult>
{
    Task<Result<TResult>> HandleAsync(TQuery query, CancellationToken ct);
}

// ── Pipeline ─────────────────────────────────────────────────────────────────

public delegate Task<TResponse> RequestHandlerDelegate<TResponse>();

public interface IPipelineBehavior<TRequest, TResponse>
{
    Task<TResponse> HandleAsync(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct);
}

// ── Dispatch (DI entry points) ────────────────────────────────────────────────

public interface ICommandBus
{
    Task<Result> SendAsync<TCommand>(TCommand command, CancellationToken ct)
        where TCommand : ICommand;

    Task<Result<TResult>> SendAsync<TCommand, TResult>(TCommand command, CancellationToken ct)
        where TCommand : ICommand<TResult>;
}

public interface IQueryBus
{
    Task<Result<TResult>> QueryAsync<TResult>(IQuery<TResult> query, CancellationToken ct);
}
```

### Shipped pipeline behaviors (execution order)

| Order | Behavior | Purpose |
|-------|----------|---------|
| 1 | `LoggingBehavior` | Structured request/response logging with duration |
| 2 | `MetricsBehavior` | OTel counter + histogram per command/query type |
| 3 | `TracingBehavior` | OTel span per command/query with `marcusprado.*` attributes |
| 4 | `ValidationBehavior` | Runs `IValidator<T>` (FluentValidation); returns `Result.Failure` on invalid |
| 5 | `AuthorizationBehavior` | Checks `IPolicyAuthorizer`; returns `ErrorCategory.Forbidden` if denied |
| 6 | `IdempotencyBehavior` | Checks `IIdempotencyStore`; short-circuits on duplicate key |
| 7 | `TransactionBehavior` | Wraps handler in `IUnitOfWork.BeginTransactionAsync` |
| 8 | `RetryBehavior` | Retries transient failures via `IResiliencePolicy` |

### DI registration

```csharp
// In application startup (e.g., Program.cs or IServiceCollection extension)
builder.Services.AddPlatformCqrs(options =>
{
    options.RegisterHandlersFromAssembly(typeof(CreateOrderHandler).Assembly);

    // Pipeline is ordered explicitly — no magic ordering by convention
    options.AddBehavior(typeof(LoggingBehavior<,>));
    options.AddBehavior(typeof(MetricsBehavior<,>));
    options.AddBehavior(typeof(TracingBehavior<,>));
    options.AddBehavior(typeof(ValidationBehavior<,>));
    options.AddBehavior(typeof(AuthorizationBehavior<,>));
    options.AddBehavior(typeof(IdempotencyBehavior<,>));
    options.AddBehavior(typeof(TransactionBehavior<,>));
    options.AddBehavior(typeof(RetryBehavior<,>));
});
```

### End-to-end usage example

```csharp
// ── Command definition ───────────────────────────────────────────────────────
public sealed record CreateOrderCommand(
    Guid CustomerId,
    IReadOnlyList<OrderLineDto> Lines) : ICommand<OrderId>;

// ── Validator (picked up automatically by ValidationBehavior) ────────────────
public sealed class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.CustomerId).NotEmpty();
        RuleFor(x => x.Lines).NotEmpty().WithMessage("Order must have at least one line.");
    }
}

// ── Handler ──────────────────────────────────────────────────────────────────
public sealed class CreateOrderHandler : ICommandHandler<CreateOrderCommand, OrderId>
{
    public async Task<Result<OrderId>> HandleAsync(
        CreateOrderCommand command, CancellationToken ct)
    {
        var order = Order.Create(command.CustomerId, command.Lines);
        if (order.IsFailure) return order.Error;

        await _repository.AddAsync(order.Value, ct);
        return order.Value.Id;  // implicit OrderId → Result<OrderId>
    }
}

// ── Minimal API endpoint ──────────────────────────────────────────────────────
app.MapPost("/orders", async (CreateOrderCommand cmd, ICommandBus bus, CancellationToken ct) =>
{
    var result = await bus.SendAsync<CreateOrderCommand, OrderId>(cmd, ct);
    return result.Match(
        onSuccess: id    => Results.Created($"/orders/{id}", new { id }),
        onFailure: error => Results.Problem(error.ToProblemDetails()));
});
```

---

## Consequences

### Positive

- **No external dependency** — the pipeline is versioned with the platform;
  no upstream breaking changes force consumer migrations.
- **`Result<T>` native** — behaviors speak the same language as handlers;
  no exception wrapping or adapter code needed.
- **Platform context propagation** — `ICorrelationContext`, `ITenantContext`,
  `IUserContext` are first-class parameters in every behavior; no `AsyncLocal`
  hacks.
- **Explicit pipeline ordering** — `AddBehavior` calls are explicit and
  ordered; no surprise registration-order issues.
- **Testable in isolation** — each behavior is a plain class; tests inject
  a mock `next` delegate without any MediatR test infrastructure.
- **Minimal allocation at dispatch time** — handler lookup is a dictionary
  read at startup; no reflection at runtime once the DI container is built.

### Negative / Trade-offs

- **No ecosystem** — MediatR has third-party behavior packages
  (e.g., `MediatR.Extensions.FluentValidation`). These must be written in-house.
- **Maintenance** — the team owns the pipeline code; bugs are the team's
  responsibility.
- **Onboarding** — developers who know MediatR well need a brief orientation.
  The API is intentionally similar (`ICommandHandler` vs `IRequestHandler`).

---

## Alternatives Considered

| Alternative | Reason rejected |
|-------------|-----------------|
| MediatR | Third-party coupling; v12 breaking changes; no native `Result<T>` or platform context integration |
| Wolverine | Rich feature set but heavyweight; designed for full message-passing systems; overkill for a dispatch layer |
| Brighter | Opinionated persistence model (Command Store); excess complexity for the platform's scope |
| No pipeline / direct handler calls | Loses cross-cutting behaviors (tracing, validation, idempotency) without bespoke wiring per handler |

---

## References

- [MediatR v12 Breaking Changes](https://github.com/jbogard/MediatR/releases/tag/v12.0.0)
- [CQRS without MediatR](https://www.youtube.com/watch?v=r8l3JCjD_BQ) — Milan Jovanović
- [Railway Oriented Programming](https://fsharpforfunandprofit.com/rop/) — Scott Wlaschin
- ADR-001 — `Result<T>` return type used by all handlers
- `src/core/MarcusPrado.Platform.Application/` — CQRS pipeline implementation
- `src/core/MarcusPrado.Platform.Application/Behaviors/` — all shipped behaviors
- `tests/unit/MarcusPrado.Platform.Application.Tests/Pipeline/` — behavior unit tests
