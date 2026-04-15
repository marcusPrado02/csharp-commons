using MarcusPrado.Platform.AspNetCore.Middleware;
using MarcusPrado.Platform.AspNetCore.Tests.Helpers;

namespace MarcusPrado.Platform.AspNetCore.Tests;

/// <summary>
/// Tests that <see cref="TenantResolutionMiddleware"/> resolves the tenant from
/// the supported sources in the correct priority order.
/// </summary>
public sealed class TenantResolutionMiddlewareTests
{
    // ── Header resolution ─────────────────────────────────────────────────────

    [Fact]
    public async Task TenantId_ShouldBeResolvedFromHeader_WhenPresent()
    {
        using var server = PlatformTestServer.Create();
        using var client = server.CreateClient();

        using var request = new HttpRequestMessage(HttpMethod.Get, "/tenant");
        request.Headers.Add(TenantResolutionMiddleware.TenantIdHeader, "acme");

        var response = await client.SendAsync(request);
        var body = await response.Content.ReadAsStringAsync();

        body.Should()
            .Be(
                "acme",
                because: $"a '{TenantResolutionMiddleware.TenantIdHeader}' header value must be propagated to ITenantContext"
            );
    }

    [Fact]
    public async Task TenantId_ShouldBeNull_WhenNoResolutionSourceIsPresent()
    {
        using var server = PlatformTestServer.Create();
        using var client = server.CreateClient();

        var response = await client.GetAsync("/tenant");
        var body = await response.Content.ReadAsStringAsync();

        body.Should()
            .Be("null", because: "ITenantContext.TenantId must be null when no header, claim, or subdomain is present");
    }

    [Theory]
    [InlineData("tenant1")]
    [InlineData("my-org")]
    [InlineData("enterprise-corp")]
    public async Task TenantId_ShouldSupportArbitraryTenantIdentifiers(string tenantId)
    {
        using var server = PlatformTestServer.Create();
        using var client = server.CreateClient();

        using var request = new HttpRequestMessage(HttpMethod.Get, "/tenant");
        request.Headers.Add(TenantResolutionMiddleware.TenantIdHeader, tenantId);

        var response = await client.SendAsync(request);
        var body = await response.Content.ReadAsStringAsync();

        body.Should().Be(tenantId);
    }
}
