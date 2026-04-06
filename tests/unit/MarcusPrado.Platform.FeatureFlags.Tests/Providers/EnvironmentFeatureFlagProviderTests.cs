namespace MarcusPrado.Platform.FeatureFlags.Tests.Providers;

public sealed class EnvironmentFeatureFlagProviderTests : IDisposable
{
    private readonly EnvironmentFeatureFlagProvider _provider = new();
    private readonly List<string> _envVarsSet = new();

    private void SetEnv(string key, string value)
    {
        Environment.SetEnvironmentVariable(key, value);
        _envVarsSet.Add(key);
    }

    public void Dispose()
    {
        foreach (var key in _envVarsSet)
        {
            Environment.SetEnvironmentVariable(key, null);
        }
    }

    [Fact]
    public async Task UnsetEnv_ReturnsNotFound()
    {
        var d = await _provider.EvaluateAsync("no-such-flag", FeatureFlagContext.Anonymous);
        Assert.Equal("flag-not-found", d.Reason);
    }

    [Fact]
    public async Task EnvTrue_ReturnsEnabled()
    {
        SetEnv("FEATURE__ENV_BOOL_FLAG", "true");
        var d = await _provider.EvaluateAsync("env-bool-flag", FeatureFlagContext.Anonymous);
        Assert.True(d.IsEnabled);
    }

    [Fact]
    public async Task EnvFalse_ReturnsDisabled()
    {
        SetEnv("FEATURE__ENV_FALSE_FLAG", "false");
        var d = await _provider.EvaluateAsync("env-false-flag", FeatureFlagContext.Anonymous);
        Assert.False(d.IsEnabled);
    }

    [Fact]
    public async Task Env100Pct_ReturnsEnabled()
    {
        SetEnv("FEATURE__ENV_PCT_FLAG", "100");
        var d = await _provider.EvaluateAsync("env-pct-flag", FeatureFlagContext.Anonymous);
        Assert.True(d.IsEnabled);
    }

    [Fact]
    public async Task Env0Pct_ReturnsDisabled()
    {
        SetEnv("FEATURE__ENV_ZERO_PCT_FLAG", "0");
        var d = await _provider.EvaluateAsync("env-zero-pct-flag", FeatureFlagContext.Anonymous);
        Assert.False(d.IsEnabled);
    }

    [Fact]
    public async Task EnvInvalidValue_ReturnsDisabled()
    {
        SetEnv("FEATURE__ENV_INVALID_FLAG", "not-a-boolean");
        var d = await _provider.EvaluateAsync("env-invalid-flag", FeatureFlagContext.Anonymous);
        Assert.False(d.IsEnabled);
    }
}
