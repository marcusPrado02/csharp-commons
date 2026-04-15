using MarcusPrado.Platform.AspNetCore.Tests.Helpers;

namespace MarcusPrado.Platform.AspNetCore.Tests;

/// <summary>
/// Tests that <see cref="MarcusPrado.Platform.AspNetCore.Middleware.CorrelationMiddleware"/>
/// correctly extracts, generates, and propagates correlation identifiers.
/// </summary>
public sealed class CorrelationMiddlewareTests
{
    private const string CorrelationIdHeader = "X-Correlation-ID";
    private const string RequestIdHeader = "X-Request-ID";

    // ── X-Correlation-ID ──────────────────────────────────────────────────────

    [Fact]
    public async Task CorrelationId_ShouldBePropagated_AsResponseHeader_WhenProvidedInRequest()
    {
        using var server = PlatformTestServer.Create();
        using var client = server.CreateClient();

        const string expectedId = "trace-abc-123";
        using var request = new HttpRequestMessage(HttpMethod.Get, "/ping");
        request.Headers.Add(CorrelationIdHeader, expectedId);

        var response = await client.SendAsync(request);

        response.Headers.TryGetValues(CorrelationIdHeader, out var values);
        values
            .Should()
            .NotBeNull()
            .And.ContainSingle()
            .Which.Should()
            .Be(expectedId, because: "the correlation ID from the request header must be echoed in the response");
    }

    [Fact]
    public async Task CorrelationId_ShouldBeGenerated_AsResponseHeader_WhenAbsentFromRequest()
    {
        using var server = PlatformTestServer.Create();
        using var client = server.CreateClient();

        var response = await client.GetAsync("/ping");

        response.Headers.TryGetValues(CorrelationIdHeader, out var values);
        var correlationId = values?.FirstOrDefault();
        correlationId
            .Should()
            .NotBeNullOrWhiteSpace(because: "a correlation ID must always be present in the response");
    }

    // ── X-Request-ID ──────────────────────────────────────────────────────────

    [Fact]
    public async Task RequestId_ShouldBePropagated_AsResponseHeader_WhenProvidedInRequest()
    {
        using var server = PlatformTestServer.Create();
        using var client = server.CreateClient();

        const string expectedId = "req-xyz-456";
        using var request = new HttpRequestMessage(HttpMethod.Get, "/ping");
        request.Headers.Add(RequestIdHeader, expectedId);

        var response = await client.SendAsync(request);

        response.Headers.TryGetValues(RequestIdHeader, out var values);
        values
            .Should()
            .NotBeNull()
            .And.ContainSingle()
            .Which.Should()
            .Be(expectedId, because: "the request ID from the request header must be echoed in the response");
    }

    [Fact]
    public async Task RequestId_ShouldBeGenerated_AsResponseHeader_WhenAbsentFromRequest()
    {
        using var server = PlatformTestServer.Create();
        using var client = server.CreateClient();

        var response = await client.GetAsync("/ping");

        response.Headers.TryGetValues(RequestIdHeader, out var values);
        var requestId = values?.FirstOrDefault();
        requestId.Should().NotBeNullOrWhiteSpace(because: "a request ID must always be present in the response");
    }

    // ── ICorrelationContext ───────────────────────────────────────────────────

    [Fact]
    public async Task CorrelationContext_ShouldBePopulated_WithSameValues_AsResponseHeaders()
    {
        using var server = PlatformTestServer.Create();
        using var client = server.CreateClient();

        const string correlationId = "ctx-corr-789";
        const string requestId = "ctx-req-000";

        using var request = new HttpRequestMessage(HttpMethod.Get, "/correlation");
        request.Headers.Add(CorrelationIdHeader, correlationId);
        request.Headers.Add(RequestIdHeader, requestId);

        var response = await client.SendAsync(request);
        var body = await response.Content.ReadAsStringAsync();

        body.Should()
            .Be(
                $"{correlationId}|{requestId}",
                because: "ICorrelationContext must be populated with the same IDs as the headers"
            );
    }
}
