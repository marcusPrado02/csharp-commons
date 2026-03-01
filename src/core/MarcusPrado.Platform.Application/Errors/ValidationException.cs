namespace MarcusPrado.Platform.Application.Errors;

/// <summary>
/// Raised when one or more input validation rules are violated. Maps to HTTP 422.
/// </summary>
public class ValidationException : AppException
{
    /// <summary>Gets the individual validation errors keyed by field name.</summary>
    public IReadOnlyDictionary<string, string[]> Errors { get; }

    /// <summary>
    /// Initialises with a generic message and no field-level errors.
    /// </summary>
    public ValidationException(string message)
        : base(message)
    {
        Errors = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Initialises with field-level validation errors. The message is set to
    /// a summary derived from the number of errors.
    /// </summary>
    public ValidationException(IReadOnlyDictionary<string, string[]> errors)
        : base($"Validation failed with {errors.Count} error(s).")
    {
        Errors = errors;
    }

    /// <summary>
    /// Initialises with a custom message and field-level validation errors.
    /// </summary>
    public ValidationException(string message, IReadOnlyDictionary<string, string[]> errors)
        : base(message)
    {
        Errors = errors;
    }
}
