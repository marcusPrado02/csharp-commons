namespace MarcusPrado.Platform.Application.Errors;

/// <summary>Raised when the caller is authenticated but lacks permission. Maps to HTTP 403.</summary>
public class ForbiddenException : AppException
{
    /// <summary>Initialises with a message describing the authorization failure.</summary>
    public ForbiddenException(string message)
        : base(message) { }

    /// <inheritdoc cref="AppException(string, Exception)" />
    public ForbiddenException(string message, Exception innerException)
        : base(message, innerException) { }
}
