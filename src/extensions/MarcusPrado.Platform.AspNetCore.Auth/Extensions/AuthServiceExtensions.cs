using MarcusPrado.Platform.Abstractions.Context;
using MarcusPrado.Platform.AspNetCore.Auth.Handlers;
using MarcusPrado.Platform.AspNetCore.Auth.Internal;
using MarcusPrado.Platform.AspNetCore.Auth.Options;
using MarcusPrado.Platform.AspNetCore.Auth.Requirements;

namespace MarcusPrado.Platform.AspNetCore.Auth.Extensions;

/// <summary>
/// Extension methods for registering platform authentication and authorization
/// services in an ASP.NET Core DI container.
/// </summary>
public static class AuthServiceExtensions
{
    /// <summary>
    /// Registers the platform JWT and API-key authentication schemes together
    /// with a scoped <see cref="IUserContext"/> implementation.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureJwt">
    /// Optional delegate to configure <see cref="JwtAuthenticationOptions"/>.
    /// </param>
    /// <param name="configureApiKey">
    /// Optional delegate to configure <see cref="ApiKeyAuthenticationOptions"/>.
    /// </param>
    public static IServiceCollection AddPlatformAuth(
        this IServiceCollection services,
        Action<JwtAuthenticationOptions>? configureJwt = null,
        Action<ApiKeyAuthenticationOptions>? configureApiKey = null)
    {
        services.AddScoped<IUserContext, DefaultUserContext>();

        services
            .AddAuthentication(PlatformAuthSchemes.Jwt)
            .AddScheme<JwtAuthenticationOptions, JwtAuthenticationHandler>(
                PlatformAuthSchemes.Jwt,
                opts => configureJwt?.Invoke(opts))
            .AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>(
                PlatformAuthSchemes.ApiKey,
                opts => configureApiKey?.Invoke(opts));

        return services;
    }

    /// <summary>
    /// Registers ASP.NET Core authorization with the platform's
    /// <see cref="PermissionAuthorizationHandler"/> and
    /// <see cref="ScopeAuthorizationHandler"/>.
    /// </summary>
    public static IServiceCollection AddPlatformAuthorization(
        this IServiceCollection services)
    {
        services.AddAuthorization();
        services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();
        services.AddSingleton<IAuthorizationHandler, ScopeAuthorizationHandler>();
        return services;
    }
}
