# ADR-014 — Explicit Degradation States Over Generic Feature Flags

| Field | Value |
|-------|-------|
| **Status** | Accepted |
| **Date** | 2026-03-10 |
| **Deciders** | Marcus Prado Silva |

---

## Context

Production services occasionally need to operate in a reduced capacity: during a database maintenance window, when a downstream dependency is unhealthy, or during a high-load event where non-critical features should be shed. Two approaches were considered:

1. **Feature flags** — generic boolean toggles. Any feature can be disabled independently. Flexible but requires operators to know which combination of flags achieves a safe degraded state.
2. **Named degradation modes** — a finite set of named states, each with a documented meaning and a pre-defined set of enforcement rules.

Feature flags scale well for product features (A/B testing, gradual rollout) but are a poor fit for operational states. A "read-only" operational mode is not a product feature — it has a specific, well-understood meaning to SREs.

---

## Decision

Use **four named degradation modes** via `DegradationMode`:

| Mode | Meaning | HTTP enforcement |
|------|---------|-----------------|
| `None` | Normal operation | No restrictions |
| `PartiallyDegraded` | Non-critical features disabled; core path continues | Custom middleware checks per-endpoint `[AllowInDegradedMode]` attribute |
| `ReadOnly` | All write operations rejected | `DegradationMiddleware` returns 405 for non-GET/HEAD requests |
| `Maintenance` | Service fully offline | `DegradationMiddleware` returns 503 for all requests |

`IDegradationController` stores the current mode (in-memory default; Redis for distributed deployments). `DegradationMiddleware` reads the mode on every request and enforces accordingly.

Management endpoints (`GET /degradation/status`, `POST /degradation/mode`) allow operators to switch modes at runtime without a deployment.

---

## Consequences

**Positive:**
- Operators have a clear vocabulary — "put the service in ReadOnly mode" is unambiguous.
- Middleware enforcement is automatic — no risk of forgetting to check a flag in a write path.
- OTel metric `platform.degradation.mode` allows alerting on unexpected mode transitions.

**Negative:**
- The four modes may not cover every scenario. Extension points are provided: services can implement `IDegradationStrategy` for custom enforcement logic per mode.
- `PartiallyDegraded` requires endpoint-level annotation (`[AllowInDegradedMode]`) which must be maintained as the API grows.

**Neutral:**
- This is orthogonal to feature flags — `MarcusPrado.Platform.FeatureFlags` handles product features; `MarcusPrado.Platform.Degradation` handles operational states. Both can be used simultaneously.
