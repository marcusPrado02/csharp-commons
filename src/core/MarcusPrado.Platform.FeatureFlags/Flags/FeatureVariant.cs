namespace MarcusPrado.Platform.FeatureFlags.Flags;

/// <summary>A named variant of a feature flag with an associated payload value.</summary>
public sealed class FeatureVariant
{
    /// <summary>Gets the variant key (e.g. "control", "treatment-a").</summary>
    public string Key { get; init; } = string.Empty;

    /// <summary>Gets the optional JSON or scalar value associated with this variant.</summary>
    public string? Value { get; init; }

    /// <summary>Gets the percentage of traffic (0–100) that receives this variant.</summary>
    public double Weight { get; init; }

    /// <summary>Returns a boolean variant (on/off).</summary>
    public static FeatureVariant On { get; } = new() { Key = "on", Value = "true", Weight = 100 };

    /// <summary>Returns a boolean off variant.</summary>
    public static FeatureVariant Off { get; } = new() { Key = "off", Value = "false", Weight = 0 };
}
