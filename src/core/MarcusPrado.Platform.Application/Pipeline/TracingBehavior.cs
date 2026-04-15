using System.Diagnostics;

namespace MarcusPrado.Platform.Application.Pipeline;

/// <summary>
/// Creates an <see cref="Activity"/> span for each request, enabling distributed
/// tracing via OpenTelemetry-compatible exporters.
/// Registered as the third behavior (order 3).
/// </summary>
/// <typeparam name="TRequest">The request type.</typeparam>
/// <typeparam name="TResponse">The response type.</typeparam>
public sealed class TracingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private static readonly ActivitySource _source = new("MarcusPrado.Platform.Application");

    /// <inheritdoc/>
    public async Task<TResponse> HandleAsync(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken = default)
    {
        var operationName = typeof(TRequest).Name;

        using var activity = _source.StartActivity(operationName, ActivityKind.Internal);

        activity?.SetTag("request.type", operationName);

        try
        {
            var response = await next(cancellationToken);

            activity?.SetStatus(ActivityStatusCode.Ok);

            return response;
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.AddTag("exception.type", ex.GetType().FullName);
            activity?.AddTag("exception.message", ex.Message);

            throw;
        }
    }
}
