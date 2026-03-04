namespace MarcusPrado.Platform.MultiTenancy.Quotas;

/// <summary>Returns quota configuration for a given tenant.</summary>
public interface ITenantQuotaProvider
{
    /// <summary>Returns the quota for <paramref name="tenantId"/>.</summary>
    Task<TenantQuota> GetQuotaAsync(string tenantId, CancellationToken ct = default);
}
