using Microsoft.Extensions.DependencyInjection;

namespace MarcusPrado.Platform.Postgres.Repository;

/// <summary>DI helpers for registering the Dapper repository.</summary>
public static class DapperExtensions
{
    /// <summary>
    /// Adds a scoped <see cref="IDapperRepository"/> backed by the provided
    /// <paramref name="connectionFactory"/>.
    /// </summary>
    public static IServiceCollection AddDapperRepository(
        this IServiceCollection services,
        Func<IServiceProvider, System.Data.IDbConnection> connectionFactory)
    {
        services.AddScoped<IDapperRepository>(sp =>
            new DapperRepository(() => connectionFactory(sp)));
        return services;
    }
}
