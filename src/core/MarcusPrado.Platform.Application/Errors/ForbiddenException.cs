using MarcusPrado.Platform.Abstractions.Errors;

namespace MarcusPrado.Platform.Application.Errors;

/// <summary>Thrown when the caller lacks permission to perform the action (HTTP 403).</summary>
public sealed class ForbiddenException : AppException
{
    /// <summary>Initializes a <see cref="ForbiddenException"/>.</summary>
    public ForbiddenException(string code, string message)
        : base(Error.Forbidden(code, message))
    {
    }
}
