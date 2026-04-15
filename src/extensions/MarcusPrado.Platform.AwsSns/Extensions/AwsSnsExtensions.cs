using Amazon.SimpleNotificationService;
using MarcusPrado.Platform.Abstractions.Sms;
using MarcusPrado.Platform.AwsSns.Options;
using MarcusPrado.Platform.AwsSns.Sms;
using Microsoft.Extensions.DependencyInjection;

namespace MarcusPrado.Platform.AwsSns.Extensions;

/// <summary>Extension methods to register the AWS SNS SMS adapter.</summary>
public static class AwsSnsExtensions
{
    /// <summary>
    /// Registers <see cref="ISmsService"/> backed by AWS SNS.
    /// </summary>
    public static IServiceCollection AddPlatformAwsSns(
        this IServiceCollection services,
        Action<AwsSnsOptions>? configure = null
    )
    {
        ArgumentNullException.ThrowIfNull(services);

        var opts = new AwsSnsOptions();
        configure?.Invoke(opts);

        services.AddSingleton(opts);
        services.AddSingleton<IAmazonSimpleNotificationService>(_ => new AmazonSimpleNotificationServiceClient(
            Amazon.RegionEndpoint.GetBySystemName(opts.Region)
        ));
        services.AddSingleton<ISmsService, SnsSmsService>();

        return services;
    }
}
