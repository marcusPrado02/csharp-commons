using MarcusPrado.Platform.Abstractions.GraphQL;
using MarcusPrado.Platform.HotChocolate.Context;
using MarcusPrado.Platform.HotChocolate.Errors;
using Microsoft.Extensions.DependencyInjection;
using HC = global::HotChocolate;

namespace MarcusPrado.Platform.HotChocolate.Extensions;

/// <summary>Extension methods to register HotChocolate GraphQL platform services.</summary>
public static class HotChocolateExtensions
{
    /// <summary>
    /// Registers the platform GraphQL infrastructure:
    /// <see cref="IPlatformResolverContext"/>, <see cref="IPlatformErrorFilter"/>
    /// bridge, and the HotChocolate <see cref="HC.IErrorFilter"/>.
    /// </summary>
    public static IServiceCollection AddPlatformGraphQL(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddHttpContextAccessor();
        services.AddSingleton<IPlatformResolverContext, HttpContextResolverContext>();

        return services;
    }

    /// <summary>
    /// Registers a user-supplied <see cref="IPlatformErrorFilter"/> implementation
    /// and wires it into HotChocolate's error pipeline via <see cref="PlatformErrorFilterBridge"/>.
    /// </summary>
    public static IServiceCollection AddPlatformErrorFilter<TFilter>(this IServiceCollection services)
        where TFilter : class, IPlatformErrorFilter
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<IPlatformErrorFilter, TFilter>();
        services.AddSingleton<HC.IErrorFilter, PlatformErrorFilterBridge>();

        return services;
    }
}
