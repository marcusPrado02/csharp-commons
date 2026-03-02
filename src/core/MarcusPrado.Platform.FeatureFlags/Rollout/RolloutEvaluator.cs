using MarcusPrado.Platform.FeatureFlags.Evaluation;
using MarcusPrado.Platform.FeatureFlags.Flags;
using MarcusPrado.Platform.FeatureFlags.Internal;

namespace MarcusPrado.Platform.FeatureFlags.Rollout;

/// <summary>Evaluates rollout strategies against a <see cref="FeatureFlagContext"/>.</summary>
internal static class RolloutEvaluator
{
    /// <summary>Returns a decision for <paramref name="flag"/> given <paramref name="context"/>.</summary>
    internal static FeatureDecision Evaluate(FeatureFlag flag, FeatureFlagContext context)
    {
        if (!flag.Enabled)
        {
            return FeatureDecision.Disabled(flag.Key, "flag-disabled");
        }

        return flag.Strategy switch
        {
            RolloutStrategy.Boolean => FeatureDecision.Enabled(flag.Key, "boolean"),
            RolloutStrategy.TenantWhitelist => EvaluateTenantWhitelist(flag, context),
            RolloutStrategy.UserWhitelist => EvaluateUserWhitelist(flag, context),
            RolloutStrategy.Percentage or RolloutStrategy.Canary => EvaluatePercentage(flag, context),
            _ => FeatureDecision.Disabled(flag.Key, "unknown-strategy"),
        };
    }

    private static FeatureDecision EvaluateTenantWhitelist(FeatureFlag flag, FeatureFlagContext ctx)
    {
        if (ctx.TenantId is not null && flag.TenantWhitelist.Contains(ctx.TenantId))
        {
            return FeatureDecision.Enabled(flag.Key, "tenant-whitelist");
        }

        return FeatureDecision.Disabled(flag.Key, "tenant-not-in-whitelist");
    }

    private static FeatureDecision EvaluateUserWhitelist(FeatureFlag flag, FeatureFlagContext ctx)
    {
        if (ctx.UserId is not null && flag.UserWhitelist.Contains(ctx.UserId))
        {
            return FeatureDecision.Enabled(flag.Key, "user-whitelist");
        }

        return FeatureDecision.Disabled(flag.Key, "user-not-in-whitelist");
    }

    private static FeatureDecision EvaluatePercentage(FeatureFlag flag, FeatureFlagContext ctx)
    {
        var pct = flag.Percentage ?? 0;
        if (pct <= 0)
        {
            return FeatureDecision.Disabled(flag.Key, "percentage-zero");
        }

        if (pct >= 100)
        {
            return FeatureDecision.Enabled(flag.Key, "percentage-full");
        }

        // Deterministic hash: flag key + user/tenant id
        var seed = $"{flag.Key}:{ctx.UserId ?? ctx.TenantId ?? "anon"}";
        var bucket = (Math.Abs(seed.GetDeterministicHash()) % 100) + 1;

        if (bucket <= pct)
        {
            return FeatureDecision.Enabled(flag.Key, $"percentage-{pct}");
        }

        return FeatureDecision.Disabled(flag.Key, $"percentage-{pct}");
    }
}
