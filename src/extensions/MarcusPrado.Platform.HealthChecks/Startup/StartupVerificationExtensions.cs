using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MarcusPrado.Platform.HealthChecks.Startup;

/// <summary>
/// Extension methods for registering startup verification services (T-51).
/// </summary>
public static class StartupVerificationExtensions
{
    /// <summary>
    /// Registers the <see cref="StartupVerificationHostedService"/> that runs all
    /// <see cref="IStartupVerification"/> registrations on startup.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to extend.</param>
    /// <returns>The same <see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddStartupVerification(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddHostedService<StartupVerificationHostedService>();
        return services;
    }

    /// <summary>
    /// Registers a <see cref="DatabaseConnectivityVerification"/> with the supplied probe.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to extend.</param>
    /// <param name="probe">A function that returns <c>true</c> when the database is reachable.</param>
    /// <param name="name">Optional name for the verification (defaults to <c>"DatabaseConnectivity"</c>).</param>
    /// <returns>The same <see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddDatabaseConnectivityVerification(
        this IServiceCollection services,
        Func<Task<bool>> probe,
        string name = "DatabaseConnectivity")
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(probe);

        services.AddSingleton<IStartupVerification>(
            new DatabaseConnectivityVerification(name, probe));
        return services;
    }

    /// <summary>
    /// Registers a <see cref="RequiredSecretsVerification"/> for the given configuration keys.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to extend.</param>
    /// <param name="keys">The configuration keys that must be present and non-empty.</param>
    /// <returns>The same <see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddRequiredSecretsVerification(
        this IServiceCollection services,
        params string[] keys)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<IStartupVerification>(sp =>
        {
            var config = sp.GetRequiredService<IConfiguration>();
            return new RequiredSecretsVerification(keys, config);
        });
        return services;
    }
}
