namespace MarcusPrado.Platform.Security.Mtls;

public static class MtlsExtensions
{
    public const string SchemeName = "Mtls";

    public static IServiceCollection AddPlatformMtls(
        this IServiceCollection services,
        Action<MtlsOptions>? configure = null
    )
    {
        var opts = new MtlsOptions();
        configure?.Invoke(opts);
        services.AddSingleton(opts);
        services.AddSingleton<CertificateTenantResolver>();
        services.AddSingleton<CertificateRevocationChecker>();

        services
            .AddAuthentication(SchemeName)
            .AddScheme<MtlsAuthenticationOptions, MtlsAuthenticationHandler>(SchemeName, _ => { });

        return services;
    }
}
