# ADR-003 — EF Core belongs in Extensions, never in Core

> **Summary**: `Microsoft.EntityFrameworkCore` may only be referenced by the
> `MarcusPrado.Platform.EfCore` extension project. Core packages (`Domain`,
> `Application`, `Persistence`) expose pure interfaces; EF Core implements them.
> This boundary is enforced by a Roslyn analyzer and architecture tests.

| Field | Value |
|-------|-------|
| **Status** | Accepted |
| **Date** | 2026-03-01 |
| **Author** | Marcus Prado Silva (Platform Architect) |
| **Tags** | persistence, domain, efcore, clean-architecture, dependency-hygiene |
| **Supersedes** | — |
| **Superseded by** | — |

---

## Context

Entity Framework Core is a powerful ORM, but it is fundamentally an
infrastructure concern: it depends on a specific database driver, brings in
substantial transitive dependencies (`Microsoft.EntityFrameworkCore.*`,
`Microsoft.Data.*`), and ties the persistence model to EF conventions
(navigation properties, shadow properties, change tracking, `[Key]`
attributes, etc.).

Core projects (`Domain`, `Application`, `Abstractions`) must be usable by any
service regardless of its persistence technology. If Core references EF Core:

- Teams adopting Core **also pull in EF Core** even if they use Dapper, MongoDB,
  or a gRPC data source.
- **Domain unit tests** require EF Core test doubles (in-memory provider or
  mocked `DbContext`), slowing the test suite and adding implicit coupling.
- **Upgrading EF Core** — a historically disruptive operation (EF Core 5 → 6
  had breaking changes in owned entities, EF Core 7 introduced JSON columns,
  EF Core 8 changed complex types) — forces a coordinated upgrade of all Core
  packages across every consuming team simultaneously.
- **Domain entities** grow EF-specific annotations (`[Key]`, `[Column]`,
  `[MaxLength]`) that leak infrastructure knowledge into the domain model.

---

## Decision

`Microsoft.EntityFrameworkCore.*` may only be referenced by
`MarcusPrado.Platform.EfCore` (an Extension project). Core projects are
prohibited from any direct or transitive reference to EF Core packages.

### Boundary definition

```
Core layer — interfaces only, no EF types
────────────────────────────────────────────────────────────────────────────
  MarcusPrado.Platform.Persistence
    └─ IRepository<TEntity, TId>       Read/write aggregate root access
    └─ IReadRepository<TEntity, TId>   Read-only projection access
    └─ IUnitOfWork                     Transaction boundary
    └─ ITransaction                    Explicit transaction scope
    └─ IMigrationRunner                Schema migration trigger (no EF ref)

  MarcusPrado.Platform.OutboxInbox
    └─ IOutboxStore                    Write outbox message
    └─ IInboxStore                     Check inbox for duplicates

  MarcusPrado.Platform.AuditLog
    └─ IAuditWriter                    Write audit record

────────────────────────────────────────────────────────────────────────────

Extensions/EfCore — EF Core implementations, only project with EF reference
────────────────────────────────────────────────────────────────────────────
  MarcusPrado.Platform.EfCore
    └─ AppDbContextBase                SaveChanges + audit hooks + outbox dispatch
    └─ TenantDbContextDecorator        Global QueryFilter by TenantId
    └─ EfRepository<TEntity, TId>      Implements IRepository<TEntity, TId>
    └─ EfReadRepository<TEntity, TId>  Implements IReadRepository<TEntity, TId>
    └─ EfUnitOfWork                    Implements IUnitOfWork + ITransaction
    └─ EfOutboxStore                   Implements IOutboxStore
    └─ EfInboxStore                    Implements IInboxStore
    └─ EfMigrationRunner               Implements IMigrationRunner
    └─ EntityTypeConfigurationBase<T>  Fluent mapping base (no data annotations)
```

### Domain entities remain pure C#

```csharp
// Domain entity — no EF attributes, no navigation property conventions
public sealed class Order : AggregateRoot<OrderId>
{
    public CustomerId CustomerId { get; private set; }
    public Money Total { get; private set; }
    private readonly List<OrderLine> _lines = [];

    // EF Core discovers this via IEntityTypeConfiguration, not attributes
    public IReadOnlyList<OrderLine> Lines => _lines.AsReadOnly();

    private Order() { }   // required by EF Core for materialisation

    public static Result<Order> Create(CustomerId customerId, IEnumerable<OrderLineDto> lines)
    {
        if (!lines.Any())
            return Error.Validation("order.empty_lines", "Order must have at least one line.");

        var order = new Order { CustomerId = customerId };
        // ... domain logic
        return order;
    }
}

// Mapping lives entirely in the EfCore extension
internal sealed class OrderConfiguration : EntityTypeConfigurationBase<Order, OrderId>
{
    protected override void ConfigureEntity(EntityTypeBuilder<Order> builder)
    {
        builder.Property(o => o.Total)
               .HasConversion<MoneyConverter>()
               .HasColumnName("total_amount");

        builder.OwnsMany<OrderLine>(o => o.Lines, lines =>
        {
            lines.WithOwner();
            lines.ToTable("order_lines");
        });
    }
}
```

