using MarcusPrado.Platform.Abstractions.Errors;

namespace MarcusPrado.Platform.Application.Errors;

/// <summary>
/// Base exception for all application-layer errors.
/// Carry a structured <see cref="Abstractions.Errors.Error"/> alongside the message
/// so the exception can be mapped deterministically to an HTTP status code or
/// <c>Result.Failure</c>.
/// </summary>
public class AppException : Exception
{
    /// <summary>Gets the structured error descriptor.</summary>
    public Error Error { get; }

    /// <summary>Initializes an <see cref="AppException"/> from a structured error.</summary>
    public AppException(Error error)
        : base(error.Message)
    {
        Error = error;
    }

    /// <summary>Initializes an <see cref="AppException"/> with an inner exception.</summary>
    public AppException(Error error, Exception innerException)
        : base(error.Message, innerException)
    {
        Error = error;
    }
}
