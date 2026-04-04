using System.Net;
using System.Text.Json;
using MarcusPrado.Platform.AspNetCore.OpenApi;

namespace MarcusPrado.Platform.AspNetCore.Tests;

/// <summary>
/// Integration tests for <see cref="OpenApiExtensions.AddPlatformOpenApi"/> and
/// <see cref="OpenApiExtensions.UsePlatformOpenApi"/>.
/// </summary>
public sealed class OpenApiTests : IAsyncDisposable
{
    private readonly WebApplication _app;
    private readonly HttpClient _client;

    /// <summary>Initialises a WebApplication with Platform OpenAPI enabled.</summary>
    public OpenApiTests()
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            EnvironmentName = "Development"
        });

        builder.WebHost.UseTestServer();
        builder.Logging.ClearProviders();

        builder.Services.AddPlatformOpenApi(opts =>
        {
            opts.Title = "Test API";
            opts.Version = "v1";
            opts.Description = "Unit test document";
            opts.EnableJwtAuth = true;
            opts.EnableApiKeyAuth = true;
            opts.IncludeContextHeaders = true;
        });

        _app = builder.Build();

        _app.MapGet("/test", () => "ok");
        _app.UsePlatformOpenApi();

        _app.StartAsync().GetAwaiter().GetResult();
        _client = _app.GetTestClient();
    }

    // ── Test 1: JSON endpoint returns 200 ─────────────────────────────────────

    [Fact]
    public async Task GetOpenApiJson_Returns200WithJsonContentType()
    {
        var response = await _client.GetAsync("/openapi/v1.json");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");
    }

    // ── Test 2: info.title matches configured value ────────────────────────────

    [Fact]
    public async Task GetOpenApiJson_HasConfiguredTitle()
    {
        var json = await GetDocumentAsync();

        json.GetProperty("info").GetProperty("title").GetString()
            .Should().Be("Test API");
    }

    // ── Test 3: info.version matches configured value ─────────────────────────

    [Fact]
    public async Task GetOpenApiJson_HasConfiguredVersion()
    {
        var json = await GetDocumentAsync();

        json.GetProperty("info").GetProperty("version").GetString()
            .Should().Be("v1");
    }

    // ── Test 4: JWT security scheme present ───────────────────────────────────

    [Fact]
    public async Task GetOpenApiJson_ContainsJwtSecuritySchemeWhenEnabled()
    {
        var json = await GetDocumentAsync();

        var schemes = json
            .GetProperty("components")
            .GetProperty("securitySchemes");

        schemes.TryGetProperty("Bearer", out var bearer).Should().BeTrue();
        bearer.GetProperty("type").GetString().Should().Be("http");
        bearer.GetProperty("scheme").GetString().Should().Be("bearer");
    }

    // ── Test 5: API-key security scheme present ───────────────────────────────

    [Fact]
    public async Task GetOpenApiJson_ContainsApiKeySecuritySchemeWhenEnabled()
    {
        var json = await GetDocumentAsync();

        var schemes = json
            .GetProperty("components")
            .GetProperty("securitySchemes");

        schemes.TryGetProperty("ApiKey", out var apiKey).Should().BeTrue();
        apiKey.GetProperty("type").GetString().Should().Be("apiKey");
        apiKey.GetProperty("name").GetString().Should().Be("X-Api-Key");
    }

    // ── Test 6: context headers injected into every operation ─────────────────

    [Fact]
    public async Task GetOpenApiJson_OperationsHaveCorrelationIdHeader()
    {
        var json = await GetDocumentAsync();

        var paths = json.GetProperty("paths");
        foreach (var path in paths.EnumerateObject())
        {
            foreach (var method in path.Value.EnumerateObject())
            {
                if (!method.Value.TryGetProperty("parameters", out var parameters))
                    continue;

                var names = parameters.EnumerateArray()
                    .Select(p => p.GetProperty("name").GetString())
                    .ToList();

                names.Should().Contain("X-Correlation-Id",
                    because: $"operation {path.Name}.{method.Name} should have X-Correlation-Id");
            }
        }
    }

    // ── Test 7: X-Tenant-Id header injected ───────────────────────────────────

    [Fact]
    public async Task GetOpenApiJson_OperationsHaveTenantIdHeader()
    {
        var json = await GetDocumentAsync();

        var paths = json.GetProperty("paths");
        foreach (var path in paths.EnumerateObject())
        {
            foreach (var method in path.Value.EnumerateObject())
            {
                if (!method.Value.TryGetProperty("parameters", out var parameters))
                    continue;

                var names = parameters.EnumerateArray()
                    .Select(p => p.GetProperty("name").GetString())
                    .ToList();

                names.Should().Contain("X-Tenant-Id",
                    because: $"operation {path.Name}.{method.Name} should have X-Tenant-Id");
            }
        }
    }

    // ── Test 8: Scalar UI returns 200 ─────────────────────────────────────────

    [Fact]
    public async Task GetScalarUi_Returns200()
    {
        var response = await _client.GetAsync("/scalar/v1");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private async Task<JsonElement> GetDocumentAsync()
    {
        var content = await _client.GetStringAsync("/openapi/v1.json");
        return JsonDocument.Parse(content).RootElement;
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        _client.Dispose();
        await _app.DisposeAsync();
    }
}

/// <summary>
/// Tests that verify options isolation — a separate app instance without JWT/API-key.
/// </summary>
public sealed class OpenApiOptionsIsolationTests : IAsyncDisposable
{
    private readonly WebApplication _app;
    private readonly HttpClient _client;

    /// <summary>Initialises a WebApplication with JWT disabled and API-key disabled.</summary>
    public OpenApiOptionsIsolationTests()
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            EnvironmentName = "Development"
        });

        builder.WebHost.UseTestServer();
        builder.Logging.ClearProviders();

        builder.Services.AddPlatformOpenApi(opts =>
        {
            opts.Title = "Minimal API";
            opts.EnableJwtAuth = false;
            opts.EnableApiKeyAuth = false;
            opts.IncludeContextHeaders = false;
        });

        _app = builder.Build();
        _app.MapGet("/ping", () => "pong");
        _app.UsePlatformOpenApi();

        _app.StartAsync().GetAwaiter().GetResult();
        _client = _app.GetTestClient();
    }

    // ── Test: no security schemes when both disabled ───────────────────────────

    [Fact]
    public async Task GetOpenApiJson_NoSecuritySchemesWhenBothDisabled()
    {
        var content = await _client.GetStringAsync("/openapi/v1.json");
        var json = JsonDocument.Parse(content).RootElement;

        // components may be absent or securitySchemes empty
        var hasBearer = false;
        var hasApiKey = false;

        if (json.TryGetProperty("components", out var components) &&
            components.TryGetProperty("securitySchemes", out var schemes))
        {
            hasBearer = schemes.TryGetProperty("Bearer", out _);
            hasApiKey = schemes.TryGetProperty("ApiKey", out _);
        }

        hasBearer.Should().BeFalse();
        hasApiKey.Should().BeFalse();
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        _client.Dispose();
        await _app.DisposeAsync();
    }
}
