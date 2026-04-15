using MarcusPrado.Platform.TestKit.Builders;

namespace MarcusPrado.Platform.TestKit.Tests;

/// <summary>
/// Unit tests for the lightweight Test Data Builder helpers (#85).
/// </summary>
public sealed class BuildersTests
{
    // ── EntityFaker ─────────────────────────────────────────────────────────

    private sealed class SampleFaker : EntityFaker<SampleEntity>
    {
        public override SampleEntity Build() => new(
            Id: NewId(),
            Name: RandomString(12),
            Email: RandomEmail(),
            Value: RandomDecimal());
    }

    private sealed record SampleEntity(Guid Id, string Name, string Email, decimal Value);

    [Fact]
    public void EntityFaker_Build_ReturnsNonNullInstance()
    {
        var faker = new SampleFaker();
        var entity = faker.Build();
        entity.Should().NotBeNull();
    }

    [Fact]
    public void EntityFaker_Build_GeneratesUniqueIds()
    {
        var faker = new SampleFaker();
        var ids = faker.BuildMany(50).Select(e => e.Id).Distinct().ToList();
        ids.Should().HaveCount(50, because: "each generated entity should have a unique ID");
    }

    [Fact]
    public void EntityFaker_BuildMany_ReturnsRequestedCount()
    {
        var faker = new SampleFaker();
        faker.BuildMany(7).Should().HaveCount(7);
    }

    [Fact]
    public void EntityFaker_RandomEmail_ContainsAtAndDomain()
    {
        var faker = new SampleFaker();
        var entity = faker.Build();
        entity.Email.Should().Contain("@").And.EndWith(".test");
    }

    [Fact]
    public void EntityFaker_RandomDecimal_IsWithinDefaultRange()
    {
        var faker = new SampleFaker();
        var values = faker.BuildMany(20).Select(e => e.Value).ToList();
        values.Should().AllSatisfy(v => v.Should().BeInRange(0.01m, 9_999.99m));
    }

    // ── CommandFaker ─────────────────────────────────────────────────────────

    private sealed class SampleCommandFaker : CommandFaker<SampleCommand>
    {
        public override SampleCommand Build() => new(
            OrderId: NewId(),
            Amount: RandomDecimal(1m, 500m),
            Currency: PickRandom("BRL", "USD", "EUR"));
    }

    private sealed record SampleCommand(Guid OrderId, decimal Amount, string Currency);

    [Fact]
    public void CommandFaker_Build_ReturnsValidCommand()
    {
        var faker = new SampleCommandFaker();
        var cmd = faker.Build();
        cmd.OrderId.Should().NotBe(Guid.Empty);
        cmd.Amount.Should().BePositive();
        cmd.Currency.Should().BeOneOf("BRL", "USD", "EUR");
    }

    [Fact]
    public void CommandFaker_BuildMany_ReturnsRequestedCount()
    {
        var faker = new SampleCommandFaker();
        faker.BuildMany(5).Should().HaveCount(5);
    }

    // ── TestDataScenarios ────────────────────────────────────────────────────

    [Fact]
    public void FreeTenant_HasFreeplanAndIsActive()
    {
        var t = TestDataScenarios.FreeTenant;
        t.Plan.Should().Be("free");
        t.IsActive.Should().BeTrue();
    }

    [Fact]
    public void PremiumTenant_HasPremiumPlan()
    {
        var t = TestDataScenarios.PremiumTenant;
        t.Plan.Should().Be("premium");
        t.IsActive.Should().BeTrue();
    }

    [Fact]
    public void SuspendedTenant_IsNotActive()
    {
        TestDataScenarios.SuspendedTenant.IsActive.Should().BeFalse();
    }

    [Fact]
    public void ValidUser_HasPermissionsAndScopes()
    {
        var u = TestDataScenarios.ValidUser;
        u.Permissions.Should().NotBeEmpty();
        u.Scopes.Should().NotBeEmpty();
        u.IsActive.Should().BeTrue();
    }

    [Fact]
    public void AnonymousUser_HasNoPermissions()
    {
        var u = TestDataScenarios.AnonymousUser;
        u.UserId.Should().BeNull();
        u.Permissions.Should().BeEmpty();
    }

    [Fact]
    public void ExpiredSubscription_IsNotActiveAndInPast()
    {
        var s = TestDataScenarios.ExpiredSubscription;
        s.IsActive.Should().BeFalse();
        s.ExpiresAt.Should().BeBefore(DateTimeOffset.UtcNow);
    }

    [Fact]
    public void ActiveSubscription_IsActiveAndInFuture()
    {
        var s = TestDataScenarios.ActiveSubscription;
        s.IsActive.Should().BeTrue();
        s.ExpiresAt.Should().BeAfter(DateTimeOffset.UtcNow);
    }
}
