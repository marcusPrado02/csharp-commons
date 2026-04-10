# ADR-008 — Multi-Tenancy via Ambient ITenantContext (Not Per-Repository)

| Field | Value |
|-------|-------|
| **Status** | Accepted |
| **Date** | 2026-03-01 |
| **Deciders** | Marcus Prado Silva |

---

## Context

Multi-tenant systems need tenant isolation at the persistence layer. There are two common designs:

1. **Per-repository tenant parameter** — every repository method receives `TenantId` as an explicit argument. Isolation is explicit and traceable.
2. **Ambient context** — `ITenantContext` is resolved from DI (set by middleware at the start of each request). EF Core global query filters read from it automatically; application code never mentions `TenantId` in domain or application layers.

Option 1 leaks infrastructure concerns into the domain: `OrderRepository.GetByIdAsync(id, tenantId)` pollutes the domain layer with a persistence concept. It also requires every new query to remember to filter by tenant, making it easy to introduce data leakage bugs.

Option 2 risks "invisible magic" where isolation filters are silently disabled. The risk is mitigated by the architectural enforcer (PLATFORM003 analyzer) and mandatory integration tests.

---

## Decision

Use **ambient `ITenantContext`** resolved from scoped DI.

`TenantResolutionMiddleware` extracts `TenantId` from JWT claims, a `X-Tenant-ID` header, or a subdomain strategy (configurable per service). It sets `ITenantContext.Current` for the duration of the request scope. `AppDbContextBase` applies a global EF Core query filter `WHERE tenant_id = @current` on all `ITenantScoped` entities. Dapper queries must include the filter explicitly (verified by integration test suite).

---

## Consequences

**Positive:**
- Domain and application layers are completely unaware of tenancy — `Order.Create(customerId, lines)` has no tenant parameter.
- Adding tenant isolation to a new entity requires only implementing `ITenantScoped` — no repository changes needed.
- `FakeTenantContext` in TestKit sets up test isolation with a single line.

**Negative:**
- Background jobs running outside an HTTP request must manually set `ITenantContext`. A `TenantJobContext` helper is provided for this purpose.
- Developers must remember that EF Core global filters can be disabled with `IgnoreQueryFilters()` — a code review convention and PLATFORM003 analyzer flag this.

**Neutral:**
- Schema-per-tenant and database-per-tenant isolation strategies are also supported via `ITenantIsolationStrategy` but are not the default; discriminator column is the default for most services.
