using MarcusPrado.Platform.Abstractions.Context;

namespace MarcusPrado.Platform.AspNetCore.Auth.Tests.Helpers;

/// <summary>
/// Factory that builds self-contained <see cref="TestServer"/> instances
/// pre-configured with the platform auth middleware and a minimal set of routes
/// used across the auth test suite.
/// </summary>
public static class AuthTestServer
{
    // ── Routes ───────────────────────────────────────────────────────────────
    public const string JwtInfoRoute = "/auth/jwt-info";
    public const string ApiKeyRoute = "/auth/apikey";
    public const string PermissionRoute = "/auth/permission";
    public const string ScopeRoute = "/auth/scope";
    public const string AnonymousRoute = "/auth/anon";

    // ── Test auth configuration ───────────────────────────────────────────────
    public const string TestApiKey = "test-api-key-abc123";
    public const string TestPermission = "read:users";
    public const string TestScope = "api:read";

    /// <summary>
    /// Creates a <see cref="TestServer"/> with both JWT and API-key schemes
    /// and permission / scope authorization policies wired up.
    /// </summary>
    public static TestServer Create(
        Action<JwtAuthenticationOptions>? configureJwt = null,
        Action<ApiKeyAuthenticationOptions>? configureApiKey = null)
    {
        var builder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services.AddPlatformAuth(
                    opts =>
                    {
                        opts.SigningKey = JwtTokenFactory.TestSigningKey;
                        opts.Issuer = JwtTokenFactory.TestIssuer;
                        opts.Audience = JwtTokenFactory.TestAudience;
                        opts.ValidateLifetime = true;
                        configureJwt?.Invoke(opts);
                    },
                    opts =>
                    {
                        opts.ValidKeys = [TestApiKey];
                        configureApiKey?.Invoke(opts);
                    });

                services.AddPlatformAuthorization();

                services.AddAuthorizationBuilder()
                    .AddPolicy("RequirePermission", policy =>
                        policy.AddRequirements(new PermissionRequirement(TestPermission)))
                    .AddPolicy("RequireScope", policy =>
                        policy.AddRequirements(new ScopeRequirement(TestScope)));

                services.AddRouting();
            })
            .Configure(app =>
            {
                app.UseRouting();
                app.UseAuthentication();
                app.UseAuthorization();
                app.UseEndpoints(endpoints =>
                {
                    // Returns the user-id from IUserContext — requires JWT
                    endpoints.MapGet(JwtInfoRoute, async (HttpContext ctx) =>
                    {
                        if (!ctx.User.Identity!.IsAuthenticated)
                        {
                            ctx.Response.StatusCode = 401;
                            return;
                        }
                        var userCtx = ctx.RequestServices.GetRequiredService<IUserContext>();
                        await ctx.Response.WriteAsync(userCtx.UserId ?? "null");
                    }).RequireAuthorization(new AuthorizeAttribute { AuthenticationSchemes = PlatformAuthSchemes.Jwt });

                    // Requires a valid API key header
                    endpoints.MapGet(ApiKeyRoute, () => "ok")
                        .RequireAuthorization(new AuthorizeAttribute { AuthenticationSchemes = PlatformAuthSchemes.ApiKey });

                    // Requires "read:users" permission
                    endpoints.MapGet(PermissionRoute, () => "ok")
                        .RequireAuthorization("RequirePermission");

                    // Requires "api:read" scope
                    endpoints.MapGet(ScopeRoute, () => "ok")
                        .RequireAuthorization("RequireScope");

                    // Completely open endpoint
                    endpoints.MapGet(AnonymousRoute, () => "anon");
                });
            });

        return new TestServer(builder);
    }

    /// <summary>Creates an <see cref="HttpClient"/> from the server.</summary>
    public static HttpClient CreateClient(
        Action<JwtAuthenticationOptions>? configureJwt = null,
        Action<ApiKeyAuthenticationOptions>? configureApiKey = null)
        => Create(configureJwt, configureApiKey).CreateClient();
}
