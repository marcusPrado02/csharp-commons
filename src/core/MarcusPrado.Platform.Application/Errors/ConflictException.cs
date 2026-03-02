using MarcusPrado.Platform.Abstractions.Errors;

namespace MarcusPrado.Platform.Application.Errors;

/// <summary>Thrown when an operation would create a duplicate or conflicting resource (HTTP 409).</summary>
public sealed class ConflictException : AppException
{
    /// <summary>Initializes a <see cref="ConflictException"/>.</summary>
    public ConflictException(string code, string message)
        : base(Error.Conflict(code, message))
    {
    }
}
