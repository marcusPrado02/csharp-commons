using MarcusPrado.Platform.MultiTenancy.Quotas;

namespace MarcusPrado.Platform.MultiTenancy.Tests;

public sealed class TenantQuotaTests
{
    [Fact]
    public void DefaultQuota_HasSensibleLimits()
    {
        TenantQuota.Default.MaxRequestsPerMinute.Should().BePositive();
        TenantQuota.Default.MaxStorageBytes.Should().BePositive();
    }

    [Fact]
    public void UnlimitedQuota_HasMaxValues()
    {
        TenantQuota.Unlimited.MaxRequestsPerMinute.Should().Be(int.MaxValue);
        TenantQuota.Unlimited.MaxStorageBytes.Should().Be(long.MaxValue);
    }

    [Fact]
    public void IsRequestLimitExceeded_BelowLimit_ReturnsFalse()
    {
        var q = new TenantQuota(10, 1_000_000);
        q.IsRequestLimitExceeded(9).Should().BeFalse();
    }

    [Fact]
    public void IsRequestLimitExceeded_AtLimit_ReturnsTrue()
    {
        var q = new TenantQuota(10, 1_000_000);
        q.IsRequestLimitExceeded(10).Should().BeTrue();
    }

    [Fact]
    public void IsStorageLimitExceeded_AtLimit_ReturnsTrue()
    {
        var q = new TenantQuota(10, 100);
        q.IsStorageLimitExceeded(100).Should().BeTrue();
    }

    [Fact]
    public void QuotaExceededException_Message_ContainsTenantAndLimitType()
    {
        var ex = new QuotaExceededException("tenant-x", "rate-limit");
        ex.Message.Should().Contain("tenant-x").And.Contain("rate-limit");
    }
}