### Enforcement (three layers)

```csharp
// 1. Roslyn analyzer — fires at build time (MP0001)
// src/tooling/MarcusPrado.Platform.Analyzers/Rules/NoEfCoreInDomainAnalyzer.cs
//
// Diagnostic: MP0001  Warning  EF Core reference in Domain/Application project.
// The EF Core package 'Microsoft.EntityFrameworkCore' must only be referenced
// from 'MarcusPrado.Platform.EfCore'.

// 2. Architecture test — runs in CI test stage
// src/tooling/MarcusPrado.Platform.ArchTests/Rules/LayeringRules.cs
[Fact]
public void DomainMustNotReferenceEfCore()
{
    Types.InAssembly(typeof(Order).Assembly)
         .ShouldNot()
         .HaveDependencyOn("Microsoft.EntityFrameworkCore")
         .Because("Domain must be persistence-ignorant (ADR-003)")
         .Check();
}

// 3. Directory.Build.props — compile-time guard for direct PackageReference
// (StyleCop Analyzers also flags DA0001 if the package is added accidentally)
```

### Migration ownership

Migrations live **inside the consuming service**, not in the platform:

```
my-service/
  src/
    MyService.Infrastructure/
      Migrations/       ← generated by `dotnet ef migrations add`
      MyServiceDbContext.cs  ← inherits AppDbContextBase
```

The platform's `EfMigrationRunner` (implementing `IMigrationRunner`) executes
`Database.MigrateAsync()` at application startup, triggered by
`IHostedService`. The Core `IMigrationRunner` interface is called in
`Application`; the EF implementation is registered by `AddPlatformEfCore<T>()`.

---

## Consequences

### Positive

- **Clean domain model** — `Entity<TId>` and `AggregateRoot<TId>` have no EF
  annotations; they are pure C# value types.
- **Fast unit tests** — domain and application logic can be tested without any
  database or EF dependency; an in-memory `FakeRepository<T>` suffices.
- **ORM flexibility** — services can replace EF Core with Dapper
  (`MarcusPrado.Platform.DataAccess`), MongoDB
  (`MarcusPrado.Platform.MongoDb`), or an in-memory stub by swapping
  extension registrations. Core code is unchanged.
- **EF Core upgrade isolation** — EF Core version bumps are absorbed in the
  `EfCore` extension and tested there; Core packages are not touched.
- **Minimal Core NuGet graph** — `Domain` and `Application` packages carry
  only `Abstractions` as a dependency, keeping consumer graphs clean.

### Negative / Trade-offs

- **Explicit mapping layer** — teams must write `IEntityTypeConfiguration<T>`
  for every entity. This is extra code, but it is the standard DDD-with-EF
  practice and keeps mapping concerns visible.
- **No lazy loading** — EF's lazy-loading proxies require virtual navigation
  properties; domain entities don't expose those. Explicit `Include()` or
  projection queries are required. This is considered a feature (explicit over
  implicit), not a limitation.
- **`private Order()` constructors** — EF Core requires a parameterless
  constructor (can be private) for materialisation. Teams unfamiliar with this
  pattern are sometimes confused on first encounter.

---

## Alternatives Considered

| Alternative | Reason rejected |
|-------------|-----------------|
| Allow EF Core attributes on domain entities (`[Key]`, `[Column]`) | Pollutes domain with infrastructure concerns; prevents swapping the ORM without modifying domain classes |
| Reference EF Core in `Persistence` abstractions | Same problem — any consumer of `Persistence` must pull in EF Core transitively |
| Separate persistence entities from domain entities via AutoMapper | Doubles the entity count; mapping layer maintenance cost exceeds the benefit for most services |
| EF Core `IEntityTypeConfiguration<T>` without any attribute | This **is** the adopted approach — fluent configuration lives in the EfCore extension exactly as described |

---

## References

- [Domain-Driven Design](https://www.domainlanguage.com/ddd/) — Eric Evans
- [Persistence Ignorance](https://deviq.com/principles/persistence-ignorance) — DevIQ
- [EF Core — Owned Entity Types](https://learn.microsoft.com/en-us/ef/core/modeling/owned-entities)
- [EF Core Fluent API Configuration](https://learn.microsoft.com/en-us/ef/core/modeling/)
- ADR-002 — Application layer calls `IRepository<T>` through the CQRS pipeline
- `src/core/MarcusPrado.Platform.Persistence/` — repository and UoW abstractions
- `src/extensions/MarcusPrado.Platform.EfCore/` — EF Core implementation
- `src/tooling/MarcusPrado.Platform.Analyzers/Rules/NoEfCoreInDomainAnalyzer.cs` — MP0001
- `src/tooling/MarcusPrado.Platform.ArchTests/Rules/LayeringRules.cs` — `DomainMustNotReferenceEfCore`
