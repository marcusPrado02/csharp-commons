namespace MarcusPrado.Platform.DataAccess.Tracing;

public static class TracingExtensions
{
    /// <summary>Adds EfCoreTracingInterceptor to the DbContext options.</summary>
    public static IServiceCollection AddEfCoreTracing(this IServiceCollection services)
    {
        services.AddSingleton<EfCoreTracingInterceptor>();
        return services;
    }
}
