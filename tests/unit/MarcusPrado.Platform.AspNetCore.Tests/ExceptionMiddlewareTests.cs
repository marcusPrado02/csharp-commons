using System.Net;
using System.Text.Json;
using MarcusPrado.Platform.AspNetCore.Tests.Helpers;

namespace MarcusPrado.Platform.AspNetCore.Tests;

/// <summary>
/// Tests that <see cref="MarcusPrado.Platform.AspNetCore.Middleware.ExceptionMiddleware"/>
/// converts unhandled exceptions into RFC 9457-compliant ProblemDetails responses.
/// </summary>
public sealed class ExceptionMiddlewareTests
{
    // ── Status code mapping ───────────────────────────────────────────────────

    [Fact]
    public async Task NotFoundException_ShouldProduce_404_ProblemDetails()
    {
        using var server = PlatformTestServer.Create();
        using var client = server.CreateClient();

        var response = await client.GetAsync("/error/notfound");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        response.Content.Headers.ContentType?.MediaType
            .Should().Be("application/problem+json");
        using var doc = await PlatformTestServer.ReadJsonAsync(response);
        doc.RootElement.GetProperty("status").GetInt32().Should().Be(404);
    }

    [Fact]
    public async Task ConflictException_ShouldProduce_409_ProblemDetails()
    {
        using var server = PlatformTestServer.Create();
        using var client = server.CreateClient();

        var response = await client.GetAsync("/error/conflict");

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        using var doc = await PlatformTestServer.ReadJsonAsync(response);
        doc.RootElement.GetProperty("status").GetInt32().Should().Be(409);
    }

    [Fact]
    public async Task UnauthorizedException_ShouldProduce_401_ProblemDetails()
    {
        using var server = PlatformTestServer.Create();
        using var client = server.CreateClient();

        var response = await client.GetAsync("/error/unauth");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        using var doc = await PlatformTestServer.ReadJsonAsync(response);
        doc.RootElement.GetProperty("status").GetInt32().Should().Be(401);
    }

    [Fact]
    public async Task ForbiddenException_ShouldProduce_403_ProblemDetails()
    {
        using var server = PlatformTestServer.Create();
        using var client = server.CreateClient();

        var response = await client.GetAsync("/error/forbidden");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        using var doc = await PlatformTestServer.ReadJsonAsync(response);
        doc.RootElement.GetProperty("status").GetInt32().Should().Be(403);
    }

    [Fact]
    public async Task ValidationException_ShouldProduce_422_ProblemDetails_WithFieldErrors()
    {
        using var server = PlatformTestServer.Create();
        using var client = server.CreateClient();

        var response = await client.GetAsync("/error/validation");

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        using var doc = await PlatformTestServer.ReadJsonAsync(response);
        var root = doc.RootElement;
        root.GetProperty("status").GetInt32().Should().Be(422);
        root.TryGetProperty("errors", out _).Should().BeTrue(
            because: "ValidationException errors must be included in the ProblemDetails extensions");
    }

    [Fact]
    public async Task UnhandledException_ShouldProduce_500_ProblemDetails_WithoutLeakingDetails()
    {
        using var server = PlatformTestServer.Create();
        using var client = server.CreateClient();

        var response = await client.GetAsync("/error/unhandled");

        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        using var doc = await PlatformTestServer.ReadJsonAsync(response);
        var root = doc.RootElement;
        root.GetProperty("status").GetInt32().Should().Be(500);
        root.GetProperty("title").GetString().Should().Be("Internal Server Error");
    }

    // ── RFC 9457 compliance ───────────────────────────────────────────────────

    [Fact]
    public async Task ProblemDetails_ShouldAlwaysContain_Type_Title_Status_Fields()
    {
        using var server = PlatformTestServer.Create();
        using var client = server.CreateClient();

        var response = await client.GetAsync("/error/notfound");

        using var doc = await PlatformTestServer.ReadJsonAsync(response);
        var root = doc.RootElement;
        root.TryGetProperty("type",   out _).Should().BeTrue("'type' is an RFC 9457 required field");
        root.TryGetProperty("title",  out _).Should().BeTrue("'title' is an RFC 9457 required field");
        root.TryGetProperty("status", out _).Should().BeTrue("'status' is an RFC 9457 required field");
    }
}
