using Microsoft.Extensions.DependencyInjection;

namespace MarcusPrado.Platform.AuditLog;

/// <summary>DI registration helpers for audit logging.</summary>
public static class AuditLogExtensions
{
    /// <summary>Registers <see cref="InMemoryAuditSink"/> as the <see cref="IAuditLogger"/>.</summary>
    public static IServiceCollection AddInMemoryAuditLog(this IServiceCollection services)
    {
        services.AddSingleton<InMemoryAuditSink>();
        services.AddSingleton<IAuditLogger>(sp => sp.GetRequiredService<InMemoryAuditSink>());
        return services;
    }

    /// <summary>Registers <see cref="LoggingAuditSink"/> as the <see cref="IAuditLogger"/>.</summary>
    public static IServiceCollection AddLoggingAuditLog(this IServiceCollection services)
    {
        services.AddSingleton<IAuditLogger, LoggingAuditSink>();
        return services;
    }
}
