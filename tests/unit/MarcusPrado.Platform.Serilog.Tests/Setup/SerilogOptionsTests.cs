namespace MarcusPrado.Platform.Serilog.Tests.Setup;

public sealed class SerilogOptionsTests
{
    [Fact]
    public void Defaults_AreReasonable()
    {
        var opts = new SerilogOptions();
        Assert.Equal("platform-service", opts.ApplicationName);
        Assert.Equal("development", opts.Environment);
        Assert.True(opts.UseColoredConsole);
        Assert.False(opts.UseJsonOutput);
        Assert.Equal("Information", opts.MinimumLevel);
    }

    [Fact]
    public void ExcludedPaths_ContainsHealth()
    {
        var opts = new SerilogOptions();
        Assert.Contains("/health", opts.ExcludedPaths);
    }

    [Fact]
    public void ApplicationName_CanBeOverridden()
    {
        var opts = new SerilogOptions { ApplicationName = "my-svc" };
        Assert.Equal("my-svc", opts.ApplicationName);
    }
}
