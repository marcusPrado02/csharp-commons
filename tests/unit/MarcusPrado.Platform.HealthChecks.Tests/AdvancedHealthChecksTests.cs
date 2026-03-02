using MarcusPrado.Platform.HealthChecks.Checks;
using FluentAssertions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

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
}
