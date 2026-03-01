using MarcusPrado.Platform.Domain.Specifications;

namespace MarcusPrado.Platform.Domain.Tests.Specifications;

public sealed class SpecificationTests
{
    // ── Test domain ───────────────────────────────────────────────────────────

    private sealed record Product(string Name, decimal Price, bool IsActive);

    private sealed class ActiveProductSpec : Specification<Product>
    {
        public override bool IsSatisfiedBy(Product candidate) => candidate.IsActive;
    }

    private sealed class AffordableProductSpec(decimal MaxPrice) : Specification<Product>
    {
        public override bool IsSatisfiedBy(Product candidate) => candidate.Price <= MaxPrice;
    }

    private readonly Product _cheapActive     = new("Widget",  9.99m,  true);
    private readonly Product _expensiveActive = new("Gadget", 99.99m, true);
    private readonly Product _cheapInactive   = new("Junk",   4.99m,  false);

    [Fact]
    public void ActiveSpec_ActiveProduct_IsTrue()
    {
        new ActiveProductSpec().IsSatisfiedBy(_cheapActive).Should().BeTrue();
    }

    [Fact]
    public void ActiveSpec_InactiveProduct_IsFalse()
    {
        new ActiveProductSpec().IsSatisfiedBy(_cheapInactive).Should().BeFalse();
    }

    // ── And ───────────────────────────────────────────────────────────────────

    [Fact]
    public void And_BothSatisfied_IsTrue()
    {
        var spec = new ActiveProductSpec().And(new AffordableProductSpec(20m));
        spec.IsSatisfiedBy(_cheapActive).Should().BeTrue();
    }

    [Fact]
    public void And_OnlyFirstSatisfied_IsFalse()
    {
        var spec = new ActiveProductSpec().And(new AffordableProductSpec(20m));
        spec.IsSatisfiedBy(_expensiveActive).Should().BeFalse();
    }

    [Fact]
    public void And_NeitherSatisfied_IsFalse()
    {
        var spec = new ActiveProductSpec().And(new AffordableProductSpec(1m));
        spec.IsSatisfiedBy(_cheapInactive).Should().BeFalse();
    }

    // ── Or ────────────────────────────────────────────────────────────────────

    [Fact]
    public void Or_AtLeastOneSatisfied_IsTrue()
    {
        var spec = new ActiveProductSpec().Or(new AffordableProductSpec(1m));

        // cheapActive: active=true OR price<=1=false → true
        spec.IsSatisfiedBy(_cheapActive).Should().BeTrue();
    }

    [Fact]
    public void Or_NeitherSatisfied_IsFalse()
    {
        // inactive AND too expensive → false
        var spec = new ActiveProductSpec().Or(new AffordableProductSpec(1m));
        spec.IsSatisfiedBy(_expensiveActive with { }).Should().BeTrue(); // active!
        spec.IsSatisfiedBy(new Product("X", 100m, false)).Should().BeFalse();
    }

    // ── Not ───────────────────────────────────────────────────────────────────

    [Fact]
    public void Not_InvertsResult()
    {
        var notActive = new ActiveProductSpec().Not();

        notActive.IsSatisfiedBy(_cheapActive).Should().BeFalse();
        notActive.IsSatisfiedBy(_cheapInactive).Should().BeTrue();
    }

    // ── Chaining ──────────────────────────────────────────────────────────────

    [Fact]
    public void Chaining_ActiveAndAffordableAndNotJunk()
    {
        var spec = new ActiveProductSpec()
            .And(new AffordableProductSpec(20m))
            .And(Specification<Product>.Create(p => p.Name != "Junk"));

        spec.IsSatisfiedBy(_cheapActive).Should().BeTrue();
        spec.IsSatisfiedBy(_cheapInactive).Should().BeFalse();
        spec.IsSatisfiedBy(new Product("Junk", 5m, true)).Should().BeFalse();
    }

    // ── Predicate factory ─────────────────────────────────────────────────────

    [Fact]
    public void Create_LambdaSpec_Works()
    {
        var spec = Specification<Product>.Create(p => p.Name.StartsWith("W"));

        spec.IsSatisfiedBy(_cheapActive).Should().BeTrue();   // "Widget"
        spec.IsSatisfiedBy(_expensiveActive).Should().BeFalse(); // "Gadget"
    }

    [Fact]
    public void Create_NullPredicate_Throws()
    {
        var act = () => Specification<Product>.Create(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    // ── Null argument guards ───────────────────────────────────────────────────

    [Fact]
    public void And_NullArgument_Throws()
    {
        var act = () => new ActiveProductSpec().And(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Or_NullArgument_Throws()
    {
        var act = () => new ActiveProductSpec().Or(null!);
        act.Should().Throw<ArgumentNullException>();
    }
}
