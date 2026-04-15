using MailKit.Net.Smtp;
using MarcusPrado.Platform.Abstractions.Email;
using MarcusPrado.Platform.MailKit.Email;
using MarcusPrado.Platform.MailKit.Options;
using Microsoft.Extensions.DependencyInjection;

namespace MarcusPrado.Platform.MailKit.Extensions;

/// <summary>Extension methods to register MailKit email services.</summary>
public static class MailKitExtensions
{
    /// <summary>
    /// Registers <see cref="IEmailSender"/> and <see cref="IEmailTemplateRenderer"/>
    /// backed by MailKit SMTP.
    /// </summary>
    public static IServiceCollection AddPlatformMailKit(
        this IServiceCollection services,
        Action<MailKitOptions>? configure = null
    )
    {
        ArgumentNullException.ThrowIfNull(services);

        var opts = new MailKitOptions();
        configure?.Invoke(opts);

        services.AddSingleton(opts);
        services.AddTransient<ISmtpClient, SmtpClient>();
        services.AddTransient<IEmailSender, MailKitEmailSender>();
        services.AddSingleton<IEmailTemplateRenderer, SimpleTemplateRenderer>();

        return services;
    }
}
