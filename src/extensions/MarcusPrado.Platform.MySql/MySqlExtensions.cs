namespace MarcusPrado.Platform.MySql;

/// <summary>Extension methods for registering MySQL platform services.</summary>
public static class MySqlExtensions
{
    /// <summary>
    /// Registers <see cref="IMySqlConnectionFactory"/> and configures <see cref="MySqlOptions"/>.
    /// </summary>
    public static IServiceCollection AddPlatformMySql(this IServiceCollection services, Action<MySqlOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configure);

        services.Configure(configure);
        services.AddSingleton<IMySqlConnectionFactory, MySqlConnectionFactory>();

        return services;
    }

    /// <summary>
    /// Registers <see cref="IMySqlConnectionFactory"/>, configures <see cref="MySqlOptions"/>,
    /// and adds a <see cref="DbContext"/> of type <typeparamref name="TContext"/> backed by MySQL.
    /// </summary>
    public static IServiceCollection AddPlatformMySql<TContext>(
        this IServiceCollection services,
        Action<MySqlOptions> configure
    )
        where TContext : DbContext
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configure);

        services.AddPlatformMySql(configure);

        services.AddDbContext<TContext>(
            (sp, opts) =>
            {
                var mysqlOpts = sp.GetRequiredService<IOptions<MySqlOptions>>().Value;
                var serverVersion = MySqlServerVersion.Parse(mysqlOpts.ServerVersion);
                opts.UseMySql(
                    mysqlOpts.ConnectionString,
                    serverVersion,
                    mysql =>
                    {
                        mysql.EnableRetryOnFailure(mysqlOpts.MaxRetryCount, mysqlOpts.MaxRetryDelay, null);
                    }
                );
            }
        );

        return services;
    }

    /// <summary>
    /// Adds a <see cref="MySqlHealthProbe"/> health check named <paramref name="name"/>.
    /// </summary>
    public static IHealthChecksBuilder AddMySqlHealthCheck(this IHealthChecksBuilder builder, string name = "mysql")
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Services.AddSingleton<MySqlHealthProbe>();
        return builder.Add(
            new HealthCheckRegistration(
                name,
                sp => sp.GetRequiredService<MySqlHealthProbe>(),
                failureStatus: null,
                tags: null
            )
        );
    }
}
