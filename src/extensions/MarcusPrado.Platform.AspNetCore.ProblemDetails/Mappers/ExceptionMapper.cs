using MarcusPrado.Platform.Application.Errors;
using Microsoft.AspNetCore.Http;

namespace MarcusPrado.Platform.AspNetCore.ProblemDetails.Mappers;

/// <summary>
/// Maps application exceptions to HTTP status codes following RFC 9457.
/// </summary>
public static class ExceptionMapper
{
    /// <summary>Returns the HTTP status code for the given exception.</summary>
    public static int GetStatusCode(Exception exception) =>
        exception switch
        {
            NotFoundException => StatusCodes.Status404NotFound,
            ConflictException => StatusCodes.Status409Conflict,
            UnauthorizedException => StatusCodes.Status401Unauthorized,
            ForbiddenException => StatusCodes.Status403Forbidden,
            ValidationException => StatusCodes.Status422UnprocessableEntity,
            AppException => StatusCodes.Status422UnprocessableEntity,
            _ => StatusCodes.Status500InternalServerError,
        };

    /// <summary>Returns the RFC 9457 type URI for the given status code.</summary>
    public static string GetProblemTypeUri(int statusCode) =>
        statusCode switch
        {
            400 => "https://tools.ietf.org/html/rfc9110#section-15.5.1",
            401 => "https://tools.ietf.org/html/rfc9110#section-15.5.2",
            403 => "https://tools.ietf.org/html/rfc9110#section-15.5.4",
            404 => "https://tools.ietf.org/html/rfc9110#section-15.5.5",
            409 => "https://tools.ietf.org/html/rfc9110#section-15.5.10",
            422 => "https://tools.ietf.org/html/rfc4918#section-11.2",
            _ => "https://tools.ietf.org/html/rfc9110#section-15.6.1",
        };
}
