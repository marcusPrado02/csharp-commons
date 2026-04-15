using System.Net;
using System.Net.Mime;
using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace MarcusPrado.Platform.Degradation;

/// <summary>
/// ASP.NET Core middleware that enforces the current <see cref="DegradationMode"/>:
/// <list type="bullet">
/// <item><see cref="DegradationMode.Maintenance"/> — returns HTTP 503 with a JSON body.</item>
/// <item><see cref="DegradationMode.ReadOnly"/> — returns HTTP 405 for write methods (POST, PUT, DELETE, PATCH).</item>
/// <item><see cref="DegradationMode.PartiallyDegraded"/> — adds an <c>X-Degradation-Mode</c> response header and passes through.</item>
/// <item><see cref="DegradationMode.None"/> — passes through without modification.</item>
/// </list>
/// </summary>
public sealed class DegradationMiddleware
{
    private static readonly HashSet<string> _writeMethods =
        new(StringComparer.OrdinalIgnoreCase) { "POST", "PUT", "DELETE", "PATCH" };

    private readonly RequestDelegate _next;
    private readonly IDegradationController _controller;

    /// <summary>
    /// Initializes a new instance of <see cref="DegradationMiddleware"/>.
    /// </summary>
    /// <param name="next">The next middleware delegate in the pipeline.</param>
    /// <param name="controller">The degradation controller used to read the current mode.</param>
    public DegradationMiddleware(RequestDelegate next, IDegradationController controller)
    {
        _next       = next ?? throw new ArgumentNullException(nameof(next));
        _controller = controller ?? throw new ArgumentNullException(nameof(controller));
    }

    /// <summary>
    /// Processes an HTTP request, enforcing the current degradation mode.
    /// </summary>
    /// <param name="context">The <see cref="HttpContext"/> for the current request.</param>
    public async Task InvokeAsync(HttpContext context)
    {
        var mode = await _controller.GetModeAsync(context.RequestAborted);

        switch (mode)
        {
            case DegradationMode.Maintenance:
                await WriteProblemAsync(
                    context,
                    (int)HttpStatusCode.ServiceUnavailable,
                    "Service Unavailable",
                    "The service is currently in maintenance mode. Please try again later.");
                return;

            case DegradationMode.ReadOnly when _writeMethods.Contains(context.Request.Method):
                await WriteProblemAsync(
                    context,
                    (int)HttpStatusCode.MethodNotAllowed,
                    "Method Not Allowed",
                    "The service is operating in read-only mode. Write operations are not permitted.");
                return;

            case DegradationMode.PartiallyDegraded:
                context.Response.Headers["X-Degradation-Mode"] = "PartiallyDegraded";
                break;
        }

        await _next(context);
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private static async Task WriteProblemAsync(
        HttpContext context,
        int statusCode,
        string title,
        string detail)
    {
        context.Response.StatusCode  = statusCode;
        context.Response.ContentType = MediaTypeNames.Application.Json;

        var body = new
        {
            status = statusCode,
            title,
            detail,
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(body));
    }
}
