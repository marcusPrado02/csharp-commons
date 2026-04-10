# Core Domain

> `MarcusPrado.Platform.Domain`

Domain-model building blocks: entities with identity-based equality, value objects with structural equality, domain events, aggregate roots with optimistic concurrency, specifications, and business rules. Zero infrastructure dependencies — pure C# with no NuGet runtime requirements.

## Install

```bash
dotnet add package MarcusPrado.Platform.Domain
```

## Entities and Aggregates

```csharp
// Value object for a strongly-typed ID
public sealed class OrderId : EntityId
{
    public OrderId(Guid value) : base(value) { }
    public static OrderId New() => new(Guid.NewGuid());
}

// Aggregate root with domain event recording
public sealed class Order : AggregateRoot<OrderId>
{
    public CustomerId CustomerId { get; private set; }
    public OrderStatus Status { get; private set; }
    private readonly List<LineItem> _lines = [];

    private Order() { } // EF Core

    public static Result<Order> Create(CustomerId customerId, IReadOnlyList<LineItem> lines)
    {
        if (lines.Count == 0)
            return Error.Validation("Order.EmptyLines", "An order must have at least one line.");

        var order = new Order
        {
            Id = OrderId.New(),
            CustomerId = customerId,
            Status = OrderStatus.Pending
        };
        order.AddDomainEvent(new OrderCreatedEvent(order.Id, customerId));
        return order;
    }

    public Result Ship()
    {
        CheckRule(new OrderMustBePaidRule(Status));  // throws BusinessRuleViolationException
        Status = OrderStatus.Shipped;
        AddDomainEvent(new OrderShippedEvent(Id));
        return Result.Success();
    }
}
```

## Business Rules

```csharp
public sealed class OrderMustBePaidRule(OrderStatus status) : IBusinessRule
{
    public bool IsBroken() => status != OrderStatus.Paid;
    public string Message => "Order must be paid before shipping.";
}
```

## Value Objects

```csharp
public sealed class Money : ValueObject
{
    public decimal Amount { get; }
    public string Currency { get; }

    public Money(decimal amount, string currency)
    {
        if (amount < 0) throw new DomainException("Money amount cannot be negative.");
        Amount = amount;
        Currency = currency.ToUpperInvariant();
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }

    public static Money operator +(Money a, Money b)
    {
        if (a.Currency != b.Currency) throw new DomainException("Cannot add different currencies.");
        return new Money(a.Amount + b.Amount, a.Currency);
    }
}
```

## Domain Events

```csharp
// Define an event
public sealed record OrderCreatedEvent(OrderId OrderId, CustomerId CustomerId)
    : DomainEvent;

// Events are automatically dispatched after SaveChanges
// (see AppDbContextBase in MarcusPrado.Platform.EfCore)
```

## Specifications

```csharp
public sealed class ActiveOrdersSpec : Specification<Order>
{
    public ActiveOrdersSpec() : base(o => o.Status != OrderStatus.Cancelled) { }
}

// Composition
var spec = new ActiveOrdersSpec()
    .And(new OrdersForCustomerSpec(customerId))
    .And(new OrdersAfterDateSpec(startDate));

IQueryable<Order> query = dbSet.Where(spec.ToExpression());
```

## Key Types

| Type | Purpose |
|------|---------|
| `Entity<TId>` | Base entity with identity equality and domain event recording |
| `AggregateRoot<TId>` | Extends `Entity<TId>` with `Version` for optimistic concurrency |
| `ValueObject` | Abstract base with structural equality via `GetEqualityComponents()` |
| `DomainEvent` | Abstract record base for domain events (carries `EventId`, `OccurredOn`) |
| `IDomainEvent` | Interface implemented by all domain events |
| `IBusinessRule` | `IsBroken()` + `Message` — enforced via `CheckRule()` |
| `BusinessRuleViolationException` | Thrown by `CheckRule()` when a rule is broken |
| `Specification<T>` | Composable query predicates (`And`, `Or`, `Not`, `ToExpression()`) |
| `EntityId` | Base for strongly-typed IDs |
| `TenantId`, `UserId`, `CorrelationId` | Built-in strongly-typed IDs |
| `IAuditable` | `CreatedAt`, `CreatedBy`, `UpdatedAt`, `UpdatedBy` — auto-filled by EfCore |
| `IPolicy` | Policy pattern: `EvaluateAsync()` → `PolicyResult` |

## Source

[`src/core/MarcusPrado.Platform.Domain`](../../src/core/MarcusPrado.Platform.Domain)
