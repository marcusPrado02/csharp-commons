namespace MarcusPrado.Platform.Application.Errors;

/// <summary>
/// Base class for all application-layer exceptions. Prefer returning
/// <see cref="MarcusPrado.Platform.Abstractions.Results.Result{T}"/> for expected
/// failures; reserve exceptions for truly exceptional conditions.
/// </summary>
public class AppException : Exception
{
    /// <inheritdoc cref="Exception(string)" />
    public AppException(string message)
        : base(message) { }

    /// <inheritdoc cref="Exception(string, Exception)" />
    public AppException(string message, Exception innerException)
        : base(message, innerException) { }
}
