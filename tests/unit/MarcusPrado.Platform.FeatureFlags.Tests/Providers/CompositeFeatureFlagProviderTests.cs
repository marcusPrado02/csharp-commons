namespace MarcusPrado.Platform.FeatureFlags.Tests.Providers;

public sealed class CompositeFeatureFlagProviderTests
{
    [Fact]
    public async Task FirstProviderResponds_ReturnsItsDecision()
    {
        var primary = new InMemoryFeatureFlagProvider();
        primary.SetFlag(new FeatureFlag { Key = "f", Enabled = true });

        var fallback = new InMemoryFeatureFlagProvider();
        // fallback has "f" disabled
        fallback.SetFlag(new FeatureFlag { Key = "f", Enabled = false });

        var composite = new CompositeFeatureFlagProvider(new[] { primary, fallback });

        var d = await composite.EvaluateAsync("f", FeatureFlagContext.Anonymous);

        Assert.True(d.IsEnabled);
    }

    [Fact]
    public async Task FirstProviderMisses_FallsThrough()
    {
        var primary = new InMemoryFeatureFlagProvider(); // no flags
        var fallback = new InMemoryFeatureFlagProvider();
        fallback.SetFlag(new FeatureFlag { Key = "g", Enabled = true });

        var composite = new CompositeFeatureFlagProvider(new[] { primary, fallback });

        var d = await composite.EvaluateAsync("g", FeatureFlagContext.Anonymous);

        Assert.True(d.IsEnabled);
    }

    [Fact]
    public async Task NoProviderResponds_ReturnsNotFound()
    {
        var composite = new CompositeFeatureFlagProvider(
            new[] { new InMemoryFeatureFlagProvider() });

        var d = await composite.EvaluateAsync("no-flag", FeatureFlagContext.Anonymous);

        Assert.Equal("flag-not-found", d.Reason);
    }
}
