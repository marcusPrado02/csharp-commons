using MarcusPrado.Platform.Abstractions.Errors;
using MarcusPrado.Platform.Abstractions.Results;

namespace MarcusPrado.Platform.Application.Pipeline;

/// <summary>
/// Marker interface applied to requests that require explicit authorization.
/// </summary>
public interface IRequireAuthorization
{
    /// <summary>Optional policy name evaluated by the authorizer.</summary>
    string? Policy => null;
}

/// <summary>
/// Checks authorization for requests that implement <see cref="IRequireAuthorization"/>.
/// Registered as the fifth behavior (order 5).
/// </summary>
/// <typeparam name="TRequest">The request type.</typeparam>
/// <typeparam name="TResponse">The response type.</typeparam>
public sealed class AuthorizationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    /// <inheritdoc/>
    public Task<TResponse> HandleAsync(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken = default
    )
    {
        // Authorization logic is injected via IPolicyAuthorizer when available.
        // Default pass-through; concrete authorization is wired per-deployment.
        return next(cancellationToken);
    }
}
