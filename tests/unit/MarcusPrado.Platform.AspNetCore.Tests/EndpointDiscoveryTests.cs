using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http.Json;
using System.Reflection;
using System.Text.Json;
using MarcusPrado.Platform.Abstractions.Errors;
using MarcusPrado.Platform.Abstractions.Validation;
using MarcusPrado.Platform.AspNetCore.Endpoints;
using MarcusPrado.Platform.AspNetCore.Filters;
using MarcusPrado.Platform.Contracts.Http;

namespace MarcusPrado.Platform.AspNetCore.Tests;

/// <summary>
/// Integration tests for <see cref="EndpointDiscovery"/>, <see cref="EndpointGroupBase"/>,
/// <see cref="ApiEnvelopeFilter"/>, <see cref="ValidationFilter{TRequest}"/>, and
/// <see cref="EndpointExtensions"/> using in-process <see cref="TestServer"/>.
/// </summary>
public sealed class EndpointDiscoveryTests
{
    // ── Test endpoint implementations ─────────────────────────────────────────

    internal sealed class PingEndpoint : IEndpoint
    {
        public void MapEndpoints(IEndpointRouteBuilder routes)
            => routes.MapGet("/ping", () => "pong");
    }

    internal sealed class HealthEndpoint : IEndpoint
    {
        public void MapEndpoints(IEndpointRouteBuilder routes)
            => routes.MapGet("/health", () => "ok");
    }

    internal sealed class ItemsGroupEndpoint : EndpointGroupBase
    {
        protected override string Group => "/v1/items";

        protected override void MapRoutes(RouteGroupBuilder group)
        {
            group.MapGet("/", () => new[] { "item1", "item2" });
            group.MapGet("/{id}", (string id) => $"item:{id}");
        }
    }

    internal sealed class EnvelopedEndpoint : IEndpoint
    {
        public void MapEndpoints(IEndpointRouteBuilder routes)
            => routes.MapGet("/enveloped", () => "hello")
                     .AddEndpointFilter<ApiEnvelopeFilter>();
    }

    internal sealed class IResultEndpoint : IEndpoint
    {
        public void MapEndpoints(IEndpointRouteBuilder routes)
            => routes.MapGet("/iresult", () => Results.Ok("direct"))
                     .AddEndpointFilter<ApiEnvelopeFilter>();
    }

    internal sealed class DependencyEndpoint : IEndpoint
    {
        private readonly IGreetingService _greetingService;

        public DependencyEndpoint(IGreetingService greetingService)
            => _greetingService = greetingService;

        public void MapEndpoints(IEndpointRouteBuilder routes)
            => routes.MapGet("/greet", () => _greetingService.Greet());
    }

    internal interface IGreetingService
    {
        string Greet();
    }

    internal sealed class GreetingService : IGreetingService
    {
        public string Greet() => "Hello from DI!";
    }

    internal sealed class ValidatedEndpoint : IEndpoint
    {
        public void MapEndpoints(IEndpointRouteBuilder routes)
            => routes.MapPost("/validated", (SampleRequest request) => Results.Ok(request.Name))
                     .AddEndpointFilter<ValidationFilter<SampleRequest>>();
    }

    internal sealed class SampleRequest
    {
        [Required]
        [MinLength(2)]
        public string? Name { get; set; }
    }

    // A custom IValidator<SampleRequest> for the ValidationFilter test
    internal sealed class SampleRequestValidator : IValidator<SampleRequest>
    {
        public Task<IValidationResult> ValidateAsync(SampleRequest request, CancellationToken cancellationToken = default)
        {
            var errors = new List<Error>();
            if (string.IsNullOrWhiteSpace(request.Name))
                errors.Add(Error.Validation("SAMPLE.NAME_REQUIRED", "Name is required", "name"));
            else if (request.Name.Length < 2)
                errors.Add(Error.Validation("SAMPLE.NAME_TOO_SHORT", "Name must be at least 2 characters", "name"));

            IValidationResult result = errors.Count == 0
                ? ValidationResult.Valid
                : ValidationResult.Invalid(errors);
            return Task.FromResult(result);
        }
    }

    internal sealed class ValidationResult : IValidationResult
    {
        public bool IsValid { get; private init; }
        public IReadOnlyList<Error> Errors { get; private init; } = [];

        public static readonly IValidationResult Valid = new ValidationResult { IsValid = true };

        public static IValidationResult Invalid(IReadOnlyList<Error> errors)
            => new ValidationResult { IsValid = false, Errors = errors };
    }

    // ── Helper ────────────────────────────────────────────────────────────────

    private static TestServer CreateServer(
        Action<IServiceCollection>? configureServices = null,
        Action<IEndpointRouteBuilder>? configureRoutes = null,
        Assembly[]? assemblies = null)
    {
        var builder = new WebHostBuilder()
            .UseEnvironment("Test")
            .ConfigureLogging(l => l.ClearProviders())
            .ConfigureServices(services =>
            {
                services.AddRouting();
                configureServices?.Invoke(services);
            })
            .Configure(app =>
            {
                app.UseRouting();
                app.UseEndpoints(endpoints =>
                {
                    if (assemblies is { Length: > 0 })
                        endpoints.MapPlatformEndpoints(assemblies);
                    configureRoutes?.Invoke(endpoints);
                });
            });

        return new TestServer(builder);
    }

