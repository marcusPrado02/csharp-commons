using MarcusPrado.Platform.Abstractions.Execution;
using MarcusPrado.Platform.Application.Execution;
using MarcusPrado.Platform.Application.Pipeline;
using Microsoft.Extensions.DependencyInjection;

namespace MarcusPrado.Platform.Application.Extensions;

/// <summary>Extension methods for registering the platform CQRS pipeline.</summary>
public static class ApplicationExtensions
{
    /// <summary>
    /// Registers the CQRS pipeline with all built-in behaviors in the correct order:
    /// Logging → Metrics → Tracing → Validation → Authorization → Idempotency → Transaction → Retry.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The same <paramref name="services"/> for chaining.</returns>
    public static IServiceCollection AddPlatformCqrs(this IServiceCollection services)
    {
        services.AddScoped<IDispatcher, Dispatcher>();
        services.AddScoped<ICommandBus>(sp => (ICommandBus)sp.GetRequiredService<IDispatcher>());
        services.AddScoped<IQueryBus>(sp => (IQueryBus)sp.GetRequiredService<IDispatcher>());

        // Behaviors registered in order 1–8 (innermost = last registered)
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(MetricsBehavior<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(TracingBehavior<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(AuthorizationBehavior<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(IdempotencyBehavior<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(RetryBehavior<,>));

        return services;
    }
}
