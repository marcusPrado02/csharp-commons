using System.Diagnostics;

namespace MarcusPrado.Platform.AspNetCore.Middleware;

/// <summary>
/// ASP.NET Core middleware that emits a structured log entry for every HTTP
/// request/response, including method, path, status code, and elapsed time.
///
/// The entry uses Microsoft.Extensions.Logging structured logging and is
/// therefore compatible with any backend (Serilog, OpenTelemetry, etc.).
/// Correlation and tenant identifiers — if set earlier in the pipeline — are
/// automatically captured by the logging enrichers of the chosen backend.
/// </summary>
public sealed class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    /// <summary>Initialises the middleware.</summary>
    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    /// <summary>Logs the request/response and advances the pipeline.</summary>
    public async Task InvokeAsync(HttpContext context)
    {
        var start = Stopwatch.GetTimestamp();

        try
        {
            await _next(context);
        }
        finally
        {
            var elapsed = Stopwatch.GetElapsedTime(start);
            var statusCode = context.Response.StatusCode;

            if (statusCode >= 500)
            {
                if (_logger.IsEnabled(LogLevel.Error))
                {
                    _logger.LogError(
                        "HTTP {Method} {Path} responded {StatusCode} in {ElapsedMs:0.000} ms",
                        context.Request.Method,
                        context.Request.Path,
                        statusCode,
                        elapsed.TotalMilliseconds
                    );
                }
            }
            else
            {
                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation(
                        "HTTP {Method} {Path} responded {StatusCode} in {ElapsedMs:0.000} ms",
                        context.Request.Method,
                        context.Request.Path,
                        statusCode,
                        elapsed.TotalMilliseconds
                    );
                }
            }
        }
    }
}