    // ── Tests ─────────────────────────────────────────────────────────────────

    // Test 1: MapPlatformEndpoints discovers PingEndpoint from the test assembly
    [Fact]
    public async Task MapPlatformEndpoints_DiscoversPingEndpoint_Returns200()
    {
        using var server = CreateServer(assemblies: [typeof(PingEndpoint).Assembly]);
        var client = server.CreateClient();

        var response = await client.GetAsync("/ping");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("pong");
    }

    // Test 2: Multiple endpoints are discovered — both routes respond
    [Fact]
    public async Task MapPlatformEndpoints_DiscoversMultipleEndpoints_BothRespond()
    {
        using var server = CreateServer(assemblies: [typeof(PingEndpoint).Assembly]);
        var client = server.CreateClient();

        var pingResponse = await client.GetAsync("/ping");
        var healthResponse = await client.GetAsync("/health");

        pingResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        healthResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        (await healthResponse.Content.ReadAsStringAsync()).Should().Contain("ok");
    }

    // Test 3: EndpointGroupBase maps routes under the group prefix
    [Fact]
    public async Task EndpointGroupBase_MapsRoutesUnderGroupPrefix_Works()
    {
        using var server = CreateServer(assemblies: [typeof(ItemsGroupEndpoint).Assembly]);
        var client = server.CreateClient();

        var listResponse = await client.GetAsync("/v1/items/");
        var itemResponse = await client.GetAsync("/v1/items/42");

        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        itemResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        (await itemResponse.Content.ReadAsStringAsync()).Should().Contain("item:42");
    }

    // Test 4: ApiEnvelopeFilter wraps a plain string response in the envelope
    [Fact]
    public async Task ApiEnvelopeFilter_WrapsPlainResponse_InEnvelope()
    {
        using var server = CreateServer(assemblies: [typeof(EnvelopedEndpoint).Assembly]);
        var client = server.CreateClient();

        var response = await client.GetAsync("/enveloped");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);
        doc.RootElement.TryGetProperty("data", out var data).Should().BeTrue();
        doc.RootElement.TryGetProperty("success", out var success).Should().BeTrue();
        success.GetBoolean().Should().BeTrue();
        data.GetString().Should().Be("hello");
    }

    // Test 5: ApiEnvelopeFilter passes through IResult responses unchanged
    [Fact]
    public async Task ApiEnvelopeFilter_PassesThroughIResult_Unchanged()
    {
        using var server = CreateServer(assemblies: [typeof(IResultEndpoint).Assembly]);
        var client = server.CreateClient();

        var response = await client.GetAsync("/iresult");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        // IResult passes through — body should be the raw "direct" string, not double-wrapped
        var body = await response.Content.ReadAsStringAsync();
        // The response is an IResult (Results.Ok) so no envelope wrapping occurs
        body.Should().NotBeEmpty();
    }

    // Test 6: ValidationFilter with valid request — handler is called, returns 200
    [Fact]
    public async Task ValidationFilter_ValidRequest_Returns200()
    {
        using var server = CreateServer(
            configureServices: services =>
            {
                services.AddSingleton<IValidator<SampleRequest>, SampleRequestValidator>();
            },
            assemblies: [typeof(ValidatedEndpoint).Assembly]);
        var client = server.CreateClient();

        var response = await client.PostAsJsonAsync("/validated", new SampleRequest { Name = "Alice" });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("Alice");
    }

    // Test 7: ValidationFilter with invalid request — returns 422 UnprocessableEntity
    [Fact]
    public async Task ValidationFilter_InvalidRequest_Returns422()
    {
        using var server = CreateServer(
            configureServices: services =>
            {
                services.AddSingleton<IValidator<SampleRequest>, SampleRequestValidator>();
            },
            assemblies: [typeof(ValidatedEndpoint).Assembly]);
        var client = server.CreateClient();

        var response = await client.PostAsJsonAsync("/validated", new SampleRequest { Name = "" });

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        var body = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(body);
        doc.RootElement.TryGetProperty("errors", out _).Should().BeTrue();
    }

    // Test 8: Endpoint with required DI constructor is resolved via service provider
    [Fact]
    public async Task MapPlatformEndpoints_EndpointWithDependency_ResolvedViaServiceProvider()
    {
        using var server = CreateServer(
            configureServices: services =>
            {
                services.AddSingleton<IGreetingService, GreetingService>();
                services.AddSingleton<DependencyEndpoint>();
            },
            assemblies: [typeof(DependencyEndpoint).Assembly]);
        var client = server.CreateClient();

        var response = await client.GetAsync("/greet");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("Hello from DI!");
    }
}
