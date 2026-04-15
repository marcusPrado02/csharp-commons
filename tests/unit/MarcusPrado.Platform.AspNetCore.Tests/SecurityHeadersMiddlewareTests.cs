using MarcusPrado.Platform.AspNetCore.Tests.Helpers;

namespace MarcusPrado.Platform.AspNetCore.Tests;

public sealed class SecurityHeadersMiddlewareTests
{
    [Fact]
    public async Task Response_HasXContentTypeOptions_Nosniff()
    {
        using var server = PlatformTestServer.CreateWithSecurityOptions();
        var response = await server.CreateClient().GetAsync("/ping");

        response.Headers.TryGetValues("X-Content-Type-Options", out var values);
        values.Should().ContainSingle().Which.Should().Be("nosniff");
    }

    [Fact]
    public async Task Response_HasXFrameOptions_Deny()
    {
        using var server = PlatformTestServer.CreateWithSecurityOptions();
        var response = await server.CreateClient().GetAsync("/ping");

        response.Headers.TryGetValues("X-Frame-Options", out var values);
        values.Should().ContainSingle().Which.Should().Be("DENY");
    }

    [Fact]
    public async Task Response_HasXXssProtection_Zero()
    {
        using var server = PlatformTestServer.CreateWithSecurityOptions();
        var response = await server.CreateClient().GetAsync("/ping");

        response.Headers.TryGetValues("X-XSS-Protection", out var values);
        values.Should().ContainSingle().Which.Should().Be("0");
    }

    [Fact]
    public async Task Response_HasReferrerPolicy_NoReferrer()
    {
        using var server = PlatformTestServer.CreateWithSecurityOptions();
        var response = await server.CreateClient().GetAsync("/ping");

        response.Headers.TryGetValues("Referrer-Policy", out var values);
        values.Should().ContainSingle().Which.Should().Be("no-referrer");
    }

    [Fact]
    public async Task Response_HasContentSecurityPolicy_DefaultSrcSelf()
    {
        using var server = PlatformTestServer.CreateWithSecurityOptions();
        var response = await server.CreateClient().GetAsync("/ping");

        response.Headers.TryGetValues("Content-Security-Policy", out var values);
        values.Should().ContainSingle().Which.Should().Be("default-src 'self'");
    }

    [Fact]
    public async Task Response_NoCsp_WhenCspDisabled()
    {
        using var server = PlatformTestServer.CreateWithSecurityOptions(o => o.EnableContentSecurityPolicy = false);
        var response = await server.CreateClient().GetAsync("/ping");

        response.Headers.TryGetValues("Content-Security-Policy", out _).Should().BeFalse();
    }

    [Fact]
    public async Task Response_UsesCustomReferrerPolicy()
    {
        using var server = PlatformTestServer.CreateWithSecurityOptions(o =>
            o.ReferrerPolicy = "strict-origin-when-cross-origin"
        );
        var response = await server.CreateClient().GetAsync("/ping");

        response.Headers.TryGetValues("Referrer-Policy", out var values);
        values.Should().ContainSingle().Which.Should().Be("strict-origin-when-cross-origin");
    }

    [Fact]
    public async Task Response_NoXFrameOptions_WhenDisabled()
    {
        using var server = PlatformTestServer.CreateWithSecurityOptions(o => o.EnableXFrameOptions = false);
        var response = await server.CreateClient().GetAsync("/ping");

        response.Headers.TryGetValues("X-Frame-Options", out _).Should().BeFalse();
    }
}
