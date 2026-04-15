using MarcusPrado.Platform.Abstractions.Context;
using MarcusPrado.Platform.Abstractions.Primitives;

namespace MarcusPrado.Platform.AspNetCore.Middleware;

/// <summary>
/// ASP.NET Core middleware that extracts or generates correlation identifiers and
/// propagates them through the request pipeline via <see cref="ICorrelationContext"/>.
///
/// Header precedence (inbound):
///   1. <c>X-Correlation-ID</c> — ties the request to an existing trace.
///   2. <c>X-Request-ID</c>    — uniquely identifies this specific request.
///
/// Both identifiers are echoed back on the response so callers can correlate logs.
/// </summary>
public sealed class CorrelationMiddleware
{
    private const string CorrelationIdHeader = "X-Correlation-ID";
    private const string RequestIdHeader = "X-Request-ID";

    private readonly RequestDelegate _next;
    private readonly IGuidFactory _guidFactory;

    /// <summary>Initialises the middleware with its dependencies.</summary>
    public CorrelationMiddleware(RequestDelegate next, IGuidFactory guidFactory)
    {
        _next = next;
        _guidFactory = guidFactory;
    }

    /// <summary>Processes the request, enriching context with correlation identifiers.</summary>
    public async Task InvokeAsync(HttpContext context, ICorrelationContext correlationContext)
    {
        var correlationId =
            context.Request.Headers[CorrelationIdHeader].FirstOrDefault() ?? _guidFactory.NewGuid().ToString("N");

        var requestId =
            context.Request.Headers[RequestIdHeader].FirstOrDefault() ?? _guidFactory.NewGuid().ToString("N");

        correlationContext.SetCorrelationId(correlationId);
        correlationContext.SetRequestId(requestId);

        context.Response.OnStarting(() =>
        {
            context.Response.Headers[CorrelationIdHeader] = correlationId;
            context.Response.Headers[RequestIdHeader] = requestId;
            return Task.CompletedTask;
        });

        await _next(context);
    }
}
