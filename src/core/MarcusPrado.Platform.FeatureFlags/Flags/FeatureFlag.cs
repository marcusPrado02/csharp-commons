using MarcusPrado.Platform.FeatureFlags.Rollout;

namespace MarcusPrado.Platform.FeatureFlags.Flags;

/// <summary>Configuration record for a feature flag.</summary>
public sealed class FeatureFlag
{
    /// <summary>Gets the unique flag key (e.g. "new-checkout-flow").</summary>
    public string Key { get; init; } = string.Empty;

    /// <summary>Gets whether the flag is active at all.</summary>
    public bool Enabled { get; init; }

    /// <summary>Gets the rollout strategy used to decide enablement per context.</summary>
    public RolloutStrategy Strategy { get; init; } = RolloutStrategy.Boolean;

    /// <summary>Gets an optional percentage (0–100) for percentage-based rollout.</summary>
    public double? Percentage { get; init; }

    /// <summary>Gets the set of tenant IDs that have this flag enabled.</summary>
    public IReadOnlySet<string> TenantWhitelist { get; init; } = new HashSet<string>();

    /// <summary>Gets the set of user IDs that have this flag enabled.</summary>
    public IReadOnlySet<string> UserWhitelist { get; init; } = new HashSet<string>();

    /// <summary>Gets the named variants (for multivariate flags).</summary>
    public IReadOnlyList<FeatureVariant> Variants { get; init; } = Array.Empty<FeatureVariant>();

    /// <summary>Gets an optional description.</summary>
    public string? Description { get; init; }
}
