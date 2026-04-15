using MarcusPrado.Platform.FeatureFlags.Evaluation;
using MarcusPrado.Platform.FeatureFlags.Flags;
using MarcusPrado.Platform.FeatureFlags.Rollout;

namespace MarcusPrado.Platform.FeatureFlags.Providers;

/// <summary>
/// Reads feature flags from environment variables.
/// Variable format: <c>FEATURE__{FLAG_KEY}=true|false|{percentage}</c>
/// where FLAG_KEY has dashes replaced by underscores.
/// </summary>
public sealed class EnvironmentFeatureFlagProvider : IFeatureFlagProvider
{
    private const string Prefix = "FEATURE__";

    /// <inheritdoc/>
    public Task<FeatureDecision> EvaluateAsync(
        string flagKey,
        FeatureFlagContext context,
        CancellationToken ct = default
    )
    {
        var envKey = Prefix + flagKey.Replace("-", "_", StringComparison.Ordinal).ToUpperInvariant();
        var value = Environment.GetEnvironmentVariable(envKey);

        if (value is null)
        {
            return Task.FromResult(FeatureDecision.NotFound(flagKey));
        }

        if (bool.TryParse(value, out var enabled))
        {
            return Task.FromResult(
                enabled
                    ? FeatureDecision.Enabled(flagKey, "env-boolean")
                    : FeatureDecision.Disabled(flagKey, "env-boolean")
            );
        }

        if (
            double.TryParse(
                value,
                System.Globalization.NumberStyles.Number,
                System.Globalization.CultureInfo.InvariantCulture,
                out var pct
            )
        )
        {
            var flag = new FeatureFlag
            {
                Key = flagKey,
                Enabled = true,
                Strategy = RolloutStrategy.Percentage,
                Percentage = pct,
            };
            return Task.FromResult(RolloutEvaluator.Evaluate(flag, context));
        }

        return Task.FromResult(FeatureDecision.Disabled(flagKey, "env-parse-error"));
    }

    /// <inheritdoc/>
    public async Task<bool> IsEnabledAsync(string flagKey, FeatureFlagContext context, CancellationToken ct = default)
    {
        var decision = await EvaluateAsync(flagKey, context, ct).ConfigureAwait(false);
        return decision.IsEnabled;
    }
}
