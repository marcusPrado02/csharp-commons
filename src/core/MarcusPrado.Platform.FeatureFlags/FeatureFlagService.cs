using MarcusPrado.Platform.FeatureFlags.Evaluation;

namespace MarcusPrado.Platform.FeatureFlags;

/// <summary>High-level service for evaluating feature flags.</summary>
public sealed class FeatureFlagService
{
    private readonly IFeatureFlagProvider _provider;

    /// <summary>Initialises the service with the given provider.</summary>
    public FeatureFlagService(IFeatureFlagProvider provider)
    {
        _provider = provider;
    }

    /// <summary>Returns true if the flag is enabled for the given context.</summary>
    public Task<bool> IsEnabledAsync(string flagKey, FeatureFlagContext? context = null, CancellationToken ct = default) =>
        _provider.IsEnabledAsync(flagKey, context ?? FeatureFlagContext.Anonymous, ct);

    /// <summary>Returns the full evaluation decision for the flag.</summary>
    public Task<FeatureDecision> EvaluateAsync(string flagKey, FeatureFlagContext? context = null, CancellationToken ct = default) =>
        _provider.EvaluateAsync(flagKey, context ?? FeatureFlagContext.Anonymous, ct);
}
