using MarcusPrado.Platform.AspNetCore.Cors;

namespace MarcusPrado.Platform.AspNetCore.Tests;

public sealed class CorsTests
{
    // ── Helper ────────────────────────────────────────────────────────────────

    private static TestServer CreateCorsServer(
        Action<PlatformCorsOptions>? configure = null,
        string policyName = CorsConstants.DefaultPolicy,
        Action<IServiceCollection>? configureServices = null)
    {
        var builder = new WebHostBuilder()
            .UseEnvironment("Test")
            .ConfigureLogging(l => l.ClearProviders())
            .ConfigureServices(services =>
            {
                services.AddRouting();
                services.AddPlatformCors(configure);
                configureServices?.Invoke(services);
            })
            .Configure(app =>
            {
                app.UseCors(policyName);
                app.Run(async ctx =>
                {
                    ctx.Response.StatusCode = 200;
                    await ctx.Response.WriteAsync("ok");
                });
            });

        return new TestServer(builder);
    }

    private static HttpRequestMessage Preflight(string origin, string method = "GET")
        => new(HttpMethod.Options, "/test")
        {
            Headers =
            {
                { "Origin", origin },
                { "Access-Control-Request-Method", method }
            }
        };

    // ── Tests ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task DevPermissive_Preflight_ReturnsWildcardOrigin()
    {
        using var server = CreateCorsServer(o => o.Profile = PlatformCorsProfile.DevPermissive);
        var client = server.CreateClient();

        var response = await client.SendAsync(Preflight("http://example.com"));

        response.Headers.TryGetValues("Access-Control-Allow-Origin", out var values).Should().BeTrue();
        values!.First().Should().Be("*");
    }

    [Fact]
    public async Task StagingRestricted_AllowedOrigin_ReturnsOriginHeader()
    {
        const string allowedOrigin = "https://staging.myapp.com";

        using var server = CreateCorsServer(o =>
        {
            o.Profile = PlatformCorsProfile.StagingRestricted;
            o.AllowedOrigins = [allowedOrigin];
        });
        var client = server.CreateClient();

        var response = await client.SendAsync(Preflight(allowedOrigin));

        response.Headers.TryGetValues("Access-Control-Allow-Origin", out var values).Should().BeTrue();
        values!.First().Should().Be(allowedOrigin);
    }

    [Fact]
    public async Task StagingRestricted_DisallowedOrigin_NoOriginHeader()
    {
        using var server = CreateCorsServer(o =>
        {
            o.Profile = PlatformCorsProfile.StagingRestricted;
            o.AllowedOrigins = ["https://staging.myapp.com"];
        });
        var client = server.CreateClient();

        var response = await client.SendAsync(Preflight("https://evil.com"));

        response.Headers.Contains("Access-Control-Allow-Origin").Should().BeFalse();
    }

    [Fact]
    public async Task ProductionLocked_AllowedOrigin_HasMaxAgeHeader()
    {
        const string allowedOrigin = "https://app.myapp.com";

        using var server = CreateCorsServer(o =>
        {
            o.Profile = PlatformCorsProfile.ProductionLocked;
            o.AllowedOrigins = [allowedOrigin];
        });
        var client = server.CreateClient();

        var response = await client.SendAsync(Preflight(allowedOrigin));

        response.Headers.TryGetValues("Access-Control-Max-Age", out var values).Should().BeTrue();
        values!.First().Should().Be("3600"); // 1 hour in seconds
    }

    [Fact]
    public async Task TenantAwarePolicy_KnownTenant_AllowsOrigin()
    {
        const string tenantId = "tenant-abc";
        const string allowedOrigin = "https://tenant-abc.myapp.com";

        using var server = CreateCorsServer(
            configure: o =>
            {
                o.EnableTenantAwarePolicy = true;
                o.TenantOrigins[tenantId] = [allowedOrigin];
            },
            policyName: CorsConstants.TenantPolicy,
            configureServices: services =>
            {
                // Register a fake ITenantContext that always returns our tenant ID
                services.AddScoped<ITenantContext>(_ => new FakeTenantContext(tenantId));
            });

        var client = server.CreateClient();

        var response = await client.SendAsync(Preflight(allowedOrigin));

        response.Headers.TryGetValues("Access-Control-Allow-Origin", out var values).Should().BeTrue();
        values!.First().Should().Be(allowedOrigin);
    }

    [Fact]
    public async Task TenantAwarePolicy_UnknownTenant_DeniesOrigin()
    {
        const string allowedOrigin = "https://other-tenant.myapp.com";

        using var server = CreateCorsServer(
            configure: o =>
            {
                o.EnableTenantAwarePolicy = true;
                o.TenantOrigins["tenant-xyz"] = [allowedOrigin];
            },
            policyName: CorsConstants.TenantPolicy,
            configureServices: services =>
            {
                // Unknown tenant — not in TenantOrigins
                services.AddScoped<ITenantContext>(_ => new FakeTenantContext("unknown-tenant"));
            });

        var client = server.CreateClient();

        var response = await client.SendAsync(Preflight(allowedOrigin));

        response.Headers.Contains("Access-Control-Allow-Origin").Should().BeFalse();
    }

    // ── Fake helpers ──────────────────────────────────────────────────────────

    private sealed class FakeTenantContext : ITenantContext
    {
        private string? _tenantId;

        public FakeTenantContext(string? tenantId) => _tenantId = tenantId;

        public string? TenantId => _tenantId;

        public void SetTenantId(string? tenantId) => _tenantId = tenantId;
    }
}
