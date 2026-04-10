# ADR-001 — Use `Result<T>` for expected failure paths

> **Summary**: Instead of throwing exceptions for predictable outcomes (validation
> failure, record not found, business rule violation), all platform operations
> return a `Result<T>` discriminated union that forces callers to handle both
> success and failure paths at compile time.

| Field | Value |
|-------|-------|
| **Status** | Accepted |
| **Date** | 2026-03-01 |
| **Author** | Marcus Prado Silva (Platform Architect) |
| **Tags** | error-handling, domain, application, performance |
| **Supersedes** | — |
| **Superseded by** | — |

---

## Context

Every service needs a uniform way to communicate whether an operation succeeded
or failed. Two common approaches exist in .NET:

1. **Exceptions** — throw on any failure (expected or unexpected).
2. **Result type** — return a discriminated union that callers must explicitly
   handle.

Platform libraries that rely solely on exceptions for expected failures (e.g.,
"record not found", "validation failed") force consumer code into wide-catch
blocks, obscure control flow, and incur significant heap allocation and
stack-unwinding costs on hot paths.

Concrete problems observed in pre-platform services:

- A `ProductService.GetByIdAsync` returned `null` on not-found; callers
  silently swallowed `NullReferenceException` downstream.
- `ValidationException` caught at the API controller level masked business
  rule violations that should have been surfaced differently to consumers.
- Load testing showed >8 % CPU overhead in exception-heavy paths (throw + catch
  on every validation miss in a bulk-import endpoint processing 50 k records).

---

## Decision

Implement a `Result<T>` / `Result` discriminated union in
`MarcusPrado.Platform.Abstractions` as the canonical return type for all
operations where failure is a normal, expected outcome. Exceptions remain
reserved for **unexpected** failures (infrastructure unavailability, bugs,
programming errors).

### Public API surface

```csharp
// Non-generic — fire-and-forget operations (commands with no return value)
public readonly struct Result : IEquatable<Result>
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error Error { get; }

    public static Result Success() => new(true, Error.None);
    public static Result Failure(Error error) => new(false, error);
    public static implicit operator Result(Error error) => Failure(error);
}

// Generic — operations that return a value on success
public readonly struct Result<T> : IEquatable<Result<T>>
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public T Value { get; }          // throws InvalidOperationException if IsFailure
    public Error Error { get; }

    public static Result<T> Success(T value) => new(true, value, Error.None);
    public static Result<T> Failure(Error error) => new(false, default!, error);
    public static implicit operator Result<T>(T value) => Success(value);
    public static implicit operator Result<T>(Error error) => Failure(error);
}

// Error descriptor — no heap allocation for stack traces on expected failures
public sealed record Error(
    string Code,
    string Message,
    ErrorCategory Category,
    ErrorSeverity Severity = ErrorSeverity.Warning)
{
    public static readonly Error None = new(string.Empty, string.Empty, ErrorCategory.None);
}

public enum ErrorCategory
{
    None,
    Validation,      // → HTTP 422
    NotFound,        // → HTTP 404
    Conflict,        // → HTTP 409
    Unauthorized,    // → HTTP 401
    Forbidden,       // → HTTP 403
    Unavailable,     // → HTTP 503
    Internal,        // → HTTP 500
}
```

### Railway-oriented extension methods

```csharp
// Functional composition without nesting
public static class ResultExtensions
{
    // Transform the value if success
    public static Result<TOut> Map<TIn, TOut>(this Result<TIn> result, Func<TIn, TOut> mapper);

    // Chain operations that themselves return Result
    public static Result<TOut> Bind<TIn, TOut>(this Result<TIn> result, Func<TIn, Result<TOut>> binder);

    // Handle both paths — terminal operation
    public static TOut Match<TIn, TOut>(this Result<TIn> result,
        Func<TIn, TOut> onSuccess, Func<Error, TOut> onFailure);

    // Side-effects without breaking the chain
    public static Result<T> OnSuccess<T>(this Result<T> result, Action<T> action);
    public static Result<T> OnFailure<T>(this Result<T> result, Action<Error> action);

    // Async variants (Task<Result<T>>) for every method above
}
```

### Typical handler usage

```csharp
public sealed class GetOrderHandler : IQueryHandler<GetOrderQuery, OrderDto>
{
    public async Task<Result<OrderDto>> HandleAsync(GetOrderQuery query, CancellationToken ct)
    {
        var order = await _repository.FindAsync(query.OrderId, ct);
        if (order is null)
            return Error.NotFound("order.not_found", $"Order {query.OrderId} does not exist.");

        return _mapper.Map<OrderDto>(order);   // implicit T → Result<T> conversion
    }
}
```

### Typical controller / Minimal API usage

