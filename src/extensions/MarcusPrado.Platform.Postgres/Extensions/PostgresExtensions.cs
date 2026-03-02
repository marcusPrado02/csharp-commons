using MarcusPrado.Platform.EfCore.DbContext;
using MarcusPrado.Platform.EfCore.Migrations;
using MarcusPrado.Platform.Postgres.Connection;
using MarcusPrado.Platform.Postgres.Health;
using MarcusPrado.Platform.Postgres.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace MarcusPrado.Platform.Postgres.Extensions;

/// <summary>Extension methods for registering PostgreSQL platform services.</summary>
public static class PostgresExtensions
{
    /// <summary>
    /// Registers <see cref="PostgresConnectionFactory"/>, the given <typeparamref name="TContext"/>,
    /// and <see cref="PostgresHealthProbe"/>.
    /// </summary>
    public static IServiceCollection AddPlatformPostgres<TContext>(
        this IServiceCollection services,
        Action<PostgresOptions>? configure = null)
        where TContext : AppDbContextBase
    {
        var opts = new PostgresOptions();
        configure?.Invoke(opts);

        services.AddSingleton(opts);
        services.AddSingleton<PostgresConnectionFactory>();
        services.AddSingleton<PostgresHealthProbe>();
        services.AddSingleton<EfMigrationRunner>();

        services.AddDbContext<TContext>(db =>
        {
            db.UseNpgsql(opts.ConnectionString, npgsql =>
            {
                npgsql.CommandTimeout(opts.CommandTimeoutSeconds);
            });
        });

        services.AddHealthChecks()
            .AddCheck<PostgresHealthProbe>("postgres");

        return services;
    }
}
