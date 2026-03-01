namespace MarcusPrado.Platform.Application.Errors;

/// <summary>Raised when the caller is not authenticated. Maps to HTTP 401.</summary>
public class UnauthorizedException : AppException
{
    /// <summary>Initialises with a message describing the authentication failure.</summary>
    public UnauthorizedException(string message)
        : base(message) { }

    /// <inheritdoc cref="AppException(string, Exception)" />
    public UnauthorizedException(string message, Exception innerException)
        : base(message, innerException) { }
}
