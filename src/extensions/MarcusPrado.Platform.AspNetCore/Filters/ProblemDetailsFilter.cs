using MarcusPrado.Platform.AspNetCore.Mapping;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace MarcusPrado.Platform.AspNetCore.Filters;

/// <summary>
/// MVC action filter that converts unhandled exceptions thrown by controllers
/// into RFC 9457-compliant <see cref="ProblemDetails"/> responses.
///
/// Use this filter as a complement to <c>ExceptionMiddleware</c> when the
/// application uses MVC / Minimal-API result processing rather than raw
/// middleware responses.
/// </summary>
public sealed class ProblemDetailsFilter : IExceptionFilter
{
    private readonly ILogger<ProblemDetailsFilter> _logger;

    /// <summary>Initialises the filter.</summary>
    public ProblemDetailsFilter(ILogger<ProblemDetailsFilter> logger)
        => _logger = logger;

    /// <inheritdoc />
    public void OnException(ExceptionContext context)
    {
        var ex         = context.Exception;
        var statusCode = ExceptionMapper.GetStatusCode(ex);

        _logger.LogError(ex, "Unhandled controller exception");

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title  = ExceptionMapper.GetTitle(statusCode),
            Type   = ExceptionMapper.GetProblemType(statusCode),
            Detail = ex.Message,
        };

        context.Result             = new ObjectResult(problem) { StatusCode = statusCode };
        context.ExceptionHandled   = true;
    }
}
