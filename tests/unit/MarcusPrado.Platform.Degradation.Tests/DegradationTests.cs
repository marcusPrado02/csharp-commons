using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using MarcusPrado.Platform.Degradation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MarcusPrado.Platform.Degradation.Tests;

public sealed class InMemoryDegradationControllerTests
{
    // ── GetModeAsync ──────────────────────────────────────────────────────────

    [Fact]
    public async Task GetModeAsync_DefaultMode_ReturnsNone()
    {
        var controller = new InMemoryDegradationController();

        var mode = await controller.GetModeAsync();

        mode.Should().Be(DegradationMode.None);
    }

    // ── SetModeAsync / GetModeAsync round-trip ────────────────────────────────

    [Theory]
    [InlineData(DegradationMode.None)]
    [InlineData(DegradationMode.PartiallyDegraded)]
    [InlineData(DegradationMode.ReadOnly)]
    [InlineData(DegradationMode.Maintenance)]
    public async Task SetModeAsync_ThenGet_ReturnsSameMode(DegradationMode expected)
    {
        var controller = new InMemoryDegradationController();

        await controller.SetModeAsync(expected);
        var actual = await controller.GetModeAsync();

        actual.Should().Be(expected);
    }

    [Fact]
    public async Task SetModeAsync_CalledTwice_ReturnsLastValue()
    {
        var controller = new InMemoryDegradationController();

        await controller.SetModeAsync(DegradationMode.Maintenance);
        await controller.SetModeAsync(DegradationMode.ReadOnly);

        var mode = await controller.GetModeAsync();
        mode.Should().Be(DegradationMode.ReadOnly);
    }
}

public sealed class DegradationMiddlewareTests
{
    // ── None mode ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task NoneMode_GetRequest_PassesThrough()
    {
        using var server = BuildServer(DegradationMode.None);
        var client = server.CreateClient();

        var response = await client.GetAsync("/ping");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    // ── Maintenance mode ──────────────────────────────────────────────────────

    [Fact]
    public async Task MaintenanceMode_AnyRequest_Returns503()
    {
        using var server = BuildServer(DegradationMode.Maintenance);
        var client = server.CreateClient();

        var response = await client.GetAsync("/ping");

        response.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
    }

    [Fact]
    public async Task MaintenanceMode_ResponseBody_IsJson()
    {
        using var server = BuildServer(DegradationMode.Maintenance);
        var client = server.CreateClient();

        var response = await client.GetAsync("/ping");
        var body = await response.Content.ReadAsStringAsync();

        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");
        body.Should().Contain("503");
    }

    // ── ReadOnly mode ─────────────────────────────────────────────────────────

    [Fact]
    public async Task ReadOnlyMode_GetRequest_PassesThrough()
    {
        using var server = BuildServer(DegradationMode.ReadOnly);
        var client = server.CreateClient();

        var response = await client.GetAsync("/ping");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Theory]
    [InlineData("POST")]
    [InlineData("PUT")]
    [InlineData("DELETE")]
    [InlineData("PATCH")]
    public async Task ReadOnlyMode_WriteMethod_Returns405(string method)
    {
        using var server = BuildServer(DegradationMode.ReadOnly);
        var client = server.CreateClient();

        var request = new HttpRequestMessage(new HttpMethod(method), "/ping")
        {
            Content = new StringContent("{}", Encoding.UTF8, "application/json"),
        };
        var response = await client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.MethodNotAllowed);
    }

    // ── PartiallyDegraded mode ────────────────────────────────────────────────

    [Fact]
    public async Task PartiallyDegradedMode_GetRequest_PassesThrough()
    {
        using var server = BuildServer(DegradationMode.PartiallyDegraded);
        var client = server.CreateClient();

        var response = await client.GetAsync("/ping");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task PartiallyDegradedMode_AddsXDegradationModeHeader()
    {
        using var server = BuildServer(DegradationMode.PartiallyDegraded);
        var client = server.CreateClient();

        var response = await client.GetAsync("/ping");

        response.Headers.Should().ContainKey("X-Degradation-Mode");
        response.Headers.GetValues("X-Degradation-Mode").Should().Contain("PartiallyDegraded");
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static TestServer BuildServer(DegradationMode initialMode)
    {
        var builder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services.AddRouting();
                var ctrl = new InMemoryDegradationController();
                ctrl.SetModeAsync(initialMode).GetAwaiter().GetResult();
                services.AddSingleton<IDegradationController>(ctrl);
            })
            .Configure(app =>
            {
                app.UsePlatformDegradation();
                app.UseRouting();
                app.UseEndpoints(e =>
                {
                    e.MapGet("/ping", () => Results.Ok(new { pong = true }));
                });
            });

        return new TestServer(builder);
    }
}

public sealed class DegradationEndpointsTests
{
    // ── GET /degradation/status ───────────────────────────────────────────────

    [Fact]
    public async Task GetStatus_ReturnsCurrentMode_AsJson()
    {
        using var server = BuildServer(DegradationMode.ReadOnly);
        var client = server.CreateClient();

        var response = await client.GetAsync("/degradation/status");
        var body = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        body.Should().Contain("ReadOnly");
    }

    // ── POST /degradation/mode ────────────────────────────────────────────────

    [Fact]
    public async Task PostMode_ValidMode_SetsAndReturnsMode()
    {
        using var server = BuildServer(DegradationMode.None);
        var client = server.CreateClient();

        var content = new StringContent(
            """{"mode":"Maintenance"}""", Encoding.UTF8, "application/json");
        var response = await client.PostAsync("/degradation/mode", content);
        var body = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        body.Should().Contain("Maintenance");
    }

    [Fact]
    public async Task PostMode_InvalidMode_Returns400()
    {
        using var server = BuildServer(DegradationMode.None);
        var client = server.CreateClient();

        var content = new StringContent(
            """{"mode":"UnknownMode"}""", Encoding.UTF8, "application/json");
        var response = await client.PostAsync("/degradation/mode", content);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PostMode_ChangePersists_VisibleViaGetStatus()
    {
        using var server = BuildServer(DegradationMode.None);
        var client = server.CreateClient();

        // set mode to PartiallyDegraded
        await client.PostAsync("/degradation/mode",
            new StringContent("""{"mode":"PartiallyDegraded"}""", Encoding.UTF8, "application/json"));

        var response = await client.GetAsync("/degradation/status");
        var body = await response.Content.ReadAsStringAsync();

        body.Should().Contain("PartiallyDegraded");
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static TestServer BuildServer(DegradationMode initialMode)
    {
        var builder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services.AddRouting();
                var ctrl = new InMemoryDegradationController();
                ctrl.SetModeAsync(initialMode).GetAwaiter().GetResult();
                services.AddSingleton<IDegradationController>(ctrl);
            })
            .Configure(app =>
            {
                app.UseRouting();
                app.UseEndpoints(e => e.MapDegradationEndpoints());
            });

        return new TestServer(builder);
    }
}
