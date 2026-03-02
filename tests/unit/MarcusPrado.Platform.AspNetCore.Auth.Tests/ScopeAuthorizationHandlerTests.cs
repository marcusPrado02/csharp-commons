namespace MarcusPrado.Platform.AspNetCore.Auth.Tests;

/// <summary>
/// Integration tests for <see cref="MarcusPrado.Platform.AspNetCore.Auth.Handlers.ScopeAuthorizationHandler"/>.
/// </summary>
public sealed class ScopeAuthorizationHandlerTests : IDisposable
{
    private readonly HttpClient _client = AuthTestServer.CreateClient();

    [Fact]
    public async Task HasRequiredScope_ShouldReturn_200()
    {
        // scope claim is a single space-separated value per RFC 8693
        var token = JwtTokenFactory.CreateValidToken(
            extraClaims: [new Claim("scope", $"{AuthTestServer.TestScope} other:scope")]);

        var request = new HttpRequestMessage(HttpMethod.Get, AuthTestServer.ScopeRoute);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task MissingScope_ShouldReturn_403()
    {
        var token = JwtTokenFactory.CreateValidToken(
            extraClaims: [new Claim("scope", "other:scope only:this")]);

        var request = new HttpRequestMessage(HttpMethod.Get, AuthTestServer.ScopeRoute);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    public void Dispose() => _client.Dispose();
}
