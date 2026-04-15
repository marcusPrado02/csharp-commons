using System.Net;
using System.Text.Json;
using Asp.Versioning;
using MarcusPrado.Platform.AspNetCore.Versioning;

namespace MarcusPrado.Platform.AspNetCore.Tests;

/// <summary>
/// Tests for API versioning: <see cref="ApiVersioningExtensions.AddPlatformApiVersioning"/>,
/// <see cref="DeprecationHeaderMiddleware"/> and <see cref="ApiVersionDiscoveryEndpoint"/>.
/// </summary>
public sealed class ApiVersioningTests
{
    // ── Helpers ───────────────────────────────────────────────────────────────

    /// <summary>Creates a test server with API versioning and a simple versioned route.</summary>
    private static TestServer CreateVersionedServer()
    {
        var builder = new WebHostBuilder()
            .UseEnvironment("Test")
            .ConfigureLogging(l => l.ClearProviders())
            .ConfigureServices(services =>
            {
                services.AddRouting();
                services.AddPlatformApiVersioning();
            })
            .Configure(app =>
            {
                app.UseRouting();
                app.UseEndpoints(endpoints =>
                {
                    endpoints
                        .MapGet(
                            "/v{version:apiVersion}/test",
                            (HttpContext ctx) =>
                            {
                                ctx.Response.StatusCode = 200;
                                return ctx.Response.WriteAsync("ok");
                            }
                        )
                        .WithApiVersionSet(
                            endpoints
                                .NewApiVersionSet()
                                .HasApiVersion(new ApiVersion(1, 0))
                                .HasApiVersion(new ApiVersion(2, 0))
                                .Build()
                        );
                });
            });

        return new TestServer(builder);
    }

    /// <summary>Creates a test server with only the deprecation middleware (no full versioning).</summary>
    private static TestServer CreateDeprecationServer(Action<DeprecationOptions>? configure = null)
    {
        var builder = new WebHostBuilder()
            .UseEnvironment("Test")
            .ConfigureLogging(l => l.ClearProviders())
            .ConfigureServices(services =>
            {
                services.AddRouting();
                services.AddPlatformDeprecation(configure);
            })
            .Configure(app =>
            {
                app.UseDeprecationHeaders();
                app.Run(async ctx =>
                {
                    ctx.Response.StatusCode = 200;
                    await ctx.Response.WriteAsync("ok");
                });
            });

        return new TestServer(builder);
    }

    /// <summary>Creates a test server with the deprecation middleware on top of full versioning.</summary>
    private static TestServer CreateDeprecationWithVersioningServer(Action<DeprecationOptions>? configure = null)
    {
        var builder = new WebHostBuilder()
            .UseEnvironment("Test")
            .ConfigureLogging(l => l.ClearProviders())
            .ConfigureServices(services =>
            {
                services.AddRouting();
                services.AddPlatformApiVersioning();
                services.AddPlatformDeprecation(configure);
            })
            .Configure(app =>
            {
                app.UseRouting();
                app.UseDeprecationHeaders();
                app.UseEndpoints(endpoints =>
                {
                    endpoints
                        .MapGet(
                            "/v{version:apiVersion}/test",
                            (HttpContext ctx) =>
                            {
                                ctx.Response.StatusCode = 200;
                                return ctx.Response.WriteAsync("ok");
                            }
                        )
                        .WithApiVersionSet(
                            endpoints
                                .NewApiVersionSet()
                                .HasApiVersion(new ApiVersion(1, 0))
                                .HasApiVersion(new ApiVersion(2, 0))
                                .ReportApiVersions()
                                .Build()
                        );
                });
            });

        return new TestServer(builder);
    }

    // ── Tests: API Versioning ─────────────────────────────────────────────────

    [Fact]
    public async Task ApiVersion_Header_ReturnsApiSupportedVersionsHeader()
    {
        // Arrange
        using var server = CreateVersionedServer();
        var client = server.CreateClient();
        var request = new HttpRequestMessage(HttpMethod.Get, "/v1.0/test");
        request.Headers.Add("api-version", "1.0");

        // Act
        var response = await client.SendAsync(request);

        // Assert — the api-supported-versions header must be present
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response
            .Headers.Should()
            .ContainKey("api-supported-versions", because: "AddPlatformApiVersioning sets ReportApiVersions = true");
    }

    [Fact]
    public async Task NoVersionInRequest_DefaultsToV1_Returns200()
    {
        // Arrange — request a header-versioned endpoint without specifying a version;
        // AssumeDefaultVersionWhenUnspecified = true should route to v1.0.
        var builder = new WebHostBuilder()
            .UseEnvironment("Test")
            .ConfigureLogging(l => l.ClearProviders())
            .ConfigureServices(services =>
            {
                services.AddRouting();
                services.AddPlatformApiVersioning();
            })
            .Configure(app =>
            {
                app.UseRouting();
                app.UseEndpoints(endpoints =>
                {
                    var versionSet = endpoints
                        .NewApiVersionSet()
                        .HasApiVersion(new ApiVersion(1, 0))
                        .ReportApiVersions()
                        .Build();

                    // Header-versioned route — no version in URL, so default must apply.
                    endpoints
                        .MapGet(
                            "/ping",
                            (HttpContext ctx) =>
                            {
                                ctx.Response.StatusCode = 200;
                                return ctx.Response.WriteAsync("pong");
                            }
                        )
                        .WithApiVersionSet(versionSet);
                });
            });

        using var server = new TestServer(builder);
        var client = server.CreateClient();

        // Act — no api-version header or segment
        var response = await client.GetAsync("/ping");

        // Assert
        response
            .StatusCode.Should()
            .Be(
                HttpStatusCode.OK,
                because: "AssumeDefaultVersionWhenUnspecified = true allows requests without a version"
            );
    }

