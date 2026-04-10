# ADR-009 — Custom Roslyn Analyzers Over Code Review Conventions

| Field | Value |
|-------|-------|
| **Status** | Accepted |
| **Date** | 2026-03-10 |
| **Deciders** | Marcus Prado Silva |

---

## Context

Architectural rules like "domain projects must not reference infrastructure" and "all application methods must return `Result<T>`" are typically enforced through:

1. **Code review checklists** — low friction to add, but rely on reviewer attention and create friction in PR turnaround.
2. **ArchUnit / NetArchTest tests** — run in CI; catch violations at test time. Discovery lag is a build cycle.
3. **Roslyn analyzers** — violations appear as compile-time errors or warnings directly in the editor, before the developer even saves the file.

The platform already uses `NetArchTest` for layering rules (architecture test project). Roslyn analyzers complement rather than replace it: ArchTests validate macro-level layering; analyzers validate micro-level API usage patterns that ArchTests cannot express concisely.

---

## Decision

Ship five custom Roslyn analyzers in `MarcusPrado.Platform.Analyzers` (`netstandard2.0` target):

| Code | Name | What it catches |
|------|------|----------------|
| `PLATFORM001` | `NoEfCoreInDomain` | `Microsoft.EntityFrameworkCore` referenced in a `*.Domain` project |
| `PLATFORM002` | `NoAspNetInDomain` | `Microsoft.AspNetCore.*` referenced in a `*.Domain` or `*.Application` project |
| `PLATFORM003` | `DomainNoInfraReference` | Any `*.Infrastructure` or `*.Persistence` namespace used in `*.Domain` |
| `PLATFORM004` | `EnforceResultType` | Public `ICommandHandler` methods not returning `Result<T>` or `Task<Result<T>>` |
| `PLATFORM005` | `EnforceIdempotencyKey` | `ICommand` records without an `[IdempotencyKey]` property when `IdempotencyBehavior` is registered |

`PLATFORM004` ships with an `AddResultWrapperCodeFix` that wraps the return type automatically.

All analyzers are `DiagnosticSeverity.Error` — violations prevent compilation rather than producing suppressible warnings.

---

## Consequences

**Positive:**
- Architectural violations are caught at edit time, before code reaches CI.
- New team members (or future contributors) cannot accidentally violate layering rules without an explicit suppression.
- `AddResultWrapperCodeFix` reduces the mechanical cost of adopting `Result<T>`.

**Negative:**
- Analyzers must target `netstandard2.0`, which limits access to modern C# APIs in analyzer code.
- False positives (e.g., a legitimate domain class that coincidentally touches an EF type for testing purposes) must be suppressed with `#pragma warning disable` — this makes suppressions visible and intentional.

**Neutral:**
- Analyzer tests use `Microsoft.CodeAnalysis.Testing` framework with both positive (violation) and negative (no violation) test cases per rule.
