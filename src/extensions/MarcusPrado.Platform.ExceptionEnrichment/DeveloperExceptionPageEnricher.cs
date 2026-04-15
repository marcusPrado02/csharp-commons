using System.Diagnostics;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;

namespace MarcusPrado.Platform.ExceptionEnrichment;

/// <summary>
/// ASP.NET Core middleware that, in the Development environment, intercepts unhandled
/// exceptions and writes a structured JSON response containing exception details and
/// request context. In non-Development environments the exception is re-thrown unchanged.
/// </summary>
public sealed class DeveloperExceptionPageEnricher : IMiddleware
{
    private static readonly JsonSerializerOptions _serializerOptions =
        new() { WriteIndented = true };

    private readonly IWebHostEnvironment _environment;

    /// <summary>
    /// Initialises the middleware with the hosting environment.
    /// </summary>
    /// <param name="environment">Used to check whether the application is running in Development.</param>
    public DeveloperExceptionPageEnricher(IWebHostEnvironment environment)
    {
        ArgumentNullException.ThrowIfNull(environment);
        _environment = environment;
    }

    /// <summary>
    /// Invokes the middleware. If the downstream pipeline throws and the environment
    /// is Development, a 500 JSON response is written containing the exception context.
    /// </summary>
    /// <param name="context">The current HTTP context.</param>
    /// <param name="next">The next middleware in the pipeline.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(next);

        try
        {
            await next(context);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            if (!_environment.IsDevelopment())
            {
                throw;
            }

            await WriteEnrichedErrorAsync(context, ex);
        }
    }

    private static async Task WriteEnrichedErrorAsync(HttpContext context, Exception exception)
    {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/json";

        var payload = new
        {
            status = StatusCodes.Status500InternalServerError,
            title = "An unhandled exception occurred (Development only).",
            exceptionType = exception.GetType().FullName,
            message = exception.Message,
            fingerprint = ExceptionFingerprinter.GetFingerprint(exception),
            stackTrace = exception.StackTrace,
            traceId = Activity.Current?.Id ?? context.TraceIdentifier,
            request = new
            {
                method = context.Request.Method,
                path = context.Request.Path.Value,
                queryString = context.Request.QueryString.Value,
                host = context.Request.Host.Value,
            },
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(payload, _serializerOptions));
    }
}
