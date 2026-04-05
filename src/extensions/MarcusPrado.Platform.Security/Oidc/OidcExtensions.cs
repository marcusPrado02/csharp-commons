namespace MarcusPrado.Platform.Security.Oidc;

public static class OidcExtensions
{
    public static IServiceCollection AddPlatformOidcClient(
        this IServiceCollection services,
        Action<OidcClientOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configure);

        services.Configure(configure);

        services.AddHttpClient<OidcClientService>()
            .ConfigureHttpClient((sp, client) =>
            {
                var opts = sp.GetRequiredService<IOptions<OidcClientOptions>>().Value;
                if (!string.IsNullOrEmpty(opts.Authority))
                    client.BaseAddress = new Uri(opts.Authority);
            });

        services.AddSingleton<IOidcClientService>(sp => sp.GetRequiredService<OidcClientService>());
        services.AddTransient<MachineToMachineHttpHandler>();

        return services;
    }
}
