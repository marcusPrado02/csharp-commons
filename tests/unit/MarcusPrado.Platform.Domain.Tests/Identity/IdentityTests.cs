using MarcusPrado.Platform.Domain.Identity;

namespace MarcusPrado.Platform.Domain.Tests.Identity;

public sealed class IdentityTests
{
    // ── EntityId hierarchy ────────────────────────────────────────────────────

    [Fact]
    public void TenantId_New_ProducesUniqueIds()
    {
        var a = TenantId.New();
        var b = TenantId.New();

        a.Should().NotBe(b);
    }

    [Fact]
    public void TenantId_ValueEquality_SameGuid()
    {
        var guid = Guid.NewGuid();
        var a = new TenantId(guid);
        var b = new TenantId(guid);

        a.Should().Be(b);
    }

    [Fact]
    public void TenantId_ImplicitFromGuid()
    {
        var guid = Guid.NewGuid();
        TenantId id = guid; // implicit conversion

        id.Value.Should().Be(guid);
    }

    [Fact]
    public void TenantId_ImplicitToGuid()
    {
        var id = TenantId.New();
        Guid raw = id; // implicit conversion

        raw.Should().Be(id.Value);
    }

    [Fact]
    public void TenantId_ToString_ReturnsGuidString()
    {
        var id = TenantId.New();
        id.ToString().Should().Be(id.Value.ToString());
    }

    // ── UserId ────────────────────────────────────────────────────────────────

    [Fact]
    public void UserId_ImplicitConversions_RoundTrip()
    {
        var guid = Guid.NewGuid();
        UserId userId = guid;
        Guid back = userId;

        back.Should().Be(guid);
    }

    [Fact]
    public void UserId_AndTenantId_AreNotEqual_EvenWithSameGuid()
    {
        var guid = Guid.NewGuid();
        var tenantId = new TenantId(guid);
        var userId = new UserId(guid);

        // Record equality = type + value, so different types are NOT equal
        tenantId.Equals(userId).Should().BeFalse();
    }

    // ── CorrelationId ─────────────────────────────────────────────────────────

    [Fact]
    public void CorrelationId_New_IsNotEmpty()
    {
        CorrelationId.New().Value.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void CorrelationId_ImplicitFromString()
    {
        CorrelationId cid = "trace-abc-123";
        cid.Value.Should().Be("trace-abc-123");
    }

    [Fact]
    public void CorrelationId_ImplicitToString()
    {
        var cid = new CorrelationId("trace-xyz");
        string s = cid;
        s.Should().Be("trace-xyz");
    }

    // ── EntityId as abstract base marker ─────────────────────────────────────

    [Fact]
    public void TenantId_IsEntityId()
    {
        TenantId.New().Should().BeAssignableTo<EntityId>();
    }

    [Fact]
    public void UserId_IsEntityIdOfGuid()
    {
        UserId.New().Should().BeAssignableTo<EntityId<Guid>>();
    }
}
