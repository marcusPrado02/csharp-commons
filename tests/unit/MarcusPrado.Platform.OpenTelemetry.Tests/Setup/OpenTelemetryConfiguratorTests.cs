namespace MarcusPrado.Platform.OpenTelemetry.Tests.Setup;

public sealed class OpenTelemetryConfiguratorTests
{
    [Fact]
    public void AddPlatformOpenTelemetry_RegistersServices()
    {
        var services = new ServiceCollection();

        services.AddPlatformOpenTelemetry(opts =>
        {
            opts.ServiceName = "test-svc";
            opts.UseOtlpExporter = false;
        });

        var provider = services.BuildServiceProvider();

        // If no exceptions are thrown, the DI registration succeeded
        Assert.NotNull(provider);
    }

    [Fact]
    public void AddPlatformOpenTelemetry_WithCustomSource_Succeeds()
    {
        var services = new ServiceCollection();

        services.AddPlatformOpenTelemetry(opts =>
        {
            opts.UseOtlpExporter = false;
            opts.AdditionalActivitySources = new[] { "MyApp.CustomSource" };
        });

        // Should not throw
        var provider = services.BuildServiceProvider();
        Assert.NotNull(provider);
    }
}
