using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using MarcusPrado.Platform.Http.Clients;
using MarcusPrado.Platform.Http.Extensions;
using MarcusPrado.Platform.Http.Handlers;
using MarcusPrado.Platform.Abstractions.Context;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Xunit;

namespace MarcusPrado.Platform.Http.Tests;

public sealed class HttpClientFactoryExtensionsTests
{
    // ── Existing tests (1–5) ─────────────────────────────────────────────────

    [Fact]
    public void AddPlatformHttpClient_RegistersHandlers()
    {
        var sp = BuildServiceProvider<SampleHttpClient>();

        sp.GetService<CorrelationHeaderHandler>().Should().NotBeNull();
        sp.GetService<TenantHeaderHandler>().Should().NotBeNull();
    }

    [Fact]
    public void AddPlatformHttpClient_RegistersTypedClient()
    {
        var sp = BuildServiceProvider<SampleHttpClient>();

        sp.GetService<SampleHttpClient>().Should().NotBeNull();
    }

    [Fact]
    public void HttpClientOptions_Defaults()
    {
        var opts = new HttpClientOptions();

        opts.Timeout.Should().Be(TimeSpan.FromSeconds(30));
        opts.BaseAddress.Should().BeNull();
    }

    [Fact]
    public void CorrelationHeaderHandler_NullCorrelation_Throws()
    {
        var act = () => new CorrelationHeaderHandler(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void TenantHeaderHandler_NullTenant_Throws()
    {
        var act = () => new TenantHeaderHandler(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    // ── New tests (6–12) ─────────────────────────────────────────────────────

    [Fact]
    public void AddPlatformHttpClient_RegistersAuthTokenHandler()
    {
        var sp = BuildServiceProvider<SampleHttpClient>();

        sp.GetService<AuthTokenHandler>().Should().NotBeNull();
    }

    [Fact]
    public void AuthTokenHandler_NullContextAccessor_Throws()
    {
        var act = () => new AuthTokenHandler(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task AuthTokenHandler_NoHttpContext_DoesNotSetAuthorizationHeader()
    {
        // IHttpContextAccessor with no HttpContext (e.g. background service scenario)
        var accessor = Substitute.For<IHttpContextAccessor>();
        accessor.HttpContext.Returns((HttpContext?)null);

        var fake = new FakeMessageHandler(HttpStatusCode.OK);
        var handler = new AuthTokenHandler(accessor) { InnerHandler = fake };
        var client = new HttpClient(handler) { BaseAddress = new Uri("http://localhost") };

        await client.GetAsync("/test");

        fake.LastRequest!.Headers.Authorization.Should().BeNull();
    }

    [Fact]
    public async Task AuthTokenHandler_WithToken_SetsAuthorizationHeader()
    {
        const string token = "Bearer eyJhbGciOiJSUzI1NiJ9.test";

        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers.Authorization = token;

        var accessor = Substitute.For<IHttpContextAccessor>();
        accessor.HttpContext.Returns(httpContext);

        var fake = new FakeMessageHandler(HttpStatusCode.OK);
        var handler = new AuthTokenHandler(accessor) { InnerHandler = fake };
        var client = new HttpClient(handler) { BaseAddress = new Uri("http://localhost") };

        await client.GetAsync("/test");

        fake.LastRequest!.Headers.Authorization.Should().NotBeNull();
        fake.LastRequest.Headers.Authorization!.ToString().Should().Be(token);
    }

    [Fact]
    public async Task AuthTokenHandler_AlreadySetHeader_DoesNotOverwrite()
    {
        const string existingToken = "Bearer existing-token";
        const string inboundToken = "Bearer inbound-token";

        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers.Authorization = inboundToken;

        var accessor = Substitute.For<IHttpContextAccessor>();
        accessor.HttpContext.Returns(httpContext);

        var fake = new FakeMessageHandler(HttpStatusCode.OK);
        var handler = new AuthTokenHandler(accessor) { InnerHandler = fake };
        var client = new HttpClient(handler) { BaseAddress = new Uri("http://localhost") };

        var request = new HttpRequestMessage(HttpMethod.Get, "/test");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "existing-token");

        await client.SendAsync(request);

        fake.LastRequest!.Headers.Authorization!.ToString().Should().Be(existingToken);
    }

    [Fact]
    public async Task CorrelationHeaderHandler_WithCorrelationId_SetsHeader()
    {
        var correlation = Substitute.For<ICorrelationContext>();
        correlation.CorrelationId.Returns("corr-abc-123");
        correlation.RequestId.Returns(string.Empty);

        var fake = new FakeMessageHandler(HttpStatusCode.OK);
        var handler = new CorrelationHeaderHandler(correlation) { InnerHandler = fake };
        var client = new HttpClient(handler) { BaseAddress = new Uri("http://localhost") };

        await client.GetAsync("/test");

        fake.LastRequest!.Headers.TryGetValues("X-Correlation-ID", out var values).Should().BeTrue();
        values.Should().Contain("corr-abc-123");
    }

    [Fact]
    public async Task TenantHeaderHandler_WithTenantId_SetsHeader()
    {
        var tenant = Substitute.For<ITenantContext>();
        tenant.TenantId.Returns("tenant-xyz");

        var fake = new FakeMessageHandler(HttpStatusCode.OK);
        var handler = new TenantHeaderHandler(tenant) { InnerHandler = fake };
        var client = new HttpClient(handler) { BaseAddress = new Uri("http://localhost") };

        await client.GetAsync("/test");

        fake.LastRequest!.Headers.TryGetValues("X-Tenant-ID", out var values).Should().BeTrue();
        values.Should().Contain("tenant-xyz");
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static IServiceProvider BuildServiceProvider<TClient>()
        where TClient : TypedHttpClient
    {
        var correlation = Substitute.For<ICorrelationContext>();
        var tenant      = Substitute.For<ITenantContext>();

        return new ServiceCollection()
            .AddLogging()
            .AddSingleton(correlation)
            .AddSingleton(tenant)
            .AddScoped<ICorrelationContext>(_ => correlation)
            .AddScoped<ITenantContext>(_ => tenant)
            .AddPlatformHttpClient<TClient>()
            .BuildServiceProvider();
    }
}

/// <summary>Minimal concrete TypedHttpClient for testing registration.</summary>
internal sealed class SampleHttpClient : TypedHttpClient
{
    public SampleHttpClient(HttpClient http, Microsoft.Extensions.Logging.ILogger<SampleHttpClient> logger)
        : base(http, logger) { }
}

/// <summary>Captures the last <see cref="HttpRequestMessage"/> sent through the pipeline.</summary>
internal sealed class FakeMessageHandler : HttpMessageHandler
{
    private readonly HttpStatusCode _statusCode;

    public HttpRequestMessage? LastRequest { get; private set; }

    public FakeMessageHandler(HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        _statusCode = statusCode;
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        LastRequest = request;
        return Task.FromResult(new HttpResponseMessage(_statusCode));
    }
}
