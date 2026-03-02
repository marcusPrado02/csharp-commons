namespace MarcusPrado.Platform.FeatureFlags.Tests.Flags;

public sealed class FeatureFlagTests
{
    [Fact]
    public void NewFlag_HasEmptyWhitelists()
    {
        var flag = new FeatureFlag { Key = "test" };
        Assert.Empty(flag.TenantWhitelist);
        Assert.Empty(flag.UserWhitelist);
        Assert.Empty(flag.Variants);
    }

    [Fact]
    public void FeatureVariantOn_IsEnabled()
    {
        Assert.Equal("on", FeatureVariant.On.Key);
        Assert.Equal(100, FeatureVariant.On.Weight);
    }

    [Fact]
    public void FeatureVariantOff_IsDisabled()
    {
        Assert.Equal("off", FeatureVariant.Off.Key);
        Assert.Equal(0, FeatureVariant.Off.Weight);
    }
}
