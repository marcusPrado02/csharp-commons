using System.Net;
using System.Net.Http;
using System.Text.Json;
using MarcusPrado.Platform.AspNetCore.RequestSizeLimiting;

namespace MarcusPrado.Platform.AspNetCore.Tests;

/// <summary>
/// Integration tests for <see cref="RequestSizeLimitMiddleware"/> using an in-process
/// <see cref="TestServer"/>. Each test builds its own <see cref="WebHostBuilder"/> inline.
/// </summary>
public sealed class RequestSizeLimitMiddlewareTests
{
    // ── Helpers ───────────────────────────────────────────────────────────────

    private static TestServer CreateServer(Action<RequestSizeLimitOptions>? configure = null)
    {
        var builder = new WebHostBuilder()
            .UseEnvironment("Test")
            .ConfigureLogging(l => l.ClearProviders())
            .ConfigureServices(services =>
            {
                services.AddPlatformRequestSizeLimit(configure);
            })
            .Configure(app =>
            {
                app.UseRequestSizeLimit();
                app.Run(async ctx =>
                {
                    // Drain the body so TestServer does not complain about unread content.
                    await ctx.Request.Body.CopyToAsync(Stream.Null);
                    ctx.Response.StatusCode = 200;
                    await ctx.Response.WriteAsync("ok");
                });
            });

        return new TestServer(builder);
    }

    // ── Tests ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task SmallBody_WithinFreeTierLimit_Returns200()
    {
        // 512 bytes is well under the 1 MB Free tier limit.
        using var server = CreateServer();
        using var client = server.CreateClient();

        var content = new ByteArrayContent(new byte[512]);
        var response = await client.PostAsync("/", content);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task BodyAtExactTierLimit_Returns200()
    {
        // Exactly at the Free limit boundary (1 048 576 bytes) → allowed.
        const long freeLimit = 1L * 1024 * 1024;
        using var server = CreateServer();
        using var client = server.CreateClient();

        var content = new ByteArrayContent(new byte[freeLimit]);
        var response = await client.PostAsync("/", content);

        response
            .StatusCode.Should()
            .Be(HttpStatusCode.OK, because: "a body exactly at the limit boundary should be permitted");
    }

    [Fact]
    public async Task BodyExceedsFreeTierLimit_Returns413WithProblemDetails()
    {
        // One byte over the 1 MB Free limit.
        const long oneByte = (1L * 1024 * 1024) + 1;
        using var server = CreateServer();
        using var client = server.CreateClient();

        var content = new ByteArrayContent(new byte[oneByte]);
        var response = await client.PostAsync("/", content);

        response.StatusCode.Should().Be(HttpStatusCode.RequestEntityTooLarge);

        var body = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);

        doc.RootElement.GetProperty("status").GetInt32().Should().Be(413);
        doc.RootElement.GetProperty("title").GetString().Should().Be("Payload Too Large");
        doc.RootElement.GetProperty("detail")
            .GetString()
            .Should()
            .Contain("Free", because: "the detail message should mention the tier name");
    }

    [Fact]
    public async Task CustomTierResolver_ProTier_AppliesProLimit()
    {
        // Pro tier = 10 MB; sending 5 MB should be allowed.
        const long fiveMb = 5L * 1024 * 1024;
        using var server = CreateServer(opts =>
        {
            // Override the resolver to always return Pro for every request.
            opts.TierResolver = _ => RequestSizeTier.Pro;
        });
        using var client = server.CreateClient();

        var content = new ByteArrayContent(new byte[fiveMb]);
        var response = await client.PostAsync("/", content);

        response.StatusCode.Should().Be(HttpStatusCode.OK, because: "5 MB is within the Pro tier 10 MB limit");
    }

    [Fact]
    public async Task ContentLengthHeaderExceedsLimit_Returns413WithoutReadingBody()
    {
        // Send a Content-Length header claiming 2 MB but an empty body.
        // The middleware must reject based on the header alone.
        const long twoMb = 2L * 1024 * 1024;
        using var server = CreateServer();
        using var client = server.CreateClient();

        var request = new HttpRequestMessage(HttpMethod.Post, "/")
        {
            Content = new ByteArrayContent(Array.Empty<byte>()),
        };
        // Manually override Content-Length to a value exceeding the Free tier limit.
        request.Content.Headers.ContentLength = twoMb;

        var response = await client.SendAsync(request);

        response
            .StatusCode.Should()
            .Be(
                HttpStatusCode.RequestEntityTooLarge,
                because: "the Content-Length header alone should trigger the 413 short-circuit"
            );
    }
}
