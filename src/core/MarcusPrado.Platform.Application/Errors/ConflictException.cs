namespace MarcusPrado.Platform.Application.Errors;

/// <summary>Raised when a request conflicts with the current state of a resource. Maps to HTTP 409.</summary>
public class ConflictException : AppException
{
    /// <summary>Initialises with a message describing the conflict.</summary>
    public ConflictException(string message)
        : base(message) { }

    /// <inheritdoc cref="AppException(string, Exception)" />
    public ConflictException(string message, Exception innerException)
        : base(message, innerException) { }
}
