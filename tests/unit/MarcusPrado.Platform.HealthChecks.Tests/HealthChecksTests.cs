using System.Net;
using FluentAssertions;
using MarcusPrado.Platform.HealthChecks.Checks;
using MarcusPrado.Platform.HealthChecks.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using NSubstitute;
using Xunit;

namespace MarcusPrado.Platform.HealthChecks.Tests;

public sealed class HealthChecksTests
{
    // ── LivenessCheck ─────────────────────────────────────────────────────────

    [Fact]
    public async Task LivenessCheck_AlwaysReturnsHealthy()
    {
        var check = new LivenessCheck();
        var ctx = new HealthCheckContext();

        var result = await check.CheckHealthAsync(ctx);

        result.Status.Should().Be(HealthStatus.Healthy);
    }

    [Fact]
    public async Task LivenessCheck_MessageIsInformative()
    {
        var check = new LivenessCheck();
        var result = await check.CheckHealthAsync(new HealthCheckContext());

        result.Description.Should().NotBeNullOrWhiteSpace();
    }

    // ── ReadinessCheck ────────────────────────────────────────────────────────

    [Fact]
    public async Task ReadinessCheck_NoProbes_ReturnsHealthy()
    {
        var check = new ReadinessCheck([]);
        var result = await check.CheckHealthAsync(new HealthCheckContext());

        result.Status.Should().Be(HealthStatus.Healthy);
    }

    [Fact]
    public async Task ReadinessCheck_AllProbesHealthy_ReturnsHealthy()
    {
        var p1 = Substitute.For<IDependencyHealthProbe>();
        var p2 = Substitute.For<IDependencyHealthProbe>();
        p1.CheckAsync(Arg.Any<CancellationToken>()).Returns(Task.FromResult<(bool, string)>((true, "ok")));
        p2.CheckAsync(Arg.Any<CancellationToken>()).Returns(Task.FromResult<(bool, string)>((true, "ok")));

        var result = await new ReadinessCheck([p1, p2])
            .CheckHealthAsync(new HealthCheckContext());

        result.Status.Should().Be(HealthStatus.Healthy);
    }

    [Fact]
    public async Task ReadinessCheck_OneUnhealthyProbe_ReturnsUnhealthy()
    {
        var good = Substitute.For<IDependencyHealthProbe>();
        var bad = Substitute.For<IDependencyHealthProbe>();
        good.Name.Returns("cache");
        bad.Name.Returns("postgres");
        good.CheckAsync(Arg.Any<CancellationToken>()).Returns(Task.FromResult<(bool, string)>((true, "ok")));
        bad.CheckAsync(Arg.Any<CancellationToken>()).Returns(Task.FromResult<(bool, string)>((false, "connection refused")));

        var result = await new ReadinessCheck([good, bad])
            .CheckHealthAsync(new HealthCheckContext());

        result.Status.Should().Be(HealthStatus.Unhealthy);
        result.Description.Should().Contain("postgres");
        result.Description.Should().Contain("connection refused");
    }

    [Fact]
    public async Task ReadinessCheck_ProbeThrows_ReturnsUnhealthy()
    {
        var faulted = Substitute.For<IDependencyHealthProbe>();
        faulted.Name.Returns("kafka");
        faulted.CheckAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromException<(bool, string)>(new InvalidOperationException("broker down")));

        var result = await new ReadinessCheck([faulted])
            .CheckHealthAsync(new HealthCheckContext());

        result.Status.Should().Be(HealthStatus.Unhealthy);
        result.Description.Should().Contain("kafka");
        result.Description.Should().Contain("broker down");
    }

    [Fact]
    public async Task ReadinessCheck_MultipleFailures_AllNamesInMessage()
    {
        var p1 = Substitute.For<IDependencyHealthProbe>();
        var p2 = Substitute.For<IDependencyHealthProbe>();
        p1.Name.Returns("db1");
        p2.Name.Returns("db2");
        p1.CheckAsync(Arg.Any<CancellationToken>()).Returns(Task.FromResult<(bool, string)>((false, "timeout")));
        p2.CheckAsync(Arg.Any<CancellationToken>()).Returns(Task.FromResult<(bool, string)>((false, "timeout")));

        var result = await new ReadinessCheck([p1, p2])
            .CheckHealthAsync(new HealthCheckContext());

        result.Status.Should().Be(HealthStatus.Unhealthy);
        result.Description.Should().Contain("db1");
        result.Description.Should().Contain("db2");
    }

    // ── DI registration ───────────────────────────────────────────────────────

    [Fact]
    public void AddPlatformHealthChecks_RegistersLivenessAndReadiness()
    {
        var services = new ServiceCollection()
            .AddLogging();

        services.AddPlatformHealthChecks();

        var sp = services.BuildServiceProvider();

        // The health check service is resolvable
        var hcService = sp.GetService<HealthCheckService>();
        hcService.Should().NotBeNull();
    }

    // ── HTTP endpoints ────────────────────────────────────────────────────────

    [Fact]
    public async Task LiveEndpoint_Returns200()
    {
        using var host = BuildTestHost(includeDetail: false);
        var client = host.CreateClient();

        var response = await client.GetAsync("/health/live");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ReadyEndpoint_NoProbes_Returns200()
    {
        using var host = BuildTestHost(includeDetail: false);
        var client = host.CreateClient();

        var response = await client.GetAsync("/health/ready");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task DetailEndpoint_WhenEnabled_Returns200WithJson()
    {
        using var host = BuildTestHost(includeDetail: true);
        var client = host.CreateClient();

        var response = await client.GetAsync("/health/detail");
        var body = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        body.Should().Contain("status");
    }

    [Fact]
    public async Task DetailEndpoint_WhenDisabled_Returns404()
    {
        using var host = BuildTestHost(includeDetail: false);
        var client = host.CreateClient();

        var response = await client.GetAsync("/health/detail");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static TestServer BuildTestHost(bool includeDetail)
    {
        var builder = new WebHostBuilder()
            .ConfigureServices(s =>
            {
                s.AddLogging();
                s.AddPlatformHealthChecks();
                s.AddRouting();
            })
            .Configure(app =>
            {
                app.UseRouting();
                app.UsePlatformHealthChecks(includeDetail);
            });

        return new TestServer(builder);
    }
}
