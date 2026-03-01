using MarcusPrado.Platform.Domain.Auditing;
using MarcusPrado.Platform.Domain.Policies;

namespace MarcusPrado.Platform.Domain.Tests;

// ── AuditRecord tests ─────────────────────────────────────────────────────────

public sealed class AuditRecordTests
{
    [Fact]
    public void Create_SetsCreatedByAndAt()
    {
        var before = DateTimeOffset.UtcNow;
        var audit = AuditRecord.Create("alice");
        var after = DateTimeOffset.UtcNow;

        audit.CreatedBy.Should().Be("alice");
        audit.CreatedAt.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
        audit.UpdatedAt.Should().BeNull();
        audit.DeletedAt.Should().BeNull();
        audit.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public void Create_WithExplicitTimestamp()
    {
        var ts = new DateTimeOffset(2025, 1, 1, 10, 0, 0, TimeSpan.Zero);
        var audit = AuditRecord.Create("system", ts);

        audit.CreatedAt.Should().Be(ts);
    }

    [Fact]
    public void Create_NullOrWhitespaceCreator_Throws()
    {
        var act = () => AuditRecord.Create("  ");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Update_ReturnsNewRecord_WithUpdatedFields()
    {
        var audit = AuditRecord.Create("alice");
        var updated = audit.Update("bob");

        updated.UpdatedBy.Should().Be("bob");
        updated.UpdatedAt.Should().NotBeNull();
        // Original is immutable
        audit.UpdatedBy.Should().BeNull();
    }

    [Fact]
    public void Delete_SetsDeletedFields_IsDeletedTrue()
    {
        var audit = AuditRecord.Create("alice");
        var deleted = audit.Delete("admin");

        deleted.IsDeleted.Should().BeTrue();
        deleted.DeletedBy.Should().Be("admin");
        deleted.DeletedAt.Should().NotBeNull();
    }

    [Fact]
    public void Restore_ClearsDeletedFields()
    {
        var deleted = AuditRecord.Create("alice").Delete("admin");
        var restored = deleted.Restore();

        restored.IsDeleted.Should().BeFalse();
        restored.DeletedAt.Should().BeNull();
        restored.DeletedBy.Should().BeNull();
    }

    [Fact]
    public void AuditRecord_IsImmutable_OriginalUnchanged()
    {
        var original = AuditRecord.Create("alice");
        _ = original.Update("bob").Delete("carol");

        original.UpdatedBy.Should().BeNull();
        original.DeletedAt.Should().BeNull();
    }
}

// ── PolicyResult tests ────────────────────────────────────────────────────────

public sealed class PolicyResultTests
{
    [Fact]
    public void Allow_IsAllowed_IsTrue()
    {
        var result = PolicyResult.Allow();

        result.IsAllowed.Should().BeTrue();
        result.IsDenied.Should().BeFalse();
    }

    [Fact]
    public void Allow_WithReason_StoresReason()
    {
        var result = PolicyResult.Allow("Approved by manager");
        result.Reason.Should().Be("Approved by manager");
    }

    [Fact]
    public void Deny_IsAllowed_IsFalse()
    {
        var result = PolicyResult.Deny("Insufficient funds.");

        result.IsAllowed.Should().BeFalse();
        result.IsDenied.Should().BeTrue();
        result.Reason.Should().Be("Insufficient funds.");
    }

    [Fact]
    public void Deny_NullOrWhitespaceReason_Throws()
    {
        var act = () => PolicyResult.Deny("  ");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Allow_ToString_ContainsAllowed()
    {
        PolicyResult.Allow("ok").ToString().Should().Contain("Allowed");
    }

    [Fact]
    public void Deny_ToString_ContainsDenied()
    {
        PolicyResult.Deny("nope").ToString().Should().Contain("Denied");
    }

    // ── IPolicy<T> usage example ──────────────────────────────────────────────

    [Fact]
    public void IPolicy_Evaluate_ReturnsCorrectResult()
    {
        IPolicy<decimal> policy = new MinimumBalancePolicy(100m);

        policy.Evaluate(200m).IsAllowed.Should().BeTrue();
        policy.Evaluate(50m).IsDenied.Should().BeTrue();
    }

    private sealed class MinimumBalancePolicy(decimal Minimum) : IPolicy<decimal>
    {
        public PolicyResult Evaluate(decimal balance) =>
            balance >= Minimum
                ? PolicyResult.Allow()
                : PolicyResult.Deny($"Balance {balance} is below minimum {Minimum}.");
    }
}
