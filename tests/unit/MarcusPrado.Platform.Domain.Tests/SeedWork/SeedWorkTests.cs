using MarcusPrado.Platform.Domain.Events;
using MarcusPrado.Platform.Domain.SeedWork;

namespace MarcusPrado.Platform.Domain.Tests.SeedWork;

// ── Test doubles ─────────────────────────────────────────────────────────────

file sealed record OrderId(Guid Value) : MarcusPrado.Platform.Domain.Identity.EntityId<Guid>(Value)
{
    public static OrderId New() => new(Guid.NewGuid());
}

file sealed record OrderPlaced(Guid OrderId) : DomainEvent;

file sealed class Order : AggregateRoot<OrderId>
{
    public string Status { get; private set; } = "Created";
    public IReadOnlyList<string> Lines { get; } = [];
    private readonly List<string> _lines = [];

    public Order(OrderId id) : base(id) { }

    public void Place()
    {
        CheckRule(new OrderMustHaveItemsRule(_lines.Count));
        Status = "Placed";
        IncrementVersion();
        AddDomainEvent(new OrderPlaced(Id.Value));
    }

    public void AddLine(string line)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(line, nameof(line));
        _lines.Add(line);
    }
}

file sealed class OrderMustHaveItemsRule : IBusinessRule
{
    private readonly int _count;
    public OrderMustHaveItemsRule(int count) => _count = count;
    public bool IsBroken() => _count == 0;
    public string Message => "An order must contain at least one item.";
}

// ── Entity<TId> tests ─────────────────────────────────────────────────────────

public sealed class EntityTests
{
    [Fact]
    public void Entities_WithSameId_AreEqual()
    {
        var id = OrderId.New();
        var a = new Order(id);
        var b = new Order(id);

        a.Should().Be(b);
        (a == b).Should().BeTrue();
    }

    [Fact]
    public void Entities_WithDifferentId_AreNotEqual()
    {
        var a = new Order(OrderId.New());
        var b = new Order(OrderId.New());

        a.Should().NotBe(b);
        (a != b).Should().BeTrue();
    }

    [Fact]
    public void Entity_NullId_Throws()
    {
        var act = () => new Order(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Entity_ToString_ContainsTypeName_And_Id()
    {
        var id = OrderId.New();
        var order = new Order(id);

        order.ToString().Should().Contain("Order").And.Contain(id.Value.ToString());
    }

    [Fact]
    public void GetHashCode_SameId_SameHash()
    {
        var id = OrderId.New();
        new Order(id).GetHashCode().Should().Be(new Order(id).GetHashCode());
    }
}

// ── AggregateRoot<TId> tests ──────────────────────────────────────────────────

public sealed class AggregateRootTests
{
    [Fact]
    public void InitialVersion_IsZero()
    {
        new Order(OrderId.New()).Version.Should().Be(0);
    }

    [Fact]
    public void Place_IncrementsVersion()
    {
        var order = new Order(OrderId.New());
        order.AddLine("Widget");
        order.Place();

        order.Version.Should().Be(1);
    }

    [Fact]
    public void Place_RaisesDomainEvent()
    {
        var order = new Order(OrderId.New());
        order.AddLine("Widget");
        order.Place();

        order.DomainEvents.Should().HaveCount(1)
             .And.ContainItemsAssignableTo<OrderPlaced>();
    }

    [Fact]
    public void ClearDomainEvents_RemovesAllEvents()
    {
        var order = new Order(OrderId.New());
        order.AddLine("Widget");
        order.Place();

        order.ClearDomainEvents();

        order.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void IDomainEventRecorder_ExposesEvents()
    {
        var order = new Order(OrderId.New());
        order.AddLine("Widget");
        order.Place();

        IDomainEventRecorder recorder = order;
        recorder.DomainEvents.Should().HaveCount(1);

        recorder.ClearDomainEvents();
        recorder.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void Place_WithNoItems_Throws_BusinessRuleViolation()
    {
        var order = new Order(OrderId.New());

        // Arrange: no items added
        var act = () => order.Place();

        act.Should().Throw<BusinessRuleViolationException>()
           .Which.BrokenRule.Message.Should().Contain("at least one item");
    }
}

// ── ValueObject tests ─────────────────────────────────────────────────────────

public sealed class ValueObjectTests
{
    private sealed class Money(decimal Amount, string Currency) : ValueObject
    {
        public decimal Amount { get; } = Amount;
        public string Currency { get; } = Currency;
        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Amount;
            yield return Currency;
        }
    }

    [Fact]
    public void ValueObjects_WithSameComponents_AreEqual()
    {
        var a = new Money(10m, "EUR");
        var b = new Money(10m, "EUR");

        a.Should().Be(b);
        (a == b).Should().BeTrue();
    }

    [Fact]
    public void ValueObjects_WithDifferentComponents_AreNotEqual()
    {
        var a = new Money(10m, "EUR");
        var b = new Money(10m, "USD");

        a.Should().NotBe(b);
        (a != b).Should().BeTrue();
    }

    [Fact]
    public void GetHashCode_SameComponents_SameHash()
    {
        new Money(5m, "GBP").GetHashCode()
            .Should().Be(new Money(5m, "GBP").GetHashCode());
    }

    [Fact]
    public void ValueObject_Equals_Null_ReturnsFalse()
    {
        new Money(1m, "USD").Equals(null).Should().BeFalse();
    }

    [Fact]
    public void ValueObject_Equals_DifferentType_ReturnsFalse()
    {
        var money = new Money(1m, "USD");
        var other = "not a value object";
        money.Equals(other).Should().BeFalse();
    }
}

// ── IBusinessRule + BusinessRuleViolationException tests ──────────────────────

public sealed class BusinessRuleTests
{
    private sealed class AlwaysBrokenRule : IBusinessRule
    {
        public bool IsBroken() => true;
        public string Message => "Always broken.";
    }

    private sealed class NeverBrokenRule : IBusinessRule
    {
        public bool IsBroken() => false;
        public string Message => "Never broken.";
    }

    [Fact]
    public void BrokenRule_Throws_WithCorrectMessage()
    {
        var rule = new AlwaysBrokenRule();
        var act = () => ThrowIfBroken(rule);

        act.Should().Throw<BusinessRuleViolationException>()
           .Which.Message.Should().Be("Always broken.");
    }

    [Fact]
    public void BrokenRule_Exception_ExposesRule()
    {
        var rule = new AlwaysBrokenRule();
        try { ThrowIfBroken(rule); }
        catch (BusinessRuleViolationException ex) { ex.BrokenRule.Should().Be(rule); }
    }

    [Fact]
    public void SatisfiedRule_DoesNotThrow()
    {
        var act = () => ThrowIfBroken(new NeverBrokenRule());
        act.Should().NotThrow();
    }

    private static void ThrowIfBroken(IBusinessRule rule)
    {
        if (rule.IsBroken()) throw new BusinessRuleViolationException(rule);
    }
}
