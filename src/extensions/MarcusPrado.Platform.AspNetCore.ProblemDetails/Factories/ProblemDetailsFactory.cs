using System.Diagnostics;
using MarcusPrado.Platform.Abstractions.Context;
using MarcusPrado.Platform.Application.Errors;
using MarcusPrado.Platform.AspNetCore.ProblemDetails.Mappers;
using Microsoft.Extensions.DependencyInjection;

namespace MarcusPrado.Platform.AspNetCore.ProblemDetails.Factories;

/// <summary>
/// Creates RFC 9457-compliant <see cref="MvcProblemDetails"/> objects enriched
/// with platform-specific extensions (traceId, tenantId, code, errors).
/// </summary>
public static class PlatformProblemDetailsFactory
{
    /// <summary>
    /// Creates a <see cref="MvcProblemDetails"/> from an exception, enriching it
    /// with context from the current <see cref="HttpContext"/>.
    /// </summary>
    public static MvcProblemDetails Create(Exception exception, HttpContext context)
    {
        var status = ExceptionMapper.GetStatusCode(exception);
        var pd = new MvcProblemDetails
        {
            Status = status,
            Title = GetTitle(status),
            Detail = exception.Message,
            Type = ExceptionMapper.GetProblemTypeUri(status),
            Instance = context.Request.Path,
        };

        Enrich(pd, exception, context);
        return pd;
    }

    /// <summary>
    /// Creates a <see cref="MvcProblemDetails"/> from an HTTP status code.
    /// </summary>
    public static MvcProblemDetails Create(int statusCode, string? detail = null)
    {
        return new MvcProblemDetails
        {
            Status = statusCode,
            Title = GetTitle(statusCode),
            Detail = detail,
            Type = ExceptionMapper.GetProblemTypeUri(statusCode),
        };
    }

    // ── Private helpers ────────────────────────────────────────────────────

    private static void Enrich(MvcProblemDetails pd, Exception exception, HttpContext context)
    {
        var traceId = Activity.Current?.TraceId.ToString() ?? context.TraceIdentifier;
        pd.Extensions["traceId"] = traceId;

        var tenantCtx = context.RequestServices.GetService<ITenantContext>();
        if (tenantCtx?.TenantId is not null)
        {
            pd.Extensions["tenantId"] = tenantCtx.TenantId;
        }

        if (exception is ValidationException validationEx && validationEx.Errors.Count > 0)
        {
            pd.Extensions["errors"] = validationEx.Errors;
        }

        if (exception is AppException)
        {
            pd.Extensions["code"] = exception
                .GetType()
                .Name.Replace("Exception", string.Empty, StringComparison.OrdinalIgnoreCase)
                .ToUpperInvariant();
        }
    }

    private static string GetTitle(int statusCode) =>
        statusCode switch
        {
            400 => "Bad Request",
            401 => "Unauthorized",
            403 => "Forbidden",
            404 => "Not Found",
            409 => "Conflict",
            422 => "Unprocessable Content",
            500 => "Internal Server Error",
            _ => "An error occurred",
        };
}
