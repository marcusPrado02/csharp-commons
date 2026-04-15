using MarcusPrado.Platform.FeatureFlags.Flags;

namespace MarcusPrado.Platform.FeatureFlags.Evaluation;

/// <summary>The result of evaluating a feature flag for a given context.</summary>
public sealed class FeatureDecision
{
    private FeatureDecision() { }

    /// <summary>Gets whether the feature is enabled.</summary>
    public bool IsEnabled { get; private init; }

    /// <summary>Gets the flag key that was evaluated.</summary>
    public string FlagKey { get; private init; } = string.Empty;

    /// <summary>Gets the variant selected (null when flag is off or no variants configured).</summary>
    public FeatureVariant? Variant { get; private init; }

    /// <summary>Gets the reason why this decision was made.</summary>
    public string Reason { get; private init; } = string.Empty;

    /// <summary>Creates an enabled decision.</summary>
    public static FeatureDecision Enabled(string flagKey, string reason, FeatureVariant? variant = null) =>
        new()
        {
            IsEnabled = true,
            FlagKey = flagKey,
            Reason = reason,
            Variant = variant,
        };

    /// <summary>Creates a disabled decision.</summary>
    public static FeatureDecision Disabled(string flagKey, string reason) =>
        new()
        {
            IsEnabled = false,
            FlagKey = flagKey,
            Reason = reason,
        };

    /// <summary>Creates a decision for a flag that was not found.</summary>
    public static FeatureDecision NotFound(string flagKey) =>
        new()
        {
            IsEnabled = false,
            FlagKey = flagKey,
            Reason = "flag-not-found",
        };
}
