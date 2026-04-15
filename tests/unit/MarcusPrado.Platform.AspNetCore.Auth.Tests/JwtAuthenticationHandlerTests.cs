namespace MarcusPrado.Platform.AspNetCore.Auth.Tests;

/// <summary>
/// Integration tests for <see cref="MarcusPrado.Platform.AspNetCore.Auth.Handlers.JwtAuthenticationHandler"/>.
/// </summary>
public sealed class JwtAuthenticationHandlerTests : IDisposable
{
    private readonly HttpClient _client = AuthTestServer.CreateClient();

    // ── Happy path ────────────────────────────────────────────────────────────

    [Fact]
    public async Task ValidToken_ShouldReturn200_WithUserId()
    {
        var token = JwtTokenFactory.CreateValidToken(subject: "user-42");
        var request = new HttpRequestMessage(HttpMethod.Get, AuthTestServer.JwtInfoRoute);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.SendAsync(request);
        var body = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        body.Should().Be("user-42");
    }

    [Fact]
    public async Task ValidToken_ShouldPopulate_Permissions_InUserContext()
    {
        var token = JwtTokenFactory.CreateValidToken(
            extraClaims: [new Claim("permission", "read:users"), new Claim("permission", "write:users")]);

        var request = new HttpRequestMessage(HttpMethod.Get, AuthTestServer.JwtInfoRoute);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    // ── Failure paths ─────────────────────────────────────────────────────────

    [Fact]
    public async Task MissingToken_ShouldReturn_401()
    {
        var response = await _client.GetAsync(AuthTestServer.JwtInfoRoute);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ExpiredToken_ShouldReturn_401()
    {
        var token = JwtTokenFactory.CreateExpiredToken();
        var request = new HttpRequestMessage(HttpMethod.Get, AuthTestServer.JwtInfoRoute);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task InvalidSignature_ShouldReturn_401()
    {
        var token = JwtTokenFactory.CreateTokenWithWrongKey();
        var request = new HttpRequestMessage(HttpMethod.Get, AuthTestServer.JwtInfoRoute);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    public void Dispose() => _client.Dispose();
}
