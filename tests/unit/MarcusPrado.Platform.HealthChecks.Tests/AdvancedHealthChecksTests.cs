using MarcusPrado.Platform.HealthChecks.Checks;
using MarcusPrado.Platform.HealthChecks.Extensions;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace MarcusPrado.Platform.HealthChecks.Tests;

public sealed class AdvancedHealthChecksTests
{
    // ── MemoryPressureHealthCheck ────────────────────────────────────────────

    [Fact]
    public async Task MemoryPressure_ThresholdsVeryHigh_ReturnsHealthy()
    {
        var check = MakeMemoryCheck(degraded: long.MaxValue, unhealthy: long.MaxValue);

        var result = await check.CheckHealthAsync(new HealthCheckContext());

        result.Status.Should().Be(HealthStatus.Healthy);
    }

    [Fact]
    public async Task MemoryPressure_DegradedThreshold_1Byte_ReturnsDegraded()
    {
        var check = MakeMemoryCheck(degraded: 1, unhealthy: long.MaxValue);

        var result = await check.CheckHealthAsync(new HealthCheckContext());

        result.Status.Should().Be(HealthStatus.Degraded);
    }

    [Fact]
    public async Task MemoryPressure_UnhealthyThreshold_1Byte_ReturnsUnhealthy()
    {
        var check = MakeMemoryCheck(degraded: 1, unhealthy: 1);

        var result = await check.CheckHealthAsync(new HealthCheckContext());

        result.Status.Should().Be(HealthStatus.Unhealthy);
    }

    [Fact]
    public async Task MemoryPressure_ContainsAllocatedBytesInData()
    {
        var check = MakeMemoryCheck(degraded: long.MaxValue, unhealthy: long.MaxValue);

        var result = await check.CheckHealthAsync(new HealthCheckContext());

        result.Data.Should().ContainKey("allocated_bytes");
    }

    // ── ThreadPoolStarvationHealthCheck ──────────────────────────────────────

    [Fact]
    public async Task ThreadPool_ZeroThresholds_ReturnsHealthy()
    {
        var check = MakeThreadPoolCheck(degraded: 0, unhealthy: 0);

        var result = await check.CheckHealthAsync(new HealthCheckContext());

        result.Status.Should().Be(HealthStatus.Healthy);
    }

    [Fact]
    public async Task ThreadPool_ContainsThreadDataInResult()
    {
        var check = MakeThreadPoolCheck(degraded: 0, unhealthy: 0);

        var result = await check.CheckHealthAsync(new HealthCheckContext());

        result.Data.Should().ContainKey("available_worker_threads");
        result.Data.Should().ContainKey("max_worker_threads");
    }

    // ── ExternalDependencyHealthCheck ─────────────────────────────────────────

