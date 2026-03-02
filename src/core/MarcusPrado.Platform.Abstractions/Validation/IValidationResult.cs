using MarcusPrado.Platform.Abstractions.Errors;

namespace MarcusPrado.Platform.Abstractions.Validation;

/// <summary>The result of running one or more validators against a request.</summary>
public interface IValidationResult
{
    /// <summary><c>true</c> when there are no validation errors.</summary>
    bool IsValid { get; }

    /// <summary>The list of validation errors; empty on success.</summary>
    IReadOnlyList<Error> Errors { get; }
}
