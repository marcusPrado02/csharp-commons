using MarcusPrado.Platform.Application.Errors;
using MarcusPrado.Platform.AspNetCore.ProblemDetails.Factories;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MarcusPrado.Platform.AspNetCore.ProblemDetails.Extensions;

/// <summary>
/// Extension methods for registering platform Problem Details middleware.
/// </summary>
public static class ProblemDetailsExtensions
{
    /// <summary>
    /// Registers the platform Problem Details exception handler middleware.
    /// Maps all <see cref="AppException"/> subclasses to RFC 9457 responses.
    /// Call this after <c>UseRouting()</c> and before the endpoint mapping.
    /// </summary>
    public static IApplicationBuilder UsePlatformProblemDetails(this IApplicationBuilder app)
    {
        app.UseExceptionHandler(errApp => errApp.Run(async context =>
        {
            var feature = context.Features.Get<IExceptionHandlerFeature>();
            if (feature is null)
                return;

            var exception = feature.Error;
            var logger = context.RequestServices
                .GetRequiredService<ILoggerFactory>()
                .CreateLogger("PlatformProblemDetails");

            var pd = PlatformProblemDetailsFactory.Create(exception, context);
            var status = pd.Status ?? StatusCodes.Status500InternalServerError;

            if (status >= 500)
                logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);
            else
                logger.LogWarning(exception, "Application exception: {Message}", exception.Message);

            context.Response.StatusCode = status;
            await context.Response.WriteAsJsonAsync(
                pd,
                options: null,
                contentType: "application/problem+json");
        }));

        return app;
    }

    /// <summary>
    /// Registers platform Problem Details services.  Call in
    /// <c>ConfigureServices</c> / <c>builder.Services</c>.
    /// </summary>
    public static IServiceCollection AddPlatformProblemDetails(
        this IServiceCollection services)
    {
        services.AddProblemDetails();
        return services;
    }
}
