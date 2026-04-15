using System.Reflection;
using MarcusPrado.Platform.Abstractions.Errors;
using MarcusPrado.Platform.Abstractions.Results;
using MarcusPrado.Platform.Abstractions.Validation;
using Microsoft.Extensions.DependencyInjection;

namespace MarcusPrado.Platform.Application.Pipeline;

/// <summary>
/// Resolves all registered <see cref="IValidator{TRequest}"/> instances, runs them
/// in parallel, aggregates failures, and short-circuits the pipeline when invalid.
/// Registered as the fourth behavior (order 4).
/// </summary>
/// <typeparam name="TRequest">The request type.</typeparam>
/// <typeparam name="TResponse">The response type; <c>Result</c> or <c>Result&lt;T&gt;</c>.</typeparam>
public sealed class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IServiceProvider _serviceProvider;

    /// <summary>Initializes the behavior with the DI service provider.</summary>
    public ValidationBehavior(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <inheritdoc/>
    public async Task<TResponse> HandleAsync(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken = default
    )
    {
        var validators = _serviceProvider.GetServices<IValidator<TRequest>>().ToArray();

        if (validators.Length == 0)
        {
            return await next(cancellationToken);
        }

        var validationTasks = validators.Select(v => v.ValidateAsync(request, cancellationToken));
        var results = await Task.WhenAll(validationTasks);

        var errors = results.Where(r => !r.IsValid).SelectMany(r => r.Errors).ToArray();

        if (errors.Length > 0)
        {
            return CreateFailure(errors[0]);
        }

        return await next(cancellationToken);
    }

    private static TResponse CreateFailure(Error error)
    {
        var responseType = typeof(TResponse);

        if (responseType == typeof(Result))
        {
            return (TResponse)(object)Result.Failure(error);
        }

        if (responseType.IsGenericType && responseType.GetGenericTypeDefinition() == typeof(Result<>))
        {
            var valueType = responseType.GetGenericArguments()[0];
            var method = typeof(Result)
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .First(m =>
                    m.Name == nameof(Result.Failure)
                    && m.IsGenericMethod
                    && m.GetParameters() is [var p]
                    && p.ParameterType == typeof(Error)
                )
                .MakeGenericMethod(valueType);

            return (TResponse)method.Invoke(null, new object[] { error })!;
        }

        throw new InvalidOperationException(
            $"ValidationBehavior does not support response type '{responseType.FullName}'. "
                + "Expected Result or Result<T>."
        );
    }
}
