using System.Net;
using System.Security.Claims;
using System.Text.Json;
using MarcusPrado.Platform.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.DependencyInjection;

namespace MarcusPrado.Platform.AspNetCore.Tests;

public sealed class RateLimitingTests
{
    // ── DI / options ──────────────────────────────────────────────────────────

    [Fact]
    public void AddPlatformRateLimiting_Defaults_RegistersOptions()
    {
        var sp   = BuildServiceProvider();
        var opts = sp.GetService<PlatformRateLimitingOptions>();

        opts.Should().NotBeNull();
        opts!.TenantPermitLimit.Should().Be(200);
        opts.UserPermitLimit.Should().Be(60);
        opts.IpPermitLimit.Should().Be(300);
    }

    [Fact]
    public void AddPlatformRateLimiting_Configure_OverridesOptions()
    {
        var sp = new ServiceCollection()
            .AddLogging()
            .AddPlatformRateLimiting(o =>
            {
                o.TenantPermitLimit = 50;
                o.UserPermitLimit   = 10;
                o.IpPermitLimit     = 25;
            })
            .BuildServiceProvider();

        var opts = sp.GetRequiredService<PlatformRateLimitingOptions>();
        opts.TenantPermitLimit.Should().Be(50);
        opts.UserPermitLimit.Should().Be(10);
        opts.IpPermitLimit.Should().Be(25);
    }

    // ── TenantRateLimitPolicy ─────────────────────────────────────────────────

    [Fact]
    public void TenantPolicy_NoHeader_UsesAnonKey()
    {
        var policy    = new TenantRateLimitPolicy(new PlatformRateLimitingOptions());
        var partition = policy.GetPartition(new DefaultHttpContext());

        partition.PartitionKey.Should().Be("__anon__");
    }

    [Fact]
    public void TenantPolicy_WithHeader_UsesTenantIdAsKey()
    {
        var policy  = new TenantRateLimitPolicy(new PlatformRateLimitingOptions());
        var ctx     = new DefaultHttpContext();
        ctx.Request.Headers["X-Tenant-ID"] = "tenant-42";

        var partition = policy.GetPartition(ctx);

        partition.PartitionKey.Should().Be("tenant-42");
    }

    [Fact]
    public void TenantPolicy_OnRejected_IsNull_DelegatesTo_GlobalHandler()
        => new TenantRateLimitPolicy(new PlatformRateLimitingOptions())
            .OnRejected.Should().BeNull("the global OnRejected handler writes the ProblemDetails body");

    // ── UserRateLimitPolicy ───────────────────────────────────────────────────

    [Fact]
    public void UserPolicy_Anonymous_UsesAnonKey()
    {
        var policy    = new UserRateLimitPolicy(new PlatformRateLimitingOptions());
        var partition = policy.GetPartition(new DefaultHttpContext());

        partition.PartitionKey.Should().Be("__anon__");
    }

    [Fact]
    public void UserPolicy_WithNameIdentifier_UsesSubjectAsKey()
    {
        var policy = new UserRateLimitPolicy(new PlatformRateLimitingOptions());
        var ctx    = new DefaultHttpContext();

        ctx.User = new ClaimsPrincipal(new ClaimsIdentity(
            new[] { new Claim(ClaimTypes.NameIdentifier, "user-99") },
            authenticationType: "test"));

        var partition = policy.GetPartition(ctx);

        partition.PartitionKey.Should().Be("user-99");
    }

    [Fact]
    public void UserPolicy_OnRejected_IsNull_DelegatesTo_GlobalHandler()
        => new UserRateLimitPolicy(new PlatformRateLimitingOptions())
            .OnRejected.Should().BeNull("the global OnRejected handler writes the ProblemDetails body");

    // ── IpRateLimitPolicy ─────────────────────────────────────────────────────

    [Fact]
    public void IpPolicy_NoRemoteIp_UsesUnknownKey()
    {
        var policy = new IpRateLimitPolicy(new PlatformRateLimitingOptions());
        // DefaultHttpContext has no remote IP — Connection.RemoteIpAddress is null
        var partition = policy.GetPartition(new DefaultHttpContext());

        partition.PartitionKey.Should().Be("__unknown__");
    }

    [Fact]
    public void IpPolicy_WithRemoteIp_UsesIpAsKey()
    {
        var policy = new IpRateLimitPolicy(new PlatformRateLimitingOptions());
        var ctx    = new DefaultHttpContext();
        ctx.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("10.0.0.1");

        var partition = policy.GetPartition(ctx);

        partition.PartitionKey.Should().Be("10.0.0.1");
    }

    [Fact]
    public void IpPolicy_OnRejected_IsNotNull()
        => new IpRateLimitPolicy(new PlatformRateLimitingOptions())
            .OnRejected.Should().NotBeNull();

    // ── IpOptions defaults ────────────────────────────────────────────────────

    [Fact]
    public void IpOptions_Defaults_AreReasonable()
    {
        var opts = new PlatformRateLimitingOptions();

        opts.IpPermitLimit.Should().BeGreaterThan(0);
        opts.IpWindow.Should().BeGreaterThan(TimeSpan.Zero);
    }

