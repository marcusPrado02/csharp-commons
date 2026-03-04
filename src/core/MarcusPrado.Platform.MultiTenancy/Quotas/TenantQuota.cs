namespace MarcusPrado.Platform.MultiTenancy.Quotas;

/// <summary>Defines resource limits for a tenant.</summary>
public sealed record TenantQuota(
    int  MaxRequestsPerMinute,
    long MaxStorageBytes)
{
    /// <summary>Conservative defaults suitable for free-tier tenants.</summary>
    public static TenantQuota Default  { get; } = new(MaxRequestsPerMinute: 60,        MaxStorageBytes: 10_737_418_240L);  // 10 GB

    /// <summary>Unlimited quota — use for system/internal tenants.</summary>
    public static TenantQuota Unlimited { get; } = new(MaxRequestsPerMinute: int.MaxValue, MaxStorageBytes: long.MaxValue);

    /// <summary>Returns <see langword="true"/> when <paramref name="requestCount"/> meets or exceeds the per-minute limit.</summary>
    public bool IsRequestLimitExceeded(int requestCount) => requestCount >= MaxRequestsPerMinute;

    /// <summary>Returns <see langword="true"/> when <paramref name="bytesUsed"/> meets or exceeds the storage limit.</summary>
    public bool IsStorageLimitExceeded(long bytesUsed) => bytesUsed >= MaxStorageBytes;
}