```csharp
app.MapGet("/orders/{id}", async (Guid id, IQueryBus bus, CancellationToken ct) =>
{
    var result = await bus.QueryAsync(new GetOrderQuery(id), ct);

    return result.Match(
        onSuccess: dto  => Results.Ok(dto),
        onFailure: error => error.Category switch
        {
            ErrorCategory.NotFound  => Results.NotFound(),
            ErrorCategory.Forbidden => Results.Forbid(),
            _                       => Results.Problem(error.Message)
        });
});
```

### `ErrorCategory` → HTTP status code mapping

| `ErrorCategory` | HTTP Status | Notes |
|----------------|-------------|-------|
| `Validation` | 422 Unprocessable Entity | Semantic validation failure |
| `NotFound` | 404 Not Found | Resource does not exist |
| `Conflict` | 409 Conflict | Duplicate, optimistic concurrency |
| `Unauthorized` | 401 Unauthorized | Missing or invalid credentials |
| `Forbidden` | 403 Forbidden | Insufficient permissions |
| `Unavailable` | 503 Service Unavailable | Downstream dependency down |
| `Internal` | 500 Internal Server Error | Unexpected error |

Mapping is centralised in `ProblemDetailsFactory` (AspNetCore extension) so the API
layer never contains a `switch` over error categories.

### Key design choices

| Choice | Rationale |
|--------|-----------|
| `readonly struct` | Zero heap allocation on the success path |
| Implicit conversions `T → Result<T>` and `Error → Result<T>` | Reduces boilerplate at call sites |
| `Map`, `Bind`, `Match` extension methods | Railway-oriented, composable pipelines |
| Async variants in `ResultAsyncExtensions` | First-class `Task<Result<T>>` support without breaking the chain |
| `ErrorCategory` enum | Centralises HTTP mapping; middleware never pattern-matches on exception types |
| `ErrorSeverity` enum | Enables structured log level selection in `LoggingBehavior` |
| Roslyn analyzer `MP0004` | Warns when `Result<T>` is discarded without checking `IsSuccess` |

---

## Consequences

### Positive

- **Explicit failure contracts** — callers see from the return type that an
  operation can fail; the compiler enforces handling.
- **Zero allocation on success path** — `readonly struct` avoids GC pressure
  in high-throughput scenarios. Benchmarks show ~40 % fewer Gen0 collections
  on paths that previously threw `ValidationException` on every miss.
- **Uniform error shape** — `ErrorCategory` maps cleanly to HTTP status codes
  in `ProblemDetailsFactory`; no per-endpoint error translation.
- **Composable pipelines** — `Bind`/`Map`/`Match` enable Railway-Oriented
  Programming without nesting.
- **Testability** — unit tests assert on return values, not thrown exceptions;
  no `Assert.Throws` for expected failure cases.

### Negative / Trade-offs

- **Caller discipline** — developers must check `IsFailure` or use `Match`;
  ignoring the result is possible (though `MP0004` warns at build time).
- **Interop with third-party libraries** — libraries that throw must be wrapped
  at the boundary; this adapter code is unavoidable boilerplate.
- **Learning curve** — teams unfamiliar with Railway-Oriented Programming need
  a ~30-minute onboarding session. The `Match` API reads well once learned.
- **No stack trace on expected failures** — intentional, but makes debugging
  "where did this `NotFound` come from?" harder without structured logging.

---

## Alternatives Considered

| Alternative | Reason rejected |
|-------------|-----------------|
| Exceptions only | High allocation cost on error paths; implicit control flow; forces wide-catch at API layer |
| `OneOf<TSuccess, TFailure>` (third-party) | External dependency; doesn't integrate with platform `Error` model or `ErrorCategory` mapping |
| `FluentResults` (third-party) | External dependency; opinionated API that would conflict with platform conventions |
| Nullable return + `out Error` | Non-composable; unpleasant async ergonomics; not idiomatic C# |
| `ValueTask<Result<T>>` everywhere | Premature optimisation; adds complexity; `Task<Result<T>>` is sufficient for I/O-bound service code |

---

## References

- [Railway Oriented Programming](https://fsharpforfunandprofit.com/rop/) — Scott Wlaschin
- [Functional Error Handling in C#](https://enterprisecraftsmanship.com/posts/functional-c-handling-failures-input-errors/) — Vladimir Khorikov
- [CA1031 — Do not catch general exception types](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/quality-rules/ca1031)
- `src/core/MarcusPrado.Platform.Abstractions/Results/` — implementation
- `src/core/MarcusPrado.Platform.Abstractions/Errors/` — `Error`, `ErrorCategory`, `ErrorSeverity`
- `src/extensions/MarcusPrado.Platform.AspNetCore.ProblemDetails/` — HTTP mapping
- `src/tooling/MarcusPrado.Platform.Analyzers/Rules/EnforceResultTypeAnalyzer.cs` — `MP0004`
- `tests/unit/MarcusPrado.Platform.Abstractions.Tests/Results/` — unit tests
