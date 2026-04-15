using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using MarcusPrado.Platform.Abstractions.Errors;
using MarcusPrado.Platform.Application.Errors;
using MarcusPrado.Platform.AspNetCore.ProblemDetails.Extensions;
using MarcusPrado.Platform.AspNetCore.ProblemDetails.Mappers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace MarcusPrado.Platform.AspNetCore.ProblemDetails.Tests;

/// <summary>
/// Integration tests for the platform ProblemDetails middleware (item #11).
/// </summary>
public sealed class ProblemDetailsMiddlewareTests : IDisposable
{
    // ── Server factory ────────────────────────────────────────────────────────

    private static HttpClient BuildClient(Exception toThrow)
    {
        var builder = new WebHostBuilder()
            .ConfigureServices(s =>
            {
                s.AddPlatformProblemDetails();
                s.AddLogging();
                s.AddRouting();
            })
            .Configure(app =>
            {
                app.UsePlatformProblemDetails();
                app.UseRouting();
                app.UseEndpoints(e =>
                    e.MapGet("/throw", _ => throw toThrow));
            });

        return new TestServer(builder).CreateClient();
    }

    private readonly HttpClient _notFound = BuildClient(new NotFoundException("ORDER.NOT_FOUND", "Order not found."));
    private readonly HttpClient _conflict = BuildClient(new ConflictException("RESOURCE.CONFLICT", "Resource already exists."));
    private readonly HttpClient _unauth = BuildClient(new UnauthorizedException("AUTH.UNAUTHORIZED", "Not authenticated."));
    private readonly HttpClient _forbidden = BuildClient(new ForbiddenException("AUTH.FORBIDDEN", "Access denied."));
    private readonly HttpClient _validation = BuildClient(new ValidationException(
        new[] { Error.Validation("VALIDATION.NAME", "Required", "Name") }));
    private readonly HttpClient _appEx = BuildClient(new AppException(Error.Validation("APP.ERROR", "Generic app error.")));
    private readonly HttpClient _unexpected = BuildClient(new InvalidOperationException("Boom."));

    // ── Status code mapping ───────────────────────────────────────────────────

    [Fact] public async Task NotFoundException_Returns404() => (await _notFound.GetAsync("/throw")).StatusCode.Should().Be(HttpStatusCode.NotFound);
    [Fact] public async Task ConflictException_Returns409() => (await _conflict.GetAsync("/throw")).StatusCode.Should().Be(HttpStatusCode.Conflict);
    [Fact] public async Task UnauthorizedException_Returns401() => (await _unauth.GetAsync("/throw")).StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    [Fact] public async Task ForbiddenException_Returns403() => (await _forbidden.GetAsync("/throw")).StatusCode.Should().Be(HttpStatusCode.Forbidden);
    [Fact] public async Task ValidationException_Returns422() => (await _validation.GetAsync("/throw")).StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    [Fact] public async Task AppException_Returns422() => (await _appEx.GetAsync("/throw")).StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    [Fact] public async Task UnknownException_Returns500() => (await _unexpected.GetAsync("/throw")).StatusCode.Should().Be(HttpStatusCode.InternalServerError);

    // ── Content-Type ──────────────────────────────────────────────────────────

    [Fact]
    public async Task ProblemDetails_HasCorrectContentType()
    {
        var response = await _notFound.GetAsync("/throw");
        response.Content.Headers.ContentType!.MediaType.Should().Be("application/problem+json");
    }

    // ── ProblemDetails body ───────────────────────────────────────────────────

    [Fact]
    public async Task NotFoundException_HasCorrectTitleAndDetail()
    {
        var response = await _notFound.GetAsync("/throw");
        var pd = await response.Content.ReadFromJsonAsync<JsonDocument>();
        pd!.RootElement.GetProperty("title").GetString().Should().Be("Not Found");
        pd.RootElement.GetProperty("detail").GetString().Should().Be("Order not found.");
        pd.RootElement.GetProperty("status").GetInt32().Should().Be(404);
    }

    [Fact]
    public async Task ValidationException_IncludesErrors()
    {
        var response = await _validation.GetAsync("/throw");
        var pd = await response.Content.ReadFromJsonAsync<JsonDocument>();
        pd!.RootElement.TryGetProperty("errors", out _).Should().BeTrue();
    }

    // ── ExceptionMapper unit tests ────────────────────────────────────────────

    [Theory]
    [InlineData(404, "https://tools.ietf.org/html/rfc9110#section-15.5.5")]
    [InlineData(401, "https://tools.ietf.org/html/rfc9110#section-15.5.2")]
    [InlineData(500, "https://tools.ietf.org/html/rfc9110#section-15.6.1")]
    public void GetProblemTypeUri_ReturnsCorrectUri(int status, string expected)
        => ExceptionMapper.GetProblemTypeUri(status).Should().Be(expected);

    public void Dispose()
    {
        _notFound.Dispose();
        _conflict.Dispose();
        _unauth.Dispose();
        _forbidden.Dispose();
        _validation.Dispose();
        _appEx.Dispose();
        _unexpected.Dispose();
    }
}
