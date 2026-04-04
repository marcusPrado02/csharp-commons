using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Options;

namespace MarcusPrado.Platform.AspNetCore.Tests;

/// <summary>
/// Tests that <see cref="CompressionExtensions.AddPlatformResponseCompression"/>
/// registers the expected services and applies correct defaults.
/// </summary>
public sealed class CompressionExtensionsTests
{
    // ── Service registration ──────────────────────────────────────────────────

    [Fact]
    public void AddPlatformResponseCompression_RegistersResponseCompressionProvider()
    {
        var sp = BuildServiceProvider();

        var providers = sp.GetServices<IResponseCompressionProvider>();

        providers.Should().NotBeNullOrEmpty(
            because: "AddPlatformResponseCompression must register at least one IResponseCompressionProvider");
    }

    [Fact]
    public void AddPlatformResponseCompression_EnableForHttps_IsTrue()
    {
        var services = new ServiceCollection();
        // Capture the options that were applied inside AddResponseCompression
        ResponseCompressionOptions? captured = null;
        services.AddPlatformResponseCompression(opts => captured = opts);

        // Build and resolve IOptions<ResponseCompressionOptions> to read the configured value
        var sp = services.BuildServiceProvider();
        var opts = sp.GetRequiredService<IOptions<ResponseCompressionOptions>>().Value;

        opts.EnableForHttps.Should().BeTrue(
            because: "HTTPS compression must be enabled by default to compress API responses over TLS");
    }

    [Fact]
    public void AddPlatformResponseCompression_MimeTypes_ContainsApplicationJson()
    {
        var sp = BuildServiceProvider();
        var opts = sp.GetRequiredService<IOptions<ResponseCompressionOptions>>().Value;

        opts.MimeTypes.Should().Contain("application/json",
            because: "JSON is the primary API response format and should be compressed");
    }

    [Fact]
    public void AddPlatformResponseCompression_MimeTypes_ContainsApplicationXProtobuf()
    {
        var sp = BuildServiceProvider();
        var opts = sp.GetRequiredService<IOptions<ResponseCompressionOptions>>().Value;

        opts.MimeTypes.Should().Contain("application/x-protobuf",
            because: "Protobuf binary responses must also be eligible for compression");
    }

    [Fact]
    public void AddPlatformResponseCompression_CustomDelegate_IsApplied()
    {
        var sp = new ServiceCollection()
            .AddPlatformResponseCompression(opts => opts.EnableForHttps = false)
            .BuildServiceProvider();

        var opts = sp.GetRequiredService<IOptions<ResponseCompressionOptions>>().Value;

        opts.EnableForHttps.Should().BeFalse(
            because: "the caller-supplied configure delegate must override the defaults");
    }

    // ── Integration: actual response compression ──────────────────────────────

    [Fact]
    public async Task AddPlatformResponseCompression_BrotliEncoding_CompressesJsonResponse()
    {
        using var server = BuildCompressionTestServer();
        using var client = server.CreateClient();

        using var request = new HttpRequestMessage(HttpMethod.Get, "/data");
        request.Headers.Add("Accept-Encoding", "br");

        var response = await client.SendAsync(request);

        response.IsSuccessStatusCode.Should().BeTrue();
        response.Content.Headers.ContentEncoding.Should().Contain("br",
            because: "when the client sends Accept-Encoding: br, the server must use Brotli compression");
    }

    [Fact]
    public async Task AddPlatformResponseCompression_GzipEncoding_CompressesJsonResponse()
    {
        using var server = BuildCompressionTestServer();
        using var client = server.CreateClient();

        using var request = new HttpRequestMessage(HttpMethod.Get, "/data");
        request.Headers.Add("Accept-Encoding", "gzip");

        var response = await client.SendAsync(request);

        response.IsSuccessStatusCode.Should().BeTrue();
        response.Content.Headers.ContentEncoding.Should().Contain("gzip",
            because: "when the client sends Accept-Encoding: gzip, the server must use Gzip compression");
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static IServiceProvider BuildServiceProvider()
        => new ServiceCollection()
            .AddPlatformResponseCompression()
            .BuildServiceProvider();

    /// <summary>
    /// Creates an inline <see cref="TestServer"/> that uses
    /// <c>UseResponseCompression()</c> and serves a JSON body large enough
    /// to trigger compression (the middleware compresses bodies above ~150 B).
    /// </summary>
    private static TestServer BuildCompressionTestServer()
    {
        // A payload well above the built-in minimum-size threshold (~150 B).
        var largeJson = "{\"items\":[" +
            string.Join(",", Enumerable.Range(1, 50).Select(i => $"{{\"id\":{i},\"name\":\"item-{i}\"}}")) +
            "]}";

        var builder = new WebHostBuilder()
            .UseEnvironment("Test")
            .ConfigureLogging(l => l.ClearProviders())
            .ConfigureServices(services =>
            {
                services.AddRouting();
                services.AddPlatformResponseCompression();
            })
            .Configure(app =>
            {
                app.UseResponseCompression();
                app.Run(async ctx =>
                {
                    ctx.Response.StatusCode = 200;
                    ctx.Response.ContentType = "application/json";
                    await ctx.Response.WriteAsync(largeJson);
                });
            });

        return new TestServer(builder);
    }
}
