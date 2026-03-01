using MarcusPrado.Platform.Abstractions.Context;
using MarcusPrado.Platform.Abstractions.Primitives;
using MarcusPrado.Platform.Application.Pipeline;
using MarcusPrado.Platform.AspNetCore.Internal;

namespace MarcusPrado.Platform.AspNetCore.Extensions;

/// <summary>
/// <see cref="IServiceCollection"/> extension methods that register the platform's
/// core services and CQRS pipeline infrastructure.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers fundamental platform services:
    /// <list type="bullet">
    ///   <item><see cref="IClock"/> — <see cref="SystemClock"/> (singleton)</item>
    ///   <item><see cref="IGuidFactory"/> — <see cref="DefaultGuidFactory"/> (singleton)</item>
    ///   <item><see cref="IJsonSerializer"/> — <see cref="SystemTextJsonSerializer"/> (singleton)</item>
    ///   <item><see cref="ICorrelationContext"/> — <see cref="DefaultCorrelationContext"/> (scoped)</item>
    ///   <item><see cref="ITenantContext"/> — <see cref="DefaultTenantContext"/> (scoped)</item>
    /// </list>
    /// </summary>
    /// <param name="services">The DI service collection to configure.</param>
    /// <returns>The same <paramref name="services"/> for chaining.</returns>
    public static IServiceCollection AddPlatformCore(this IServiceCollection services)
    {
        services.AddSingleton<IClock,          SystemClock>();
        services.AddSingleton<IGuidFactory,    DefaultGuidFactory>();
        services.AddSingleton<IJsonSerializer, SystemTextJsonSerializer>();
        services.AddScoped<ICorrelationContext, DefaultCorrelationContext>();
        services.AddScoped<ITenantContext,      DefaultTenantContext>();
        return services;
    }

    /// <summary>
    /// Registers all CQRS pipeline behaviours in execution order:
    /// <c>Validation → Authorization → Logging → Tracing → Metrics → Retry → Idempotency → Transaction</c>.
    /// Each behaviour is registered as <see cref="IPipelineBehavior"/> so that a
    /// custom command bus can resolve them as an ordered list.
    /// </summary>
    /// <param name="services">The DI service collection to configure.</param>
    /// <returns>The same <paramref name="services"/> for chaining.</returns>
    public static IServiceCollection AddPlatformCqrs(this IServiceCollection services)
    {
        services.AddTransient<IPipelineBehavior, ValidationBehavior>();
        services.AddTransient<IPipelineBehavior, AuthorizationBehavior>();
        services.AddTransient<IPipelineBehavior, LoggingBehavior>();
        services.AddTransient<IPipelineBehavior, TracingBehavior>();
        services.AddTransient<IPipelineBehavior, MetricsBehavior>();
        services.AddTransient<IPipelineBehavior, RetryBehavior>();
        services.AddTransient<IPipelineBehavior, IdempotencyBehavior>();
        services.AddTransient<IPipelineBehavior, TransactionBehavior>();
        return services;
    }
}
