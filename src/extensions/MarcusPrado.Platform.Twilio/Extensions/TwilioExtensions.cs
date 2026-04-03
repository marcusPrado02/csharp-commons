using MarcusPrado.Platform.Abstractions.Sms;
using MarcusPrado.Platform.Twilio.Options;
using MarcusPrado.Platform.Twilio.Sms;
using Microsoft.Extensions.DependencyInjection;

namespace MarcusPrado.Platform.Twilio.Extensions;

/// <summary>Extension methods to register Twilio SMS services.</summary>
public static class TwilioExtensions
{
    /// <summary>Registers <see cref="ISmsService"/> backed by Twilio.</summary>
    public static IServiceCollection AddPlatformTwilio(
        this IServiceCollection services,
        Action<TwilioOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        var opts = new TwilioOptions();
        configure?.Invoke(opts);

        services.AddSingleton(opts);
        services.AddSingleton<ISmsService, TwilioSmsService>();

        return services;
    }
}
