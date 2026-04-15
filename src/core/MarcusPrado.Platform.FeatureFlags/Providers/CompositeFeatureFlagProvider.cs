using MarcusPrado.Platform.FeatureFlags.Evaluation;

namespace MarcusPrado.Platform.FeatureFlags.Providers;

/// <summary>
/// Queries a chain of <see cref="IFeatureFlagProvider"/> instances and returns the first
/// definitive answer (i.e. the first provider that does not return flag-not-found).
/// </summary>
public sealed class CompositeFeatureFlagProvider : IFeatureFlagProvider
{
    private readonly IReadOnlyList<IFeatureFlagProvider> _providers;

    /// <summary>Initialises the composite with an ordered list of providers.</summary>
    public CompositeFeatureFlagProvider(IEnumerable<IFeatureFlagProvider> providers)
    {
        _providers = providers.ToList();
    }

    /// <inheritdoc/>
    public async Task<FeatureDecision> EvaluateAsync(
        string flagKey,
        FeatureFlagContext context,
        CancellationToken ct = default
    )
    {
        foreach (var provider in _providers)
        {
            var decision = await provider.EvaluateAsync(flagKey, context, ct).ConfigureAwait(false);
            if (decision.Reason != "flag-not-found")
            {
                return decision;
            }
        }

        return FeatureDecision.NotFound(flagKey);
    }

    /// <inheritdoc/>
    public async Task<bool> IsEnabledAsync(string flagKey, FeatureFlagContext context, CancellationToken ct = default)
    {
        var decision = await EvaluateAsync(flagKey, context, ct).ConfigureAwait(false);
        return decision.IsEnabled;
    }
}
