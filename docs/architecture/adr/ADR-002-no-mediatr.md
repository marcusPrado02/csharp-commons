# ADR-002 — Own CQRS pipeline instead of MediatR

| Field | Value |
|-------|-------|
| **Status** | Accepted |
| **Date** | 2026-03-01 |
| **Deciders** | Platform team |
| **Technical story** | Platform Commons item #9 (Application layer) |

---

## Context

MediatR is the de-facto standard mediator library for .NET CQRS. However, the
platform library must remain a foundation that teams build on top of — imposing
a specific third-party dispatcher couples all consumer services to MediatR's
release cadence, licensing, and API surface.

MediatR v12 introduced breaking changes (e.g., removed `IRequest<TResponse>`,
unified handler registration) that forced widespread consumer updates. A custom
pipeline eliminates this risk.

Additionally, the platform's pipeline requirements differ from MediatR's defaults:
- Platform behaviors must integrate with `Result<T>` (no exceptions for domain
  failures).
- The pipeline needs first-class `ICorrelationContext` and `ITenantContext`
  propagation.
- `IdempotencyBehavior` must interact with `IIdempotencyStore`, which is a
  platform abstraction unknown to MediatR.

---

## Decision

Implement a first-party CQRS pipeline in `MarcusPrado.Platform.Application`
with no dependency on MediatR or any other third-party dispatcher.

### Core abstractions

```csharp
// Commands (mutation, returns Result)
public interface ICommand { }
public interface ICommand<TResult> : ICommand { }
public interface ICommandHandler<TCommand> where TCommand : ICommand
    { Task<Result> HandleAsync(TCommand command, CancellationToken ct); }
public interface ICommandHandler<TCommand, TResult> where TCommand : ICommand<TResult>
    { Task<Result<TResult>> HandleAsync(TCommand command, CancellationToken ct); }

// Queries (read-only, returns Result<T>)
public interface IQuery<TResult> { }
public interface IQueryHandler<TQuery, TResult> where TQuery : IQuery<TResult>
    { Task<Result<TResult>> HandleAsync(TQuery query, CancellationToken ct); }

// Pipeline
public interface IPipelineBehavior<TRequest, TResponse>
    { Task<TResponse> HandleAsync(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct); }

// Bus (DI entry point)
public interface ICommandBus { Task<Result> SendAsync<TCommand>(TCommand command, CancellationToken ct); }
public interface IQueryBus   { Task<Result<TResult>> QueryAsync<TResult>(IQuery<TResult> query, CancellationToken ct); }
```

### Shipped behaviors (ordered)

| Order | Behavior | Purpose |
|-------|----------|---------|
| 1 | `LoggingBehavior` | Structured request/response logging |
| 2 | `MetricsBehavior` | Command/query duration + success/failure counters |
| 3 | `TracingBehavior` | OpenTelemetry span per command/query |
| 4 | `ValidationBehavior` | Runs `IValidator<T>` → returns `Result.Failure` if invalid |
| 5 | `AuthorizationBehavior` | Checks `IPolicyAuthorizer` → returns 403 if denied |
| 6 | `IdempotencyBehavior` | Checks `IIdempotencyStore`; skips handler on duplicate |
| 7 | `TransactionBehavior` | Wraps handler in `IUnitOfWork.BeginTransactionAsync` |
| 8 | `RetryBehavior` | Retries transient failures via `IResiliencePolicy` |

---

## Consequences

### Positive

- **No external dependency** — the pipeline is part of the platform, upgraded
  on the platform's own schedule.
- **Result<T> native** — behaviors speak the same language as handlers; no
  exception wrapping needed.
- **Platform context propagation** — `ICorrelationContext`, `ITenantContext`,
  `IUserContext` are first-class citizens of every behavior.
- **Testable in isolation** — each behavior is a plain class implementing a
  well-known interface; no MediatR test infrastructure needed.
- **Minimal allocation** — the delegate chain is a linked list of
  `RequestHandlerDelegate<T>` closures; no reflection at dispatch time once
  handlers are registered at startup.

### Negative / Trade-offs

- **No ecosystem** — MediatR has a rich ecosystem of third-party behaviors
  (e.g., FluentValidation integration). These must be written or adapted.
- **Maintenance** — the team owns the pipeline code; bugs are the team's bugs.
- **Onboarding** — developers who know MediatR need a brief orientation.

---

## Alternatives Considered

| Alternative | Reason rejected |
|-------------|-----------------|
| MediatR | Third-party coupling; breaking changes between versions; doesn't natively integrate with `Result<T>` or platform contexts |
| Wolverine | Rich feature set but heavy; designed for full message-passing systems; overkill for the dispatch layer |
| No pipeline / direct handler calls | Loses cross-cutting behaviors (logging, tracing, validation) without bespoke wiring per handler |

---

## References

- [MediatR v12 Breaking Changes](https://github.com/jbogard/MediatR/releases/tag/v12.0.0)
- [CQRS without MediatR](https://www.youtube.com/watch?v=r8l3JCjD_BQ) — Milan Jovanović
- `src/core/MarcusPrado.Platform.Application/CQRS/` — implementation stubs
- ADR-001 — `Result<T>` is the return type for all handlers