    [Fact]
    public void ExternalDependency_NullUrl_ThrowsArgumentException()
    {
        var act = () => new ExternalDependencyHealthCheck(new System.Net.Http.HttpClient(), null!);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ExternalDependency_EmptyUrl_ThrowsArgumentException()
    {
        var act = () => new ExternalDependencyHealthCheck(new System.Net.Http.HttpClient(), "  ");
        act.Should().Throw<ArgumentException>();
    }

    // ── DegradedHealthCheck ───────────────────────────────────────────────────

    [Fact]
    public async Task DegradedHealthCheck_AlwaysReturnsDegraded()
    {
        var check = new DegradedHealthCheck("maintenance mode");

        var result = await check.CheckHealthAsync(new HealthCheckContext());

        result.Status.Should().Be(HealthStatus.Degraded);
    }

    [Fact]
    public async Task DegradedHealthCheck_ReasonAppearsInDescription()
    {
        const string reason = "planned maintenance";
        var check = new DegradedHealthCheck(reason);

        var result = await check.CheckHealthAsync(new HealthCheckContext());

        result.Description.Should().Be(reason);
    }

    [Fact]
    public void DegradedHealthCheck_NullReason_ThrowsArgumentException()
    {
        var act = () => new DegradedHealthCheck(null!);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void DegradedHealthCheck_WhitespaceReason_ThrowsArgumentException()
    {
        var act = () => new DegradedHealthCheck("   ");
        act.Should().Throw<ArgumentException>();
    }

    // ── HealthCheckHistory ────────────────────────────────────────────────────

    [Fact]
    public void HealthCheckHistory_RecordAndRetrieve_ReturnsSameRecord()
    {
        var history = new HealthCheckHistory(maxHistoryPerCheck: 5);
        var record  = new HealthCheckRecord("db", HealthStatus.Healthy, DateTimeOffset.UtcNow, "ok");

        history.Record(record);

        var records = history.GetByName("db");
        records.Should().ContainSingle(r => r.Name == "db" && r.Status == HealthStatus.Healthy);
    }

    [Fact]
    public void HealthCheckHistory_CircularBuffer_TrimsOldestRecords()
    {
        var history = new HealthCheckHistory(maxHistoryPerCheck: 3);

        for (var i = 0; i < 5; i++)
            history.Record(new HealthCheckRecord("svc", HealthStatus.Healthy, DateTimeOffset.UtcNow, $"run {i}"));

        history.GetByName("svc").Should().HaveCount(3);
    }

    [Fact]
    public void HealthCheckHistory_GetAll_ReturnsAllCheckNames()
    {
        var history = new HealthCheckHistory(maxHistoryPerCheck: 5);
        history.Record(new HealthCheckRecord("alpha", HealthStatus.Healthy, DateTimeOffset.UtcNow, null));
        history.Record(new HealthCheckRecord("beta",  HealthStatus.Degraded, DateTimeOffset.UtcNow, null));

        var all = history.GetAll();

        all.Should().ContainKey("alpha");
        all.Should().ContainKey("beta");
    }

    [Fact]
    public void HealthCheckHistory_UnknownName_ReturnsEmptyList()
    {
        var history = new HealthCheckHistory();

        history.GetByName("nonexistent").Should().BeEmpty();
    }

    // ── AddDegradedHealthCheck DI registration ────────────────────────────────

    [Fact]
    public void AddDegradedHealthCheck_RegistersCheck_ServiceResolvable()
    {
        var services = new ServiceCollection().AddLogging();
        services.AddHealthChecks().AddDegradedHealthCheck("degraded-test", "reason");

        var sp = services.BuildServiceProvider();
        var hcService = sp.GetService<HealthCheckService>();

        hcService.Should().NotBeNull();
    }

    // ── /health/extended endpoint ─────────────────────────────────────────────

    [Fact]
    public async Task ExtendedHealthEndpoint_ReturnsOkWithJsonArray()
    {
        using var host = BuildTestHostWithHistory();
        var client = host.CreateClient();

        var response = await client.GetAsync("/health/extended");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().NotBeNullOrWhiteSpace();
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static MemoryPressureHealthCheck MakeMemoryCheck(long degraded, long unhealthy)
        => new(Options.Create(new MemoryPressureOptions
        {
            DegradedThresholdBytes  = degraded,
            UnhealthyThresholdBytes = unhealthy,
        }));

    private static ThreadPoolStarvationHealthCheck MakeThreadPoolCheck(int degraded, int unhealthy)
        => new(Options.Create(new ThreadPoolStarvationOptions
        {
            DegradedMinAvailableWorkers  = degraded,
            UnhealthyMinAvailableWorkers = unhealthy,
        }));

    private static TestServer BuildTestHostWithHistory()
    {
        var builder = new WebHostBuilder()
            .ConfigureServices(s =>
            {
                s.AddLogging();
                s.AddHealthCheckHistory(maxHistoryPerCheck: 5);
                s.AddRouting();
            })
            .Configure(app =>
            {
                app.UseRouting();
                app.UseEndpoints(endpoints =>
                {
                    endpoints.MapExtendedHealthEndpoint();
                });
            });

        return new TestServer(builder);
    }
}
