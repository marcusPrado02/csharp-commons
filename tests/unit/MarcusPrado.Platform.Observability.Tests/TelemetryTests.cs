namespace MarcusPrado.Platform.Observability.Tests;

public sealed class TelemetryTests
{
    // Test 1: PlatformMeter RequestCount is not null
    [Fact]
    public void PlatformMeter_RequestCount_IsNotNull()
    {
        using var meter = new PlatformMeter("test-service");
        meter.RequestCount.Should().NotBeNull();
    }

    // Test 2: PlatformMeter RequestDuration histogram is not null
    [Fact]
    public void PlatformMeter_RequestDuration_IsNotNull()
    {
        using var meter = new PlatformMeter("test-service");
        meter.RequestDuration.Should().NotBeNull();
    }

    // Test 3: PlatformMeter.MeterName constant
    [Fact]
    public void PlatformMeter_MeterName_IsExpectedValue()
    {
        PlatformMeter.MeterName.Should().Be("MarcusPrado.Platform");
    }

    // Test 4: TelemetryOptions defaults — EnableTracing, EnableMetrics, EnableLogs all true
    [Fact]
    public void TelemetryOptions_Defaults_AllSignalsEnabled()
    {
        var opts = new TelemetryOptions();
        opts.EnableTracing.Should().BeTrue();
        opts.EnableMetrics.Should().BeTrue();
        opts.EnableLogs.Should().BeTrue();
    }

    // Test 5: TelemetryOptions.ServiceName default is not empty
    [Fact]
    public void TelemetryOptions_ServiceName_DefaultIsNotEmpty()
    {
        var opts = new TelemetryOptions();
        opts.ServiceName.Should().NotBeNullOrEmpty();
    }

    // Test 6: AddPlatformTelemetry registers PlatformMeter in DI
    [Fact]
    public void AddPlatformTelemetry_RegistersPlatformMeter()
    {
        var services = new ServiceCollection();
        services.AddPlatformTelemetry(opts =>
        {
            opts.ServiceName = "test-svc";
        });

        using var provider = services.BuildServiceProvider();
        var meter = provider.GetService<PlatformMeter>();
        meter.Should().NotBeNull();
    }

    // Test 7: OtelHealthCheckPublisher.PublishAsync does not throw on valid input
    [Fact]
    public async Task OtelHealthCheckPublisher_PublishAsync_DoesNotThrow()
    {
        using var publisher = new OtelHealthCheckPublisher();
        var report = new HealthReport(new Dictionary<string, HealthReportEntry>(), HealthStatus.Healthy, TimeSpan.Zero);

        var act = async () => await publisher.PublishAsync(report, CancellationToken.None);
        await act.Should().NotThrowAsync();
    }

    // Test 8: PlatformMeter has all expected instrument properties
    [Fact]
    public void PlatformMeter_AllInstruments_AreNotNull()
    {
        using var meter = new PlatformMeter("test-service");
        meter.ActiveRequests.Should().NotBeNull();
        meter.ErrorCount.Should().NotBeNull();
        meter.CacheHitCount.Should().NotBeNull();
        meter.CacheMissCount.Should().NotBeNull();
    }

    // Test 9: AddPlatformTelemetry registers TelemetryOptions in DI
    [Fact]
    public void AddPlatformTelemetry_RegistersTelemetryOptions()
    {
        var services = new ServiceCollection();
        services.AddPlatformTelemetry(opts => opts.ServiceName = "my-svc");

        using var provider = services.BuildServiceProvider();
        var opts = provider.GetService<TelemetryOptions>();
        opts.Should().NotBeNull();
        opts!.ServiceName.Should().Be("my-svc");
    }
}
