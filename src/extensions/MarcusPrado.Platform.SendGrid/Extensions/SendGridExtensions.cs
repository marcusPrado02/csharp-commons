using MarcusPrado.Platform.Abstractions.Email;
using MarcusPrado.Platform.SendGrid.Email;
using MarcusPrado.Platform.SendGrid.Options;
using Microsoft.Extensions.DependencyInjection;
using SendGrid;

namespace MarcusPrado.Platform.SendGrid.Extensions;

/// <summary>Extension methods to register the SendGrid email adapter.</summary>
public static class SendGridExtensions
{
    /// <summary>
    /// Registers <see cref="IEmailSender"/> backed by SendGrid.
    /// </summary>
    public static IServiceCollection AddPlatformSendGrid(
        this IServiceCollection services,
        Action<SendGridOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        var opts = new SendGridOptions();
        configure?.Invoke(opts);

        services.AddSingleton(opts);
        services.AddSingleton<ISendGridClient>(_ => new SendGridClient(opts.ApiKey));
        services.AddSingleton<IEmailSender, SendGridEmailSender>();

        return services;
    }
}
