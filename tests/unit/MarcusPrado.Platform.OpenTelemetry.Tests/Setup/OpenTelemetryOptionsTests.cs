namespace MarcusPrado.Platform.OpenTelemetry.Tests.Setup;

public sealed class OpenTelemetryOptionsTests
{
    [Fact]
    public void Defaults_AreReasonable()
    {
        var opts = new OpenTelemetryOptions();
        Assert.Equal("platform-service", opts.ServiceName);
        Assert.Equal("0.0.0", opts.ServiceVersion);
        Assert.Equal("http://localhost:4317", opts.OtlpEndpoint);
        Assert.True(opts.UseOtlpExporter);
        Assert.True(opts.InstrumentAspNetCore);
        Assert.True(opts.InstrumentHttpClient);
    }

    [Fact]
    public void AdditionalActivitySources_DefaultsEmpty()
    {
        var opts = new OpenTelemetryOptions();
        Assert.Empty(opts.AdditionalActivitySources);
    }

    [Fact]
    public void ServiceName_CanBeOverridden()
    {
        var opts = new OpenTelemetryOptions { ServiceName = "my-svc" };
        Assert.Equal("my-svc", opts.ServiceName);
    }
}
