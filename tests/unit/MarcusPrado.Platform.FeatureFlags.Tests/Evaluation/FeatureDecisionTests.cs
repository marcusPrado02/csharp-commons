namespace MarcusPrado.Platform.FeatureFlags.Tests.Evaluation;

public sealed class FeatureDecisionTests
{
    [Fact]
    public void Enabled_ReturnsEnabledDecision()
    {
        var d = FeatureDecision.Enabled("my-flag", "boolean");
        Assert.True(d.IsEnabled);
        Assert.Equal("my-flag", d.FlagKey);
    }

    [Fact]
    public void Disabled_ReturnsDisabledDecision()
    {
        var d = FeatureDecision.Disabled("my-flag", "flag-disabled");
        Assert.False(d.IsEnabled);
    }

    [Fact]
    public void NotFound_ReturnsFlagNotFoundReason()
    {
        var d = FeatureDecision.NotFound("x");
        Assert.False(d.IsEnabled);
        Assert.Equal("flag-not-found", d.Reason);
    }

    [Fact]
    public void Enabled_WithVariant_StoresVariant()
    {
        var v = FeatureVariant.On;
        var d = FeatureDecision.Enabled("f", "test", v);
        Assert.Same(v, d.Variant);
    }
}
