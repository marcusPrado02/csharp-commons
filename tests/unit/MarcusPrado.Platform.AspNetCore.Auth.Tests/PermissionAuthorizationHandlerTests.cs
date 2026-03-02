namespace MarcusPrado.Platform.AspNetCore.Auth.Tests;

/// <summary>
/// Integration tests for <see cref="MarcusPrado.Platform.AspNetCore.Auth.Handlers.PermissionAuthorizationHandler"/>.
/// </summary>
public sealed class PermissionAuthorizationHandlerTests : IDisposable
{
    private readonly HttpClient _client = AuthTestServer.CreateClient();

    [Fact]
    public async Task HasRequiredPermission_ShouldReturn_200()
    {
        var token = JwtTokenFactory.CreateValidToken(
            extraClaims: [new Claim("permission", AuthTestServer.TestPermission)]);

        var request = new HttpRequestMessage(HttpMethod.Get, AuthTestServer.PermissionRoute);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task MissingPermission_ShouldReturn_403()
    {
        // Token has a different, unrelated permission
        var token = JwtTokenFactory.CreateValidToken(
            extraClaims: [new Claim("permission", "write:only")]);

        var request = new HttpRequestMessage(HttpMethod.Get, AuthTestServer.PermissionRoute);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    public void Dispose() => _client.Dispose();
}
