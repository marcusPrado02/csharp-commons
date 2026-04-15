using System.Net;
using System.Text.Json;
using MarcusPrado.Platform.ExceptionEnrichment;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MarcusPrado.Platform.ExceptionEnrichment.Tests;

public sealed class ExceptionEnrichmentTests
{
    // ── ExceptionFingerprinter ────────────────────────────────────────────────

    [Fact]
    public void ExceptionFingerprinter_SameException_ReturnsSameFingerprint()
    {
        Exception ex;
        try
        {
            throw new InvalidOperationException("test message");
        }
        catch (InvalidOperationException caught)
        {
            ex = caught;
        }

        var fp1 = ExceptionFingerprinter.GetFingerprint(ex);
        var fp2 = ExceptionFingerprinter.GetFingerprint(ex);

        fp1.Should().Be(fp2);
    }

    [Fact]
    public void ExceptionFingerprinter_DifferentMessage_ReturnsDifferentFingerprint()
    {
        Exception ex1;
        Exception ex2;

        try
        {
            throw new InvalidOperationException("message A");
        }
        catch (InvalidOperationException e)
        {
            ex1 = e;
        }

        try
        {
            throw new InvalidOperationException("message B");
        }
        catch (InvalidOperationException e)
        {
            ex2 = e;
        }

        var fp1 = ExceptionFingerprinter.GetFingerprint(ex1);
        var fp2 = ExceptionFingerprinter.GetFingerprint(ex2);

        fp1.Should().NotBe(fp2);
    }

    [Fact]
    public void ExceptionFingerprinter_NullException_ThrowsArgumentNullException()
    {
        var act = () => ExceptionFingerprinter.GetFingerprint(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    // ── ExceptionGrouper ─────────────────────────────────────────────────────

    [Fact]
    public void ExceptionGrouper_MultipleExceptions_GroupsByType()
    {
        // Create two exceptions with identical types and messages but no stack trace
        // so their fingerprints are deterministically equal.
        var exA1 = new InvalidOperationException("same");
        var exA2 = new InvalidOperationException("same");
        var exB = new ArgumentException("other");

        var groups = ExceptionGrouper.GroupByFingerprint([exA1, exA2, exB]);

        // exA1 and exA2 share the same fingerprint (same type + same message + no frame)
        // exB has its own fingerprint
        groups.Should().HaveCountGreaterThanOrEqualTo(2);
        groups.Values.Should().Contain(g => g.Count == 2);
    }

    [Fact]
    public void ExceptionGrouper_EmptyInput_ReturnsEmptyDictionary()
    {
        var groups = ExceptionGrouper.GroupByFingerprint([]);

        groups.Should().BeEmpty();
    }

    // ── DeveloperExceptionPageEnricher ───────────────────────────────────────

    [Fact]
    public async Task DeveloperExceptionPageEnricher_InDevelopment_Returns500WithJson()
    {
        using var host = await new HostBuilder()
            .ConfigureWebHost(web =>
            {
                web.UseTestServer();
                web.UseEnvironment(Environments.Development);
                web.ConfigureServices(services =>
                {
                    services.AddPlatformExceptionEnrichment();
                });
                web.Configure(app =>
                {
                    app.UsePlatformDeveloperExceptionPage();
                    app.Run(_ => throw new InvalidOperationException("boom"));
                });
            })
            .StartAsync();

        var client = host.GetTestClient();
        var response = await client.GetAsync("/test");

        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");

        var body = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        doc.RootElement.GetProperty("status").GetInt32().Should().Be(500);
        doc.RootElement.GetProperty("message").GetString().Should().Be("boom");
        doc.RootElement.TryGetProperty("fingerprint", out _).Should().BeTrue();
        doc.RootElement.TryGetProperty("stackTrace", out _).Should().BeTrue();
    }

    [Fact]
    public async Task DeveloperExceptionPageEnricher_NoException_PassesThrough()
    {
        using var host = await new HostBuilder()
            .ConfigureWebHost(web =>
            {
                web.UseTestServer();
                web.UseEnvironment(Environments.Development);
                web.ConfigureServices(services =>
                {
                    services.AddPlatformExceptionEnrichment();
                });
                web.Configure(app =>
                {
                    app.UsePlatformDeveloperExceptionPage();
                    app.Run(async ctx =>
                    {
                        ctx.Response.StatusCode = StatusCodes.Status200OK;
                        await ctx.Response.WriteAsync("ok");
                    });
                });
            })
            .StartAsync();

        var client = host.GetTestClient();
        var response = await client.GetAsync("/test");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Be("ok");
    }
}
