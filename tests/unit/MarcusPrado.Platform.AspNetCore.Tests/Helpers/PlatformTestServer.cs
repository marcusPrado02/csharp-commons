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
                services.AddPlatformSecurityHeaders();
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
                            throw new NotFoundException("RESOURCE.NOT_FOUND", "Resource not found");

                        case "/error/conflict":
                            throw new ConflictException("RESOURCE.CONFLICT", "Resource already exists");

                        case "/error/unauth":
                            throw new UnauthorizedException("AUTH.UNAUTHORIZED", "Not authenticated");

                        case "/error/forbidden":
                            throw new ForbiddenException("AUTH.FORBIDDEN", "Access denied");

                        case "/error/validation":
                            var errors = new List<Error>
                            {
                                Error.Validation("VALIDATION.NAME", "Name is required", "name"),
                                Error.Validation("VALIDATION.EMAIL", "Email is invalid", "email"),
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

    /// <summary>
    /// Creates a <see cref="TestServer"/> with custom <see cref="Security.SecurityHeadersOptions"/>
    /// so individual header-toggle tests can be isolated.
    /// </summary>
    internal static TestServer CreateWithSecurityOptions(
        Action<Security.SecurityHeadersOptions>? configure = null)
    {
        var builder = new WebHostBuilder()
            .UseEnvironment("Test")
            .ConfigureLogging(l => l.ClearProviders())
            .ConfigureServices(services =>
            {
                services.AddRouting();
                services.AddPlatformCore();
                services.AddPlatformSecurityHeaders(configure);
            })
            .Configure(app =>
            {
                app.UsePlatformMiddlewares();
                app.Run(async ctx =>
                {
                    ctx.Response.StatusCode = 200;
                    await ctx.Response.WriteAsync("ok");
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
