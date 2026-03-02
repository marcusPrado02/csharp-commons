using MarcusPrado.Platform.Abstractions.Validation;

namespace MarcusPrado.Platform.AspNetCore.Filters;

/// <summary>
/// Minimal-API <see cref="IEndpointFilter"/> that validates the first argument
/// of type <typeparamref name="TRequest"/> via all registered
/// <see cref="IValidator{TRequest}"/> instances before the handler runs.
/// Returns HTTP 422 with an error list when validation fails.
/// </summary>
/// <typeparam name="TRequest">The request DTO to validate.</typeparam>
public sealed class ValidationFilter<TRequest> : IEndpointFilter
    where TRequest : class
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    /// <summary>
    /// Initialises with all registered validators for <typeparamref name="TRequest"/>.
    /// </summary>
    public ValidationFilter(IEnumerable<IValidator<TRequest>> validators)
    {
        ArgumentNullException.ThrowIfNull(validators);
        _validators = validators;
    }

    /// <inheritdoc/>
    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next)
    {
        var request = context.Arguments.OfType<TRequest>().FirstOrDefault();
        if (request is null)
        {
            return await next(context);
        }

        var errors = new List<string>();
        foreach (var validator in _validators)
        {
            var result = await validator.ValidateAsync(request, context.HttpContext.RequestAborted);
            if (!result.IsValid)
            {
                errors.AddRange(result.Errors.Select(e => e.Message));
            }
        }

        if (errors.Count > 0)
        {
            return Results.UnprocessableEntity(new { errors });
        }

        return await next(context);
    }
}
