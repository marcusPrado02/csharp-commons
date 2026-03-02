namespace MarcusPrado.Platform.FeatureFlags.Tests;

public sealed class FeatureFlagServiceTests
{
    private readonly InMemoryFeatureFlagProvider _provider = new();
    private readonly FeatureFlagService _service;

    public FeatureFlagServiceTests()
    {
        _service = new FeatureFlagService(_provider);
    }

    [Fact]
    public async Task IsEnabledAsync_WhenEnabled_ReturnsTrue()
    {
        _provider.SetFlag(new FeatureFlag { Key = "svc-flag", Enabled = true });
        Assert.True(await _service.IsEnabledAsync("svc-flag"));
    }

    [Fact]
    public async Task IsEnabledAsync_WhenNotFound_ReturnsFalse()
    {
        Assert.False(await _service.IsEnabledAsync("no-such"));
    }

    [Fact]
    public async Task EvaluateAsync_ReturnsFullDecision()
    {
        _provider.SetFlag(new FeatureFlag { Key = "e2e", Enabled = true });
        var d = await _service.EvaluateAsync("e2e");
        Assert.True(d.IsEnabled);
        Assert.Equal("e2e", d.FlagKey);
    }

    [Fact]
    public async Task IsEnabledAsync_NullContext_UsesAnonymous()
    {
        _provider.SetFlag(new FeatureFlag { Key = "anon", Enabled = true });
        Assert.True(await _service.IsEnabledAsync("anon", context: null));
    }
}
