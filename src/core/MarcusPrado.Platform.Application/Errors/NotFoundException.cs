using MarcusPrado.Platform.Abstractions.Errors;

namespace MarcusPrado.Platform.Application.Errors;

/// <summary>Thrown when a requested resource does not exist (maps to HTTP 404).</summary>
public sealed class NotFoundException : AppException
{
    /// <summary>Initializes a <see cref="NotFoundException"/> for the given resource.</summary>
    /// <param name="code">Stable error code, e.g. <c>"ORDER.NOT_FOUND"</c>.</param>
    /// <param name="message">Human-readable description.</param>
    public NotFoundException(string code, string message)
        : base(Error.NotFound(code, message))
    {
    }
}