    // ── Tests: DeprecationHeaderMiddleware ────────────────────────────────────

    [Fact]
    public async Task DeprecationMiddleware_DeprecatedVersion_AddsDeprecationHeader()
    {
        // Arrange — use full versioning so IApiVersioningFeature is populated
        var deprecationDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        using var server = CreateDeprecationWithVersioningServer(opts =>
        {
            opts.DeprecatedVersions["1.0"] = (deprecationDate, null);
        });
        var client = server.CreateClient();

        // Act
        var response = await client.GetAsync("/v1.0/test");

        // Assert
        response.Headers.Should().ContainKey("Deprecation", because: "version 1.0 is registered as deprecated");
        response.Headers.GetValues("Deprecation").First().Should().Be(deprecationDate.ToString("R"));
    }

    [Fact]
    public async Task DeprecationMiddleware_NonDeprecatedVersion_NoDeprecationHeader()
    {
        // Arrange
        var deprecationDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        using var server = CreateDeprecationWithVersioningServer(opts =>
        {
            opts.DeprecatedVersions["1.0"] = (deprecationDate, null);
        });
        var client = server.CreateClient();

        // Act — request v2.0 which is NOT deprecated
        var response = await client.GetAsync("/v2.0/test");

        // Assert
        response.Headers.Should().NotContainKey("Deprecation", because: "version 2.0 is not deprecated");
    }

    [Fact]
    public async Task DeprecationMiddleware_DeprecatedWithSunset_AddsBothHeaders()
    {
        // Arrange
        var deprecationDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var sunsetDate = new DateTimeOffset(2025, 6, 30, 0, 0, 0, TimeSpan.Zero);
        using var server = CreateDeprecationWithVersioningServer(opts =>
        {
            opts.DeprecatedVersions["1.0"] = (deprecationDate, sunsetDate);
        });
        var client = server.CreateClient();

        // Act
        var response = await client.GetAsync("/v1.0/test");

        // Assert
        response.Headers.Should().ContainKey("Deprecation");
        response.Headers.Should().ContainKey("Sunset", because: "a sunset date was configured for version 1.0");
        response.Headers.GetValues("Sunset").First().Should().Be(sunsetDate.ToString("R"));
    }

    [Fact]
    public async Task DeprecationMiddleware_NoVersionFeature_NoHeaderAdded()
    {
        // Arrange — server without versioning: IApiVersioningFeature will be null.
        // The middleware must handle this gracefully (no crash, no headers).
        using var server = CreateDeprecationServer(opts =>
        {
            opts.DeprecatedVersions["1.0"] = (DateTimeOffset.UtcNow, null);
        });
        var client = server.CreateClient();

        // Act
        var response = await client.GetAsync("/");

        // Assert
        response
            .StatusCode.Should()
            .Be(HttpStatusCode.OK, because: "the middleware must not throw when IApiVersioningFeature is absent");
        response
            .Headers.Should()
            .NotContainKey("Deprecation", because: "no version feature → no version can be matched");
    }

    // ── Tests: ApiVersionDiscoveryEndpoint ────────────────────────────────────

    [Fact]
    public async Task ApiVersionDiscovery_ReturnsVersionsAndDeprecatedArrays()
    {
        // Arrange
        var builder = new WebHostBuilder()
            .UseEnvironment("Test")
            .ConfigureLogging(l => l.ClearProviders())
            .ConfigureServices(services =>
            {
                services.AddRouting();
                services.AddApiVersionDiscovery(opts =>
                {
                    opts.SupportedVersions = ["1.0", "2.0"];
                    opts.DeprecatedVersions = ["1.0"];
                });
            })
            .Configure(app =>
            {
                app.UseRouting();
                app.UseEndpoints(endpoints =>
                {
                    new ApiVersionDiscoveryEndpoint().MapEndpoints(endpoints);
                });
            });

        using var server = new TestServer(builder);
        var client = server.CreateClient();

        // Act
        var response = await client.GetAsync("/api-versions");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(body);

        doc.RootElement.GetProperty("versions")
            .EnumerateArray()
            .Select(e => e.GetString())
            .Should()
            .BeEquivalentTo(["1.0", "2.0"]);

        doc.RootElement.GetProperty("deprecated")
            .EnumerateArray()
            .Select(e => e.GetString())
            .Should()
            .BeEquivalentTo(["1.0"]);
    }
}
