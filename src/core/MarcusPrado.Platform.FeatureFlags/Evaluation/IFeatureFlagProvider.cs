namespace MarcusPrado.Platform.FeatureFlags.Evaluation;

/// <summary>Evaluates feature flags for a given context.</summary>
public interface IFeatureFlagProvider
{
    /// <summary>Evaluates flag <paramref name="flagKey"/> for the given <paramref name="context"/>.</summary>
    Task<FeatureDecision> EvaluateAsync(string flagKey, FeatureFlagContext context, CancellationToken ct = default);

    /// <summary>Returns true if flag <paramref name="flagKey"/> is enabled for <paramref name="context"/>.</summary>
    Task<bool> IsEnabledAsync(string flagKey, FeatureFlagContext context, CancellationToken ct = default);
}
