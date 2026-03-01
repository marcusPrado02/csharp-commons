# ADR-001 — Use `Result<T>` for expected failure paths

| Field | Value |
|-------|-------|
| **Status** | Accepted |
| **Date** | 2026-03-01 |
| **Deciders** | Platform team |
| **Technical story** | Platform Commons item #2 |

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

---

## Decision

Implement a `Result<T>` / `Result` discriminated union in
`MarcusPrado.Platform.Abstractions` as the canonical return type for all
operations where failure is a normal, expected outcome.

### Implementation

```csharp
// Non-generic (fire-and-forget operations)
public readonly struct Result : IEquatable<Result> { ... }

// Generic (operations that return a value on success)
public readonly struct Result<T> : IEquatable<Result<T>> { ... }

// Error descriptor (no heap allocations for stack traces)
public sealed record Error(string Code, string Message,
                           ErrorCategory Category, ErrorSeverity Severity);
```

Key design choices:

| Choice | Rationale |
|--------|-----------|
| `readonly struct` | Zero heap allocation on the success path |
| Implicit conversions `T → Result<T>` and `Error → Result<T>` | Reduces boilerplate at call sites |
| `Map`, `Bind`, `Match`, `OnSuccess`, `OnFailure` extension methods | Railway-oriented, composable pipelines |
| Async variants in `ResultAsyncExtensions` | First-class async support without breaking the chain |
| `ErrorCategory` enum | Allows middleware (e.g. `ExceptionMiddleware`) to map to HTTP status codes without pattern-matching on exception types |
| `ErrorSeverity` enum | Enables structured log level selection centrally |

---

## Consequences

### Positive

- **Explicit failure contracts** — callers see from the return type that an
  operation can fail; the compiler enforces handling.
- **Zero allocation on success path** — `readonly struct` avoids GC pressure
  in high-throughput scenarios.
- **Uniform error shape** — `ErrorCategory` maps cleanly to HTTP status codes
  in `ExceptionMiddleware` / `ProblemDetailsFactory`.
- **Composable pipelines** — `Bind`/`Map`/`Match` enable Railway-Oriented
  Programming without nesting.
- **Testability** — unit tests check return values, not thrown exceptions.

### Negative / Trade-offs

- **Caller discipline** — developers must check `IsFailure` or use `Match`;
  ignoring the result is possible (but flagged by Roslyn analyzer
  `EnforceResultTypeAnalyzer` in `MarcusPrado.Platform.Analyzers`).
- **Interop with third-party libraries** — libraries that throw must be wrapped
  at the boundary; adapter code is boilerplate.
- **Learning curve** — teams unfamiliar with functional patterns need onboarding.

---

## Alternatives Considered

| Alternative | Reason rejected |
|-------------|-----------------|
| Exceptions only | High allocation cost; control flow is implicit; forces wide-catch at API layer |
| `OneOf<TSuccess, TFailure>` (third-party) | External dependency; less control; doesn't integrate with platform `Error` model |
| `FluentResults` (third-party) | External dependency; opinionated API that might conflict with platform conventions |
| Nullable return + `out Error` | Doesn't compose; unpleasant async ergonomics; not idiomatic C# 10+ |

---

## References

- [Railway Oriented Programming](https://fsharpforfunandprofit.com/rop/) — Scott Wlaschin
- [Functional Error Handling in C#](https://enterprisecraftsmanship.com/posts/functional-c-handling-failures-input-errors/) — Vladimir Khorikov
- [CA1822 / CA1031](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/quality-rules/) — related Roslyn rules
- `src/core/MarcusPrado.Platform.Abstractions/Results/` — implementation
- `tests/unit/MarcusPrado.Platform.Abstractions.Tests/Results/` — 92 unit tests
