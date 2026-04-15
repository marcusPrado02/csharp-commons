namespace MarcusPrado.Platform.FeatureFlags.Tests.Providers;

public sealed class InMemoryFeatureFlagProviderTests
{
    private readonly InMemoryFeatureFlagProvider _provider = new();
    private readonly FeatureFlagContext _ctx = FeatureFlagContext.Anonymous;

    [Fact]
    public async Task UnknownFlag_ReturnsNotFound()
    {
        var d = await _provider.EvaluateAsync("unknown", _ctx);
        Assert.Equal("flag-not-found", d.Reason);
    }

    [Fact]
    public async Task DisabledFlag_ReturnsDisabled()
    {
        _provider.SetFlag(new FeatureFlag { Key = "off", Enabled = false });
        var d = await _provider.EvaluateAsync("off", _ctx);
        Assert.False(d.IsEnabled);
    }

    [Fact]
    public async Task EnabledBooleanFlag_ReturnsEnabled()
    {
        _provider.SetFlag(new FeatureFlag { Key = "on", Enabled = true });
        var d = await _provider.EvaluateAsync("on", _ctx);
        Assert.True(d.IsEnabled);
    }

    [Fact]
    public async Task IsEnabledAsync_ReturnsBool()
    {
        _provider.SetFlag(new FeatureFlag { Key = "feat", Enabled = true });
        Assert.True(await _provider.IsEnabledAsync("feat", _ctx));
    }

    [Fact]
    public async Task TenantWhitelist_EnablesMatchingTenant()
    {
        _provider.SetFlag(
            new FeatureFlag
            {
                Key = "twl",
                Enabled = true,
                Strategy = RolloutStrategy.TenantWhitelist,
                TenantWhitelist = new HashSet<string> { "tenant-1" },
            }
        );

        var ctx = new FeatureFlagContext { TenantId = "tenant-1" };
        Assert.True(await _provider.IsEnabledAsync("twl", ctx));
    }

    [Fact]
    public async Task TenantWhitelist_DisablesNonMatchingTenant()
    {
        _provider.SetFlag(
            new FeatureFlag
            {
                Key = "twl2",
                Enabled = true,
                Strategy = RolloutStrategy.TenantWhitelist,
                TenantWhitelist = new HashSet<string> { "tenant-1" },
            }
        );

        var ctx = new FeatureFlagContext { TenantId = "tenant-99" };
        Assert.False(await _provider.IsEnabledAsync("twl2", ctx));
    }

    [Fact]
    public async Task UserWhitelist_EnablesMatchingUser()
    {
        _provider.SetFlag(
            new FeatureFlag
            {
                Key = "uwl",
                Enabled = true,
                Strategy = RolloutStrategy.UserWhitelist,
                UserWhitelist = new HashSet<string> { "user-abc" },
            }
        );

        var ctx = new FeatureFlagContext { UserId = "user-abc" };
        Assert.True(await _provider.IsEnabledAsync("uwl", ctx));
    }

    [Fact]
    public async Task PercentageZero_AlwaysDisabled()
    {
        _provider.SetFlag(
            new FeatureFlag
            {
                Key = "pct0",
                Enabled = true,
                Strategy = RolloutStrategy.Percentage,
                Percentage = 0,
            }
        );

        Assert.False(await _provider.IsEnabledAsync("pct0", _ctx));
    }

    [Fact]
    public async Task PercentageFull_AlwaysEnabled()
    {
        _provider.SetFlag(
            new FeatureFlag
            {
                Key = "pct100",
                Enabled = true,
                Strategy = RolloutStrategy.Percentage,
                Percentage = 100,
            }
        );

        Assert.True(await _provider.IsEnabledAsync("pct100", _ctx));
    }

    [Fact]
    public async Task RemoveFlag_ReturnsNotFound()
    {
        _provider.SetFlag(new FeatureFlag { Key = "del", Enabled = true });
        _provider.RemoveFlag("del");
        var d = await _provider.EvaluateAsync("del", _ctx);
        Assert.Equal("flag-not-found", d.Reason);
    }

    [Fact]
    public async Task UserWhitelist_DisablesNonMatchingUser()
    {
        _provider.SetFlag(
            new FeatureFlag
            {
                Key = "uwl2",
                Enabled = true,
                Strategy = RolloutStrategy.UserWhitelist,
                UserWhitelist = new HashSet<string> { "user-abc" },
            }
        );

        var ctx = new FeatureFlagContext { UserId = "user-xyz" };
        Assert.False(await _provider.IsEnabledAsync("uwl2", ctx));
    }

    [Fact]
    public async Task CanaryStrategy_EnablesUserInBucket()
    {
        // "canary-flag":"user-0" => deterministic bucket 5, within 20%
        _provider.SetFlag(
            new FeatureFlag
            {
                Key = "canary-flag",
                Enabled = true,
                Strategy = RolloutStrategy.Canary,
                Percentage = 20,
            }
        );

        var ctx = new FeatureFlagContext { UserId = "user-0" };
        Assert.True(await _provider.IsEnabledAsync("canary-flag", ctx));
    }

    [Fact]
    public async Task CanaryStrategy_DisablesUserOutsideBucket()
    {
        // "canary-flag":"user-10" => deterministic bucket 94, outside 20%
        _provider.SetFlag(
            new FeatureFlag
            {
                Key = "canary-flag",
                Enabled = true,
                Strategy = RolloutStrategy.Canary,
                Percentage = 20,
            }
        );

        var ctx = new FeatureFlagContext { UserId = "user-10" };
        Assert.False(await _provider.IsEnabledAsync("canary-flag", ctx));
    }

    [Fact]
    public async Task MultipleFlags_EvaluatedIndependently()
    {
        _provider.SetFlag(new FeatureFlag { Key = "flag-a", Enabled = true });
        _provider.SetFlag(new FeatureFlag { Key = "flag-b", Enabled = false });
        _provider.SetFlag(
            new FeatureFlag
            {
                Key = "flag-c",
                Enabled = true,
                Strategy = RolloutStrategy.Percentage,
                Percentage = 100,
            }
        );

        Assert.True(await _provider.IsEnabledAsync("flag-a", _ctx));
        Assert.False(await _provider.IsEnabledAsync("flag-b", _ctx));
        Assert.True(await _provider.IsEnabledAsync("flag-c", _ctx));
    }
}
