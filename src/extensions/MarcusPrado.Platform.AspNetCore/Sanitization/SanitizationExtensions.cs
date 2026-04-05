using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace MarcusPrado.Platform.AspNetCore.Sanitization;

/// <summary>Extension methods for registering input sanitization services.</summary>
public static class SanitizationExtensions
{
    /// <summary>Registers <see cref="IInputSanitizer"/> (<see cref="HtmlSanitizerAdapter"/>) and <see cref="SanitizingModelBinderProvider"/>.</summary>
    public static IServiceCollection AddPlatformSanitization(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddSingleton<IInputSanitizer, HtmlSanitizerAdapter>();
        services.Configure<MvcOptions>(opts =>
            opts.ModelBinderProviders.Insert(0, new SanitizingModelBinderProvider()));
        return services;
    }
}
