using System.Text.Json;

namespace MarcusPrado.Platform.AspNetCore.Tests.Helpers;

/// <summary>
/// Creates an in-process <see cref="TestServer"/> with the full platform
/// middleware stack registered.  Test routes are mapped via
/// <paramref name="configureRoutes"/> so each test class can express exactly
/// what endpoints it needs.
/// </summary>
internal static class PlatformTestServer
{
    // ── Test routes ────────────────────────────────────────────────────────────
    //   /ping              → 200 "pong"
    //   /correlation       → 200 "{correlationId}|{requestId}"
    //   /tenant            → 200 "{tenantId}" (or "null")
    //   /error/notfound    → throws NotFoundException
    //   /error/conflict    → throws ConflictException
    //   /error/unauth      → throws UnauthorizedException
    //   /error/forbidden   → throws ForbiddenException
    //   /error/validation  → throws ValidationException with field errors
    //   /error/unhandled   → throws InvalidOperationException (untyped)

    /// <summary>
    /// Creates and returns a <see cref="TestServer"/> configured with
    /// <see cref="ServiceCollectionExtensions.AddPlatformCore"/> and
    /// <see cref="WebApplicationExtensions.UsePlatformMiddlewares"/>.
    /// </summary>
    internal static TestServer Create()
    {
        var builder = new WebHostBuilder()
            .UseEnvironment("Test")
            .ConfigureLogging(l => l.ClearProviders())
            .ConfigureServices(services =>
            {
                services.AddRouting();
                services.AddPlatformCore();
            })
            .Configure(app =>
            {
                app.UsePlatformMiddlewares();

                app.Run(async ctx =>
                {
                    switch (ctx.Request.Path.Value)
                    {
                        case "/ping":
                            ctx.Response.StatusCode = 200;
                            await ctx.Response.WriteAsync("pong");
                            break;

                        case "/correlation":
                            var correlCtx = ctx.RequestServices
                                .GetRequiredService<ICorrelationContext>();
                            ctx.Response.StatusCode = 200;
                            await ctx.Response.WriteAsync(
                                $"{correlCtx.CorrelationId}|{correlCtx.RequestId}");
                            break;

                        case "/tenant":
                            var tenantCtx = ctx.RequestServices
                                .GetRequiredService<ITenantContext>();
                            ctx.Response.StatusCode = 200;
                            await ctx.Response.WriteAsync(tenantCtx.TenantId ?? "null");
                            break;

                        case "/error/notfound":
                            throw new NotFoundException("Resource not found");

                        case "/error/conflict":
                            throw new ConflictException("Resource already exists");

                        case "/error/unauth":
                            throw new UnauthorizedException("Not authenticated");

                        case "/error/forbidden":
                            throw new ForbiddenException("Access denied");

                        case "/error/validation":
                            var errors = new Dictionary<string, string[]>(
                                StringComparer.OrdinalIgnoreCase)
                            {
                                ["name"]  = ["Name is required"],
                                ["email"] = ["Email is invalid"],
                            };
                            throw new ValidationException(errors);

                        case "/error/unhandled":
                            throw new InvalidOperationException("Unexpected failure");

                        default:
                            ctx.Response.StatusCode = 404;
                            break;
                    }
                });
            });

        return new TestServer(builder);
    }

    /// <summary>Reads the response body as a JSON document.</summary>
    internal static async Task<JsonDocument> ReadJsonAsync(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        return JsonDocument.Parse(content);
    }
}
