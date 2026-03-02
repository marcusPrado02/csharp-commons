using MarcusPrado.Platform.Contracts.Http;

namespace MarcusPrado.Platform.AspNetCore.Filters;

/// <summary>
/// Minimal-API <see cref="IEndpointFilter"/> that wraps non-envelope responses
/// inside an <see cref="ApiEnvelope{T}"/>, normalising the response shape
/// across all endpoints in a group.
/// </summary>
public sealed class ApiEnvelopeFilter : IEndpointFilter
{
    /// <inheritdoc/>
    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next)
    {
        var result = await next(context);

        return result switch
        {
            // Already wrapped — pass through unchanged
            ApiEnvelope      => result,
            // No body — pass through
            null             => result,
            // Wrap anything else
            _                => new ApiEnvelope<object> { Success = true, Data = result },
        };
    }
}
