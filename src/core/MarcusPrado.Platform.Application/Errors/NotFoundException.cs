namespace MarcusPrado.Platform.Application.Errors;

/// <summary>Raised when a requested resource was not found. Maps to HTTP 404.</summary>
public class NotFoundException : AppException
{
    /// <summary>Initialises with a message describing what was not found.</summary>
    public NotFoundException(string message)
        : base(message) { }

    /// <inheritdoc cref="AppException(string, Exception)" />
    public NotFoundException(string message, Exception innerException)
        : base(message, innerException) { }
}