    // ── Integration: HTTP round-trips via TestServer ──────────────────────────

    [Fact]
    public async Task RateLimiting_WithinLimit_Returns200()
    {
        using var server = BuildRateLimitServer(
            tenantPermit: 5, userPermit: 5,
            policy: PlatformRateLimitingExtensions.TenantPolicy,
            tenantId: "t-integration");
        using var client = server.CreateClient();

        var response = await client.SendAsync(BuildTenantRequest("t-integration"));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task RateLimiting_ExceedingTenantLimit_Returns429()
    {
        const string tenant = "t-exceeded";
        using var server = BuildRateLimitServer(
            tenantPermit: 2, userPermit: 100,
            policy: PlatformRateLimitingExtensions.TenantPolicy,
            tenantId: tenant);
        using var client = server.CreateClient();

        // Exhaust the 2-permit budget
        await client.SendAsync(BuildTenantRequest(tenant));
        await client.SendAsync(BuildTenantRequest(tenant));

        // 3rd request must be rejected
        var response = await client.SendAsync(BuildTenantRequest(tenant));

        response.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
    }

    [Fact]
    public async Task RateLimiting_Rejected_ContentType_IsProblemJson()
    {
        const string tenant = "t-content-type";
        using var server = BuildRateLimitServer(
            tenantPermit: 1, userPermit: 100,
            policy: PlatformRateLimitingExtensions.TenantPolicy,
            tenantId: tenant);
        using var client = server.CreateClient();

        await client.SendAsync(BuildTenantRequest(tenant));     // exhaust
        var response = await client.SendAsync(BuildTenantRequest(tenant));

        response.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
        response.Content.Headers.ContentType?.MediaType
            .Should().Be("application/problem+json");
    }

    [Fact]
    public async Task RateLimiting_Rejected_Body_ContainsProblemDetails()
    {
        const string tenant = "t-body";
        using var server = BuildRateLimitServer(
            tenantPermit: 1, userPermit: 100,
            policy: PlatformRateLimitingExtensions.TenantPolicy,
            tenantId: tenant);
        using var client = server.CreateClient();

        await client.SendAsync(BuildTenantRequest(tenant));     // exhaust
        var response = await client.SendAsync(BuildTenantRequest(tenant));

        response.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);

        var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        json.RootElement.GetProperty("status").GetInt32().Should().Be(429);
        json.RootElement.GetProperty("title").GetString().Should().Be("Too Many Requests");
    }

    [Fact]
    public async Task RateLimiting_TwoTenants_HaveIndependentCounters()
    {
        const string tenantA = "t-a";
        const string tenantB = "t-b";

        // Permit 1 per window — tenantA will be exhausted, tenantB must still pass
        using var server = BuildRateLimitServer(
            tenantPermit: 1, userPermit: 100,
            policy: PlatformRateLimitingExtensions.TenantPolicy,
            tenantId: null /* endpoint has no RequireRateLimiting restriction inline */);
        using var client = server.CreateClient();

        await client.SendAsync(BuildTenantRequest(tenantA));    // exhaust tenantA
        var rejectA = await client.SendAsync(BuildTenantRequest(tenantA));
        var passB   = await client.SendAsync(BuildTenantRequest(tenantB));

        rejectA.StatusCode.Should().Be(HttpStatusCode.TooManyRequests,
            because: "tenantA has exhausted its quota");
        passB.StatusCode.Should().Be(HttpStatusCode.OK,
            because: "tenantB has its own independent counter");
    }

    // ── helpers ───────────────────────────────────────────────────────────────

    private static IServiceProvider BuildServiceProvider()
        => new ServiceCollection()
            .AddLogging()
            .AddPlatformRateLimiting()
            .BuildServiceProvider();

    /// <summary>
    /// Builds a <see cref="TestServer"/> that maps GET /limited with the
    /// given rate-limit policy and exposes the raw tenant header to the
    /// partitioned limiter.
    /// </summary>
    private static TestServer BuildRateLimitServer(
        int tenantPermit,
        int userPermit,
        string policy,
        string? tenantId)
    {
        var builder = new WebHostBuilder()
            .UseEnvironment("Test")
            .ConfigureLogging(l => l.ClearProviders())
            .ConfigureServices(services =>
            {
                services.AddRouting();
                services.AddPlatformRateLimiting(o =>
                {
                    o.TenantPermitLimit = tenantPermit;
                    o.UserPermitLimit   = userPermit;
                });
            })
            .Configure(app =>
            {
                app.UseRouting();
                app.UseRateLimiter();
                app.UseEndpoints(endpoints =>
                {
                    endpoints
                        .MapGet("/limited", () => "ok")
                        .RequireRateLimiting(policy);
                });
            });

        return new TestServer(builder);
    }

    private static HttpRequestMessage BuildTenantRequest(string tenantId)
    {
        var req = new HttpRequestMessage(HttpMethod.Get, "/limited");
        req.Headers.Add("X-Tenant-ID", tenantId);
        return req;
    }
}
