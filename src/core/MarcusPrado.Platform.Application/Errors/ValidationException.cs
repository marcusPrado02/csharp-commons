using MarcusPrado.Platform.Abstractions.Errors;

namespace MarcusPrado.Platform.Application.Errors;

/// <summary>Thrown when one or more validation rules are violated (maps to HTTP 422).</summary>
public sealed class ValidationException : AppException
{
    /// <summary>Gets all validation errors.</summary>
    public IReadOnlyList<Error> Errors { get; }

    /// <summary>Initializes a <see cref="ValidationException"/> with a list of errors.</summary>
    public ValidationException(IReadOnlyList<Error> errors)
        : base(errors.Count > 0
            ? errors[0]
            : Error.Validation("VALIDATION.FAILED", "One or more validation errors occurred."))
    {
        Errors = errors;
    }
}
