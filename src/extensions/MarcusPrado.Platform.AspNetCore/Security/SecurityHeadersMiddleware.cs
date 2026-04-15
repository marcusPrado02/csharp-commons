namespace MarcusPrado.Platform.AspNetCore.Middleware;

/// <summary>
/// ASP.NET Core middleware that injects recommended security headers into every
/// HTTP response via <c>HttpResponse.OnStarting</c>, ensuring headers are
/// set regardless of where in the pipeline the response originates.
/// </summary>
public sealed class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;
    private readonly Security.SecurityHeadersOptions _options;

    /// <summary>Initialises the middleware.</summary>
    public SecurityHeadersMiddleware(RequestDelegate next, Security.SecurityHeadersOptions options)
    {
        _next = next;
        _options = options;
    }

    /// <summary>Processes the request and schedules security-header injection.</summary>
    public Task InvokeAsync(HttpContext context)
    {
        context.Response.OnStarting(() =>
        {
            var h = context.Response.Headers;

            if (_options.EnableXContentTypeOptions)
                h["X-Content-Type-Options"] = "nosniff";

            if (_options.EnableXFrameOptions)
                h["X-Frame-Options"] = "DENY";

            // X-XSS-Protection: 0 disables the legacy XSS auditor, which can
            // itself introduce security vulnerabilities in older browsers.
            h["X-XSS-Protection"] = "0";

            if (_options.EnableReferrerPolicy)
                h["Referrer-Policy"] = _options.ReferrerPolicy;

            if (_options.EnableContentSecurityPolicy
                && !h.ContainsKey("Content-Security-Policy"))
            {
                h["Content-Security-Policy"] = _options.ContentSecurityPolicy;
            }

            return Task.CompletedTask;
        });

        return _next(context);
    }
}
