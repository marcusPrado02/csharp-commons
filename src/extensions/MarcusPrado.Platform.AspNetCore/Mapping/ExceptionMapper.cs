using MarcusPrado.Platform.Application.Errors;
using MarcusPrado.Platform.Domain.SeedWork;

namespace MarcusPrado.Platform.AspNetCore.Mapping;

/// <summary>
/// Maps .NET exceptions to RFC 9457 ProblemDetails HTTP status codes and type URIs.
/// </summary>
public static class ExceptionMapper
{
    /// <summary>
    /// Returns the appropriate HTTP status code for the given <paramref name="exception"/>.
    /// </summary>
    public static int GetStatusCode(Exception exception) =>
        exception switch
        {
            NotFoundException => StatusCodes.Status404NotFound,
            ConflictException => StatusCodes.Status409Conflict,
            UnauthorizedException => StatusCodes.Status401Unauthorized,
            ForbiddenException => StatusCodes.Status403Forbidden,
            ValidationException => StatusCodes.Status422UnprocessableEntity,
            DomainException => StatusCodes.Status422UnprocessableEntity,
            AppException => StatusCodes.Status400BadRequest,
            _ => StatusCodes.Status500InternalServerError,
        };

    /// <summary>
    /// Returns the RFC 9110 problem type URI for the given <paramref name="statusCode"/>.
    /// </summary>
    public static string GetProblemType(int statusCode) =>
        statusCode switch
        {
            StatusCodes.Status400BadRequest => "https://tools.ietf.org/html/rfc9110#section-15.5.1",
            StatusCodes.Status401Unauthorized => "https://tools.ietf.org/html/rfc9110#section-15.5.2",
            StatusCodes.Status403Forbidden => "https://tools.ietf.org/html/rfc9110#section-15.5.4",
            StatusCodes.Status404NotFound => "https://tools.ietf.org/html/rfc9110#section-15.5.5",
            StatusCodes.Status409Conflict => "https://tools.ietf.org/html/rfc9110#section-15.5.10",
            StatusCodes.Status422UnprocessableEntity => "https://tools.ietf.org/html/rfc9110#section-15.5.21",
            _ => "https://tools.ietf.org/html/rfc9110#section-15.6.1",
        };

    /// <summary>
    /// Returns a short, human-readable title for the given <paramref name="statusCode"/>.
    /// </summary>
    public static string GetTitle(int statusCode) =>
        statusCode switch
        {
            StatusCodes.Status400BadRequest => "Bad Request",
            StatusCodes.Status401Unauthorized => "Unauthorized",
            StatusCodes.Status403Forbidden => "Forbidden",
            StatusCodes.Status404NotFound => "Not Found",
            StatusCodes.Status409Conflict => "Conflict",
            StatusCodes.Status422UnprocessableEntity => "Unprocessable Entity",
            _ => "Internal Server Error",
        };
}
