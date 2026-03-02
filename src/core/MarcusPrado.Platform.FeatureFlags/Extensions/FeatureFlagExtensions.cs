using MarcusPrado.Platform.FeatureFlags.Evaluation;
using MarcusPrado.Platform.FeatureFlags.Providers;
using Microsoft.Extensions.DependencyInjection;

namespace MarcusPrado.Platform.FeatureFlags.Extensions;

/// <summary>DI extension methods for the FeatureFlags module.</summary>
public static class FeatureFlagExtensions
{
    /// <summary>Registers an in-memory feature flag provider and the <see cref="FeatureFlagService"/>.</summary>
    public static IServiceCollection AddInMemoryFeatureFlags(this IServiceCollection services)
    {
        var provider = new InMemoryFeatureFlagProvider();
        services.AddSingleton(provider);
        services.AddSingleton<IFeatureFlagProvider>(provider);
        services.AddSingleton<FeatureFlagService>();
        return services;
    }

    /// <summary>Registers an environment-variable-based feature flag provider.</summary>
    public static IServiceCollection AddEnvironmentFeatureFlags(this IServiceCollection services)
    {
        services.AddSingleton<IFeatureFlagProvider, EnvironmentFeatureFlagProvider>();
        services.AddSingleton<FeatureFlagService>();
        return services;
    }

    /// <summary>
    /// Registers a composite provider that queries all registered <see cref="IFeatureFlagProvider"/>
    /// instances in registration order.
    /// </summary>
    public static IServiceCollection AddCompositeFeatureFlags(this IServiceCollection services)
    {
        services.AddSingleton<IFeatureFlagProvider>(sp =>
        {
            var providers = sp.GetServices<IFeatureFlagProvider>();
            return new CompositeFeatureFlagProvider(providers);
        });
        services.AddSingleton<FeatureFlagService>();
        return services;
    }
}
