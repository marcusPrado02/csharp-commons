namespace MarcusPrado.Platform.MultiTenancy.Quotas;

/// <summary>Thrown when a tenant exceeds their allocated quota.</summary>
public sealed class QuotaExceededException : Exception
{
    /// <summary>The tenant that exceeded the quota.</summary>
    public string TenantId { get; }

    /// <summary>The type of limit that was exceeded (e.g. "RequestsPerMinute").</summary>
    public string LimitType { get; }

    /// <summary>
    /// Initialises a new <see cref="QuotaExceededException"/>.
    /// </summary>
    public QuotaExceededException(string tenantId, string limitType)
        : base($"Tenant '{tenantId}' exceeded quota limit '{limitType}'.")
    {
        TenantId = tenantId;
        LimitType = limitType;
    }
}
