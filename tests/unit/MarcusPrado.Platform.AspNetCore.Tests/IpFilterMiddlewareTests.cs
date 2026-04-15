using System.Net;
using MarcusPrado.Platform.AspNetCore.IpFiltering;

namespace MarcusPrado.Platform.AspNetCore.Tests;

/// <summary>
/// Tests for <see cref="IpFilterMiddleware"/> covering whitelist, blacklist,
/// CIDR matching, and X-Forwarded-For header handling.
/// </summary>
public sealed class IpFilterMiddlewareTests
{
    // ── Helpers ───────────────────────────────────────────────────────────────

    private static TestServer CreateServer(Action<IpFilterOptions>? configure = null)
    {
        var builder = new WebHostBuilder()
            .UseEnvironment("Test")
            .ConfigureLogging(l => l.ClearProviders())
            .ConfigureServices(services =>
            {
                services.AddPlatformIpFilter(configure);
            })
            .Configure(app =>
            {
                app.UseIpFilter();
                app.Run(async ctx =>
                {
                    ctx.Response.StatusCode = 200;
                    await ctx.Response.WriteAsync("ok");
                });
            });

        return new TestServer(builder);
    }

    /// <summary>
    /// Sends a request to the server spoofing the remote IP via TestServer.SendAsync.
    /// Returns the resulting HttpContext so callers can inspect response status.
    /// </summary>
    private static Task<HttpContext> SendAsync(
        TestServer server,
        string remoteIp,
        Action<HttpContext>? configureContext = null
    ) =>
        server.SendAsync(ctx =>
        {
            ctx.Connection.RemoteIpAddress = IPAddress.Parse(remoteIp);
            configureContext?.Invoke(ctx);
        });

    // ── Tests ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task NoRestrictions_AllowsRequest_Returns200()
    {
        // No whitelist, no blacklist → every IP is allowed.
        using var server = CreateServer();

        var ctx = await SendAsync(server, "10.0.0.1");

        ctx.Response.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task BlacklistedIp_BlocksRequest_Returns403()
    {
        using var server = CreateServer(opts =>
        {
            opts.Blacklist.Add("192.168.1.50");
        });

        var ctx = await SendAsync(server, "192.168.1.50");

        ctx.Response.StatusCode.Should().Be(403);
    }

    [Fact]
    public async Task IpNotInWhitelist_BlocksRequest_Returns403()
    {
        using var server = CreateServer(opts =>
        {
            opts.Whitelist.Add("192.168.1.100");
        });

        var ctx = await SendAsync(server, "10.0.0.1");

        ctx.Response.StatusCode.Should().Be(403);
    }

    [Fact]
    public async Task IpInWhitelist_AllowsRequest_Returns200()
    {
        using var server = CreateServer(opts =>
        {
            opts.Whitelist.Add("192.168.1.100");
        });

        var ctx = await SendAsync(server, "192.168.1.100");

        ctx.Response.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task XForwardedFor_Respected_WhenTrustForwardedForIsTrue()
    {
        // RemoteIpAddress is a trusted proxy, but the real client (from X-Forwarded-For) is blacklisted.
        using var server = CreateServer(opts =>
        {
            opts.TrustForwardedFor = true;
            opts.Blacklist.Add("203.0.113.5");
        });

        var ctx = await SendAsync(
            server,
            "10.0.0.1",
            c =>
            {
                c.Request.Headers["X-Forwarded-For"] = "203.0.113.5";
            }
        );

        ctx.Response.StatusCode.Should()
            .Be(403, because: "the X-Forwarded-For IP should be evaluated when TrustForwardedFor is true");
    }

    [Fact]
    public async Task BlacklistCheckedBeforeWhitelist_BlacklistedWhitelistedIp_Returns403()
    {
        // Same IP is both whitelisted and blacklisted; blacklist must win.
        using var server = CreateServer(opts =>
        {
            opts.Blacklist.Add("192.168.1.1");
            opts.Whitelist.Add("192.168.1.1");
        });

        var ctx = await SendAsync(server, "192.168.1.1");

        ctx.Response.StatusCode.Should().Be(403, because: "blacklist must be evaluated before whitelist");
    }

    [Fact]
    public async Task CidrBlacklist_BlocksIpInRange_Returns403()
    {
        using var server = CreateServer(opts =>
        {
            opts.Blacklist.Add("10.10.0.0/24");
        });

        var ctx = await SendAsync(server, "10.10.0.55");

        ctx.Response.StatusCode.Should().Be(403, because: "an IP within the blacklisted CIDR range must be blocked");
    }

    [Fact]
    public async Task CidrWhitelist_AllowsIpInRange_Returns200()
    {
        using var server = CreateServer(opts =>
        {
            opts.Whitelist.Add("172.16.0.0/12");
        });

        var ctx = await SendAsync(server, "172.20.5.10");

        ctx.Response.StatusCode.Should().Be(200, because: "an IP within the whitelisted CIDR range must be allowed");
    }
}
