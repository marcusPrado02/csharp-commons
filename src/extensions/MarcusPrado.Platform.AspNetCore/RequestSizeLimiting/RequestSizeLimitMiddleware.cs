using System.Text.Json;

namespace MarcusPrado.Platform.AspNetCore.RequestSizeLimiting;

/// <summary>
/// ASP.NET Core middleware that enforces per-tenant request body size limits by tier.
/// <para>
/// Strategy:
/// <list type="bullet">
///   <item>If the <c>Content-Length</c> header is present and exceeds the tier limit, the request is
///   rejected immediately with 413 — the body is never read.</item>
///   <item>For chunked / unknown-length requests, the middleware relies on Kestrel's own
///   <c>IHttpMaxRequestBodySizeFeature</c>. Callers may also inject a counting stream to enforce
///   mid-read limits; that extension point is left to the application layer.</item>
/// </list>
/// </para>
/// Returns a <c>application/problem+json</c> RFC 9457 ProblemDetails response on violation.
/// </summary>
public sealed class RequestSizeLimitMiddleware
{
    private readonly RequestDelegate _next;
    private readonly RequestSizeLimitOptions _options;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    /// <summary>Initialises the middleware.</summary>
    public RequestSizeLimitMiddleware(RequestDelegate next, RequestSizeLimitOptions options)
    {
        ArgumentNullException.ThrowIfNull(next);
        ArgumentNullException.ThrowIfNull(options);

        _next = next;
        _options = options;
    }

    /// <summary>Processes the request, checking body size against the resolved tier limit.</summary>
    public async Task InvokeAsync(HttpContext context)
    {
        var tier = _options.TierResolver(context);
        var limit = _options.GetLimit(tier);

        // Fast-path: Content-Length header present — reject before touching the body.
        if (context.Request.ContentLength.HasValue && context.Request.ContentLength.Value > limit)
        {
            await WritePayloadTooLargeAsync(context, tier, limit);
            return;
        }

        await _next(context);
    }

    private static async Task WritePayloadTooLargeAsync(HttpContext context, RequestSizeTier tier, long limit)
    {
        context.Response.StatusCode = StatusCodes.Status413RequestEntityTooLarge;
        context.Response.ContentType = "application/problem+json";

        var problem = new
        {
            status = 413,
            title = "Payload Too Large",
            detail = $"Request body exceeds the {limit} byte limit for tier {tier}.",
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(problem, _jsonOptions));
    }
}
