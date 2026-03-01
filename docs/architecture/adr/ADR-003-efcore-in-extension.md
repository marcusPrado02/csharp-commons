# ADR-003 — EF Core belongs in Extensions, never in Core

| Field | Value |
|-------|-------|
| **Status** | Accepted |
| **Date** | 2026-03-01 |
| **Deciders** | Platform team |
| **Technical story** | Platform Commons items #12 (EfCore), #8 (ArchTests) |

---

## Context

Entity Framework Core is a powerful ORM, but it is fundamentally an
infrastructure concern: it depends on a specific database driver, brings in
substantial transitive dependencies (`Microsoft.EntityFrameworkCore.*`), and
ties the persistence model to EF conventions (navigation properties, change
tracking, etc.).

Core projects (`Domain`, `Application`, `Abstractions`) must be usable by any
service regardless of the persistence technology it chooses (EF Core, Dapper,
MongoDB driver, gRPC, etc.). If Core references EF Core:

- Teams adopting Core also pull in EF Core even if they use a different ORM.
- Unit tests of domain and application logic require EF Core test doubles.
- Upgrading EF Core (a major breaking-change source) forces a coordinated
  upgrade of all Core packages.

---

## Decision

EF Core may only be referenced by `MarcusPrado.Platform.EfCore` (an Extension
project). Core projects are strictly prohibited from referencing any
`Microsoft.EntityFrameworkCore.*` package.

### Boundary

```
Core (Domain / Application / Persistence abstractions)
  └─ IRepository<T>           ← interface only; no EF types
  └─ IUnitOfWork              ← interface only
  └─ ITransaction             ← interface only
  └─ IAuditWriter             ← interface only

Extensions/EfCore
  └─ AppDbContextBase         ← implements SaveChanges + audit + outbox
  └─ TenantDbContextDecorator ← adds global QueryFilter by TenantId
  └─ EfUnitOfWork             ← implements IUnitOfWork + ITransaction
  └─ EfOutboxStore            ← implements IOutboxStore
  └─ EfInboxStore             ← implements IInboxStore
  └─ EfMigrationRunner        ← implements IMigrationRunner
  └─ EfRepository<T>          ← implements IRepository<T>
```

### Enforcement

The rule is encoded in `MarcusPrado.Platform.ArchTests` as
`LayeringRules.DomainMustNotReferenceEfCore()` using NetArchTest:

```csharp
Types.InAssembly(domainAssembly)
     .ShouldNot()
     .HaveDependencyOn("Microsoft.EntityFrameworkCore")
     .Check();
```

And as a Roslyn analyzer in `MarcusPrado.Platform.Analyzers`:
`NoEfCoreInDomainAnalyzer` — diagnostic `MP0001`.

---

## Consequences

### Positive

- **Clean domain model** — `Entity<TId>` and `AggregateRoot<TId>` have no EF
  annotations; they are pure C# classes.
- **Testability** — domain and application logic can be unit-tested without
  any database or EF dependency.
- **Flexibility** — services can replace EF Core with Dapper, MongoDB, or an
  in-memory store by swapping the `EfCore` extension for another adapter.
- **Dependency hygiene** — NuGet graph of Core packages stays minimal.

### Negative / Trade-offs

- **Mapping layer** — teams must map between domain entities and EF persistence
  models (or use owned entity types). This is extra code but is the standard
  DDD practice.
- **Navigation properties** — EF's lazy-loading via navigation properties does
  not work on `Entity<TId>` without EF attributes; explicit loading must be
  used.

---

## Alternatives Considered

| Alternative | Reason rejected |
|-------------|-----------------|
| Allow EF Core attributes on domain entities | Pollutes domain with infrastructure concerns; prevents swapping the ORM |
| Separate domain entities from persistence entities via AutoMapper | Doubles the entity count; high ceremony for simple CRUD services |
| Use EF Core's `IEntityTypeConfiguration<T>` without attributes | This is already the adopted approach — fluent configuration in EfCore extension |

---

## References

- [DDD Layered Architecture](https://www.domainlanguage.com/ddd/) — Eric Evans
- [Persistence Ignorance](https://deviq.com/principles/persistence-ignorance) — DevIQ
- `src/core/MarcusPrado.Platform.Persistence/` — persistence abstractions
- `src/extensions/MarcusPrado.Platform.EfCore/` — EF Core implementation
- `src/tooling/MarcusPrado.Platform.ArchTests/Rules/LayeringRules.cs` — enforcement
