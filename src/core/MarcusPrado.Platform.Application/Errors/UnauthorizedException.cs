using MarcusPrado.Platform.Abstractions.Errors;

namespace MarcusPrado.Platform.Application.Errors;

/// <summary>Thrown when the caller is not authenticated (HTTP 401).</summary>
public sealed class UnauthorizedException : AppException
{
    /// <summary>Initializes an <see cref="UnauthorizedException"/>.</summary>
    public UnauthorizedException(string code, string message)
        : base(Error.Unauthorized(code, message))
    {
    }
}
