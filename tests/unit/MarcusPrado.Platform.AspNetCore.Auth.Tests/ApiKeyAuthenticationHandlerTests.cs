namespace MarcusPrado.Platform.AspNetCore.Auth.Tests;

/// <summary>
/// Integration tests for <see cref="MarcusPrado.Platform.AspNetCore.Auth.Handlers.ApiKeyAuthenticationHandler"/>.
/// </summary>
public sealed class ApiKeyAuthenticationHandlerTests : IDisposable
{
    private readonly HttpClient _client = AuthTestServer.CreateClient();

    [Fact]
    public async Task ValidApiKey_ShouldReturn_200()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, AuthTestServer.ApiKeyRoute);
        request.Headers.Add(ApiKeyAuthenticationOptions.DefaultHeaderName, AuthTestServer.TestApiKey);

        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task MissingApiKey_ShouldReturn_401()
    {
        var response = await _client.GetAsync(AuthTestServer.ApiKeyRoute);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task InvalidApiKey_ShouldReturn_401()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, AuthTestServer.ApiKeyRoute);
        request.Headers.Add(ApiKeyAuthenticationOptions.DefaultHeaderName, "wrong-key-value");

        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    public void Dispose() => _client.Dispose();
}
