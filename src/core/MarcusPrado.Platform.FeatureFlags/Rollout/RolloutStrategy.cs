namespace MarcusPrado.Platform.FeatureFlags.Rollout;

/// <summary>Determines how a feature flag decides on enablement.</summary>
public enum RolloutStrategy
{
    /// <summary>Flag is either fully on or fully off.</summary>
    Boolean,

    /// <summary>Enabled for a percentage of users/tenants based on a hash.</summary>
    Percentage,

    /// <summary>Enabled only for specific tenants in a whitelist.</summary>
    TenantWhitelist,

    /// <summary>Enabled only for specific users in a whitelist.</summary>
    UserWhitelist,

    /// <summary>Canary deployment — enabled only for the first percentage of traffic.</summary>
    Canary,
}
