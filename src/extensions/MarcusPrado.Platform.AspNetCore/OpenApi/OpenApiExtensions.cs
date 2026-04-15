using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Scalar.AspNetCore;

namespace MarcusPrado.Platform.AspNetCore.OpenApi;

/// <summary>
/// Extension methods for registering and configuring Platform OpenAPI + Scalar.
/// </summary>
public static class OpenApiExtensions
{
    /// <summary>
    /// Registers OpenAPI document generation with JWT and/or API-key security schemes
    /// and optional platform context-header parameters on every operation.
    /// </summary>
    public static IServiceCollection AddPlatformOpenApi(
        this IServiceCollection services,
        Action<PlatformOpenApiOptions>? configure = null
    )
    {
        var opts = new PlatformOpenApiOptions();
        configure?.Invoke(opts);
        services.AddSingleton(opts);

        services.AddOpenApi(options =>
        {
            options.AddDocumentTransformer(
                (document, context, ct) =>
                {
                    document.Info.Title = opts.Title;
                    document.Info.Version = opts.Version;
                    document.Info.Description = opts.Description;

                    document.Components ??= new OpenApiComponents();
                    document.Components.SecuritySchemes ??= new Dictionary<string, OpenApiSecurityScheme>();

                    if (opts.EnableJwtAuth)
                    {
                        document.Components.SecuritySchemes[opts.JwtSchemeName] = new OpenApiSecurityScheme
                        {
                            Type = SecuritySchemeType.Http,
                            Scheme = "bearer",
                            BearerFormat = "JWT",
                            Description = "Enter your JWT token.",
                        };
                    }

                    if (opts.EnableApiKeyAuth)
                    {
                        document.Components.SecuritySchemes[opts.ApiKeySchemeName] = new OpenApiSecurityScheme
                        {
                            Type = SecuritySchemeType.ApiKey,
                            In = ParameterLocation.Header,
                            Name = opts.ApiKeyHeaderName,
                            Description = "API key header.",
                        };
                    }

                    return Task.CompletedTask;
                }
            );

            if (opts.IncludeContextHeaders)
                options.AddOperationTransformer<PlatformOperationTransformer>();
        });

        return services;
    }

    /// <summary>
    /// Maps the OpenAPI JSON endpoint and the Scalar interactive reference UI.
    /// </summary>
    public static IEndpointRouteBuilder UsePlatformOpenApi(this IEndpointRouteBuilder app)
    {
        app.MapOpenApi();
        app.MapScalarApiReference();
        return app;
    }
}
