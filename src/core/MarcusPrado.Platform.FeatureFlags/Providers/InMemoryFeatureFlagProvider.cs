using System.Collections.Concurrent;
using MarcusPrado.Platform.FeatureFlags.Evaluation;
using MarcusPrado.Platform.FeatureFlags.Flags;
using MarcusPrado.Platform.FeatureFlags.Rollout;

namespace MarcusPrado.Platform.FeatureFlags.Providers;

/// <summary>
/// Thread-safe in-memory flag provider. Ideal for unit tests and local development.
/// </summary>
public sealed class InMemoryFeatureFlagProvider : IFeatureFlagProvider
{
    private readonly ConcurrentDictionary<string, FeatureFlag> _flags = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Adds or replaces a flag definition.</summary>
    public void SetFlag(FeatureFlag flag) => _flags[flag.Key] = flag;

    /// <summary>Removes a flag definition.</summary>
    public void RemoveFlag(string key) => _flags.TryRemove(key, out _);

    /// <inheritdoc/>
    public Task<FeatureDecision> EvaluateAsync(string flagKey, FeatureFlagContext context, CancellationToken ct = default)
    {
        if (!_flags.TryGetValue(flagKey, out var flag))
        {
            return Task.FromResult(FeatureDecision.NotFound(flagKey));
        }

        return Task.FromResult(RolloutEvaluator.Evaluate(flag, context));
    }

    /// <inheritdoc/>
    public async Task<bool> IsEnabledAsync(string flagKey, FeatureFlagContext context, CancellationToken ct = default)
    {
        var decision = await EvaluateAsync(flagKey, context, ct).ConfigureAwait(false);
        return decision.IsEnabled;
    }
}
