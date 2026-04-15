using System.Diagnostics;
using MarcusPrado.Platform.Application.Errors;
using MarcusPrado.Platform.AspNetCore.Mapping;

namespace MarcusPrado.Platform.AspNetCore.Middleware;

/// <summary>
/// ASP.NET Core middleware that catches all unhandled exceptions and converts
/// them to RFC 9457-compliant <see cref="ProblemDetails"/> JSON responses.
///
/// Mapping is delegated to <see cref="ExceptionMapper"/>, which translates
/// well-known platform exceptions to the appropriate HTTP status codes. All
/// unknown exceptions produce HTTP 500 without leaking internal stack traces.
/// </summary>
public sealed class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    /// <summary>Initialises the middleware with its dependencies.</summary>
    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    /// <summary>Processes the request and handles any unhandled exceptions.</summary>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
#pragma warning disable CA1031 // Catching all exceptions is intentional in an exception-handling middleware
        catch (Exception ex)
#pragma warning restore CA1031
        {
            _logger.LogError(
                ex,
                "Unhandled exception for {Method} {Path}",
                context.Request.Method,
                context.Request.Path
            );

            await WriteProbleDetailsAsync(context, ex);
        }
    }

    private static async Task WriteProbleDetailsAsync(HttpContext context, Exception exception)
    {
        var statusCode = ExceptionMapper.GetStatusCode(exception);

        // Build a flat RFC 9457 ProblemDetails JSON object using a plain dictionary
        // so that all fields — including extensions — appear at the top level.
        var body = new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["status"] = statusCode,
            ["title"] = ExceptionMapper.GetTitle(statusCode),
            ["type"] = ExceptionMapper.GetProblemType(statusCode),
            ["detail"] = exception.Message,
            ["traceId"] = Activity.Current?.Id ?? context.TraceIdentifier,
        };

        // RFC 9457 extension: field-level validation errors (only for ValidationException)
        if (exception is ValidationException validationEx && validationEx.Errors.Count > 0)
            body["errors"] = validationEx.Errors;

        context.Response.StatusCode = statusCode;

        // Pass content-type directly to WriteAsJsonAsync so it is not overridden
        await context.Response.WriteAsJsonAsync(body, options: null, contentType: "application/problem+json");
    }
}
