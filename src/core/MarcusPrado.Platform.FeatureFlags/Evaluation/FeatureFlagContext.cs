namespace MarcusPrado.Platform.FeatureFlags.Evaluation;

/// <summary>Provides contextual information for feature flag evaluation.</summary>
public sealed class FeatureFlagContext
{
    /// <summary>Gets the tenant identifier, if any.</summary>
    public string? TenantId { get; init; }

    /// <summary>Gets the user identifier, if any.</summary>
    public string? UserId { get; init; }

    /// <summary>Gets the deployment environment name (e.g. "production", "staging").</summary>
    public string? Environment { get; init; }

    /// <summary>Gets the deployment region (e.g. "us-east-1").</summary>
    public string? Region { get; init; }

    /// <summary>Gets additional custom attributes for rule evaluation.</summary>
    public IReadOnlyDictionary<string, string> Attributes { get; init; } = new Dictionary<string, string>();

    /// <summary>Returns an anonymous context with no identifying information.</summary>
    public static FeatureFlagContext Anonymous { get; } = new();
}
