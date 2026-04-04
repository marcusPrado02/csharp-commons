using MarcusPrado.Platform.AspNetCore.Filters;

namespace MarcusPrado.Platform.AspNetCore.Endpoints;

/// <summary>
/// Convenience extension methods for applying common endpoint filters
/// to <see cref="RouteHandlerBuilder"/> instances.
/// </summary>
public static class EndpointExtensions
{
    /// <summary>
    /// Adds <see cref="ApiEnvelopeFilter"/> to the endpoint, wrapping plain
    /// object responses inside an <see cref="MarcusPrado.Platform.Contracts.Http.ApiEnvelope{T}"/>.
    /// </summary>
    public static RouteHandlerBuilder WithApiEnvelope(this RouteHandlerBuilder builder)
        => builder.AddEndpointFilter<ApiEnvelopeFilter>();

    /// <summary>
    /// Adds <see cref="ValidationFilter{TRequest}"/> to the endpoint, validating
    /// the first argument of type <typeparamref name="TRequest"/> before the handler runs.
    /// </summary>
    public static RouteHandlerBuilder WithValidation<TRequest>(this RouteHandlerBuilder builder)
        where TRequest : class
        => builder.AddEndpointFilter<ValidationFilter<TRequest>>();
}
