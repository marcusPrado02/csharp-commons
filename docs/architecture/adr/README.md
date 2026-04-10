# Architecture Decision Records

This directory contains the Architecture Decision Records (ADRs) for
**MarcusPrado Platform Commons**. Each ADR documents a significant technical
decision: the context that motivated it, the decision made, its consequences,
and the alternatives that were considered and rejected.

> **Format**: Each ADR follows the [MADR template](https://adr.github.io/madr/)
> (Markdown Architectural Decision Records) with additions for `Author`,
> `Tags`, and a one-line `Summary`.

---

## Index

| ADR | Status | Title | Tags |
|-----|--------|-------|------|
| [ADR-001](ADR-001-result-type.md) | Accepted | Use `Result<T>` for expected failure paths | `error-handling` `domain` `performance` |
| [ADR-002](ADR-002-no-mediatr.md) | Accepted | Own CQRS pipeline instead of MediatR | `cqrs` `application-layer` `dependencies` |
| [ADR-003](ADR-003-efcore-in-extension.md) | Accepted | EF Core belongs in Extensions, never in Core | `persistence` `clean-architecture` `efcore` |
| [ADR-004](ADR-004-otel-semantic-conventions.md) | Accepted | OpenTelemetry Semantic Conventions for all telemetry | `observability` `opentelemetry` `standards` |
| [ADR-005](ADR-005-versioning-strategy.md) | Accepted | Versioning strategy: MinVer + Central Package Management | `versioning` `build` `nuget` `ci-cd` |
| [ADR-006](ADR-006-testing-strategy.md) | Accepted | Testing strategy: layered test pyramid | `testing` `quality` `ci-cd` `tdd` |

---

## Statuses

| Status | Meaning |
|--------|---------|
| **Draft** | Being written; not yet reviewed |
| **Proposed** | Written and under review |
| **Accepted** | Adopted; active decision |
| **Deprecated** | No longer applies; not superseded |
| **Superseded** | Replaced by a newer ADR |

---

## How to write a new ADR

1. Copy the template below into a new file named `ADR-NNN-short-title.md`.
2. Fill in all fields; leave none as "TODO".
3. Add an entry to the index table in this file.
4. Reference the ADR from any code or documentation it governs.

### ADR template

```markdown
# ADR-NNN — Title

> **Summary**: One sentence describing the decision and its primary benefit.

| Field | Value |
|-------|-------|
| **Status** | Draft |
| **Date** | YYYY-MM-DD |
| **Author** | Your Name (Role) |
| **Tags** | tag1, tag2 |
| **Supersedes** | — |
| **Superseded by** | — |

---

## Context

What problem or situation prompted this decision?
Include constraints, observations, or prior incidents.

---

## Decision

What was decided, and how is it implemented?
Include code examples, diagrams, or configuration snippets.

---

## Consequences

### Positive
- ...

### Negative / Trade-offs
- ...

---

## Alternatives Considered

| Alternative | Reason rejected |
|-------------|-----------------|
| ... | ... |

---

## References

- ...
```

---

## Relationship to other documentation

- **[overview.md](../overview.md)** — High-level architecture diagram and
  solution structure. Start here if you are new to the codebase.
- **[layer-rules.md](../layer-rules.md)** — Authoritative dependency rules,
  enforcement matrix, and visualised dependency flow.
- **ADRs** — The *why* behind the rules. When a rule in `layer-rules.md`
  seems arbitrary, the governing ADR explains the reasoning.
