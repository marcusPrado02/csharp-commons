using Microsoft.Extensions.Logging.Abstractions;

namespace MarcusPrado.Platform.Security.Tests;

internal sealed class FakeTokenHandler : HttpMessageHandler
{
    public int CallCount { get; private set; }
    public int ExpiresIn { get; set; } = 3600;

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        CallCount++;
        var json = $$"""{"access_token":"token-{{CallCount}}","token_type":"Bearer","expires_in":{{ExpiresIn}}}""";
        return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK)
        {
            Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json")
        });
    }
}

public class OidcClientServiceTests
{
    private static OidcClientService CreateService(FakeTokenHandler handler, OidcClientOptions? options = null)
    {
        var http = new HttpClient(handler) { BaseAddress = new Uri("https://auth.example.com") };
        var opts = options ?? new OidcClientOptions
        {
            Authority = "https://auth.example.com",
            ClientId = "my-client",
            ClientSecret = "secret",
            Scope = "api",
            RefreshBeforeExpirySeconds = 30,
        };
        var wrappedOptions = Options.Create(opts);
        var logger = NullLogger<OidcClientService>.Instance;
        return new OidcClientService(http, wrappedOptions, logger);
    }

    [Fact]
    public async Task GetAccessTokenAsync_FirstCall_FetchesTokenFromEndpoint()
    {
        var handler = new FakeTokenHandler();
        var svc = CreateService(handler);

        var token = await svc.GetAccessTokenAsync();

        token.Should().Be("token-1");
        handler.CallCount.Should().Be(1);
    }

    [Fact]
    public async Task GetAccessTokenAsync_SecondCallWithinTtl_UsesCachedToken()
    {
        var handler = new FakeTokenHandler { ExpiresIn = 3600 };
        var svc = CreateService(handler);

        var token1 = await svc.GetAccessTokenAsync();
        var token2 = await svc.GetAccessTokenAsync();

        token1.Should().Be("token-1");
        token2.Should().Be("token-1");
        handler.CallCount.Should().Be(1);
    }

    [Fact]
    public async Task GetAccessTokenAsync_TokenExpired_ReFetchesNewToken()
    {
        var handler = new FakeTokenHandler { ExpiresIn = 0 };
        var svc = CreateService(handler, new OidcClientOptions
        {
            Authority = "https://auth.example.com",
            ClientId = "my-client",
            ClientSecret = "secret",
            Scope = "api",
            RefreshBeforeExpirySeconds = 0,
        });

        var token1 = await svc.GetAccessTokenAsync();
        // With ExpiresIn=0, the token is immediately expired
        var token2 = await svc.GetAccessTokenAsync();

        handler.CallCount.Should().Be(2);
        token2.Should().Be("token-2");
    }

    [Fact]
    public void IsExpired_WithBufferSeconds_ReturnsTrueWhenWithinBuffer()
    {
        var issuedAt = DateTimeOffset.UtcNow.AddSeconds(-3570); // issued 59.5 min ago
        var response = new TokenResponse("tok", "Bearer", 3600, issuedAt);

        // ExpiresAt = issuedAt + 3600s = ~30s from now
        // With buffer=60, it should appear expired
        response.IsExpired(bufferSeconds: 60).Should().BeTrue();
        response.IsExpired(bufferSeconds: 0).Should().BeFalse();
    }

    [Fact]
    public void TokenResponse_ExpiresAt_IsIssuedAtPlusExpiresIn()
    {
        var issuedAt = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var response = new TokenResponse("tok", "Bearer", 3600, issuedAt);

        response.ExpiresAt.Should().Be(issuedAt.AddSeconds(3600));
    }

    [Fact]
    public async Task MachineToMachineHttpHandler_InjectsBearerToken()
    {
        var oidcService = Substitute.For<IOidcClientService>();
        oidcService.GetAccessTokenAsync(Arg.Any<CancellationToken>()).Returns("my-access-token");

        HttpRequestMessage? captured = null;
        var innerHandler = new FakeInnerHandler(req =>
        {
            captured = req;
            return new HttpResponseMessage(System.Net.HttpStatusCode.OK);
        });

        var handler = new MachineToMachineHttpHandler(oidcService) { InnerHandler = innerHandler };
        var client = new HttpClient(handler);

        await client.GetAsync("https://api.example.com/data");

        captured.Should().NotBeNull();
        captured!.Headers.Authorization.Should().NotBeNull();
        captured.Headers.Authorization!.Scheme.Should().Be("Bearer");
        captured.Headers.Authorization.Parameter.Should().Be("my-access-token");
    }

    [Fact]
    public async Task GetAccessTokenAsync_ConcurrentCalls_OnlyOneHttpCallMade()
    {
        var handler = new FakeTokenHandler { ExpiresIn = 3600 };
        var svc = CreateService(handler);

        // Fire two concurrent calls before any token is cached
        var task1 = svc.GetAccessTokenAsync();
        var task2 = svc.GetAccessTokenAsync();
        await Task.WhenAll(task1, task2);

        // Due to double-check locking, only one HTTP call should be made
        handler.CallCount.Should().Be(1);
        (await task1).Should().Be("token-1");
        (await task2).Should().Be("token-1");
    }

    [Fact]
    public void AddPlatformOidcClient_RegistersIOidcClientService()
    {
        var services = new ServiceCollection();
        services.AddLogging();

        services.AddPlatformOidcClient(opts =>
        {
            opts.Authority = "https://auth.example.com";
            opts.ClientId = "client";
            opts.ClientSecret = "secret";
            opts.Scope = "api";
        });

        var provider = services.BuildServiceProvider();
        var oidc = provider.GetService<IOidcClientService>();

        oidc.Should().NotBeNull();
    }
}

internal sealed class FakeInnerHandler : HttpMessageHandler
{
    private readonly Func<HttpRequestMessage, HttpResponseMessage> _handler;

    public FakeInnerHandler(Func<HttpRequestMessage, HttpResponseMessage> handler)
        => _handler = handler;

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        => Task.FromResult(_handler(request));
}
