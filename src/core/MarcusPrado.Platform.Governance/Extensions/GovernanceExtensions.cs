using MarcusPrado.Platform.Governance.ADR;
using MarcusPrado.Platform.Governance.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace MarcusPrado.Platform.Governance.Extensions;

/// <summary>Extension methods for registering governance services.</summary>
public static class GovernanceExtensions
{
    /// <summary>
    /// Registers the platform governance services with in-memory implementations.
    /// Replace implementations with persistent versions for production use.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The same <paramref name="services"/> for chaining.</returns>
    public static IServiceCollection AddPlatformGovernance(this IServiceCollection services)
    {
        services.AddSingleton<IContractRegistry, InMemoryContractRegistry>();
        services.AddSingleton<IAdrStore, InMemoryAdrStore>();

        return services;
    }
}
