using MarcusPrado.Platform.Abstractions.Errors;

namespace MarcusPrado.Platform.Abstractions.Validation;

/// <summary>
/// Validates an instance of <typeparamref name="TRequest"/> and returns a
/// structured <see cref="IValidationResult"/>.
/// </summary>
/// <typeparam name="TRequest">The type being validated.</typeparam>
public interface IValidator<in TRequest>
{
    /// <summary>Validates the request asynchronously.</summary>
    Task<IValidationResult> ValidateAsync(TRequest request, CancellationToken cancellationToken = default);
}
