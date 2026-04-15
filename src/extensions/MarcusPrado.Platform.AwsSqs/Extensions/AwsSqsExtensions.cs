// <copyright file="AwsSqsExtensions.cs" company="MarcusPrado">
// Copyright (c) MarcusPrado. All rights reserved.
// </copyright>

using Amazon;
using Amazon.Runtime;
using MarcusPrado.Platform.AwsSqs.Health;
using MarcusPrado.Platform.AwsSqs.Sns;
using MarcusPrado.Platform.AwsSqs.Sqs;

namespace MarcusPrado.Platform.AwsSqs.Extensions;

/// <summary>Extension methods to register AWS SQS and SNS platform services.</summary>
public static class AwsSqsExtensions
{
    /// <summary>
    /// Registers <see cref="IAmazonSQS"/>, <see cref="IAmazonSimpleNotificationService"/>,
    /// <see cref="ISqsPublisher"/>, <see cref="ISqsConsumer"/>, <see cref="ISnsPublisher"/>,
    /// and a health check into the service collection.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to configure.</param>
    /// <param name="configureSqs">An optional delegate that configures <see cref="SqsOptions"/>.</param>
    /// <param name="configureSns">An optional delegate that configures <see cref="SnsOptions"/>.</param>
    /// <returns>The original <paramref name="services"/> for chaining.</returns>
    public static IServiceCollection AddPlatformAwsSqs(
        this IServiceCollection services,
        Action<SqsOptions>? configureSqs = null,
        Action<SnsOptions>? configureSns = null
    )
    {
        ArgumentNullException.ThrowIfNull(services);

        if (configureSqs is not null)
        {
            services.Configure(configureSqs);
        }
        else
        {
            services.Configure<SqsOptions>(_ => { });
        }

        if (configureSns is not null)
        {
            services.Configure(configureSns);
        }
        else
        {
            services.Configure<SnsOptions>(_ => { });
        }

        services.AddSingleton<IAmazonSQS>(sp =>
        {
            var opts = sp.GetRequiredService<IOptions<SqsOptions>>().Value;
            var config = new Amazon.SQS.AmazonSQSConfig();

            if (!string.IsNullOrWhiteSpace(opts.ServiceUrl))
            {
                config.ServiceURL = opts.ServiceUrl;
            }
            else
            {
                config.RegionEndpoint = RegionEndpoint.GetBySystemName(opts.Region);
            }

            return new Amazon.SQS.AmazonSQSClient(new AnonymousAWSCredentials(), config);
        });

        services.AddSingleton<IAmazonSimpleNotificationService>(sp =>
        {
            var opts = sp.GetRequiredService<IOptions<SnsOptions>>().Value;
            var config = new Amazon.SimpleNotificationService.AmazonSimpleNotificationServiceConfig();

            if (!string.IsNullOrWhiteSpace(opts.ServiceUrl))
            {
                config.ServiceURL = opts.ServiceUrl;
            }
            else
            {
                config.RegionEndpoint = RegionEndpoint.GetBySystemName(opts.Region);
            }

            return new Amazon.SimpleNotificationService.AmazonSimpleNotificationServiceClient(
                new AnonymousAWSCredentials(),
                config
            );
        });

        services.AddSingleton<ISqsPublisher, SqsPublisher>();
        services.AddSingleton<ISqsConsumer, SqsConsumer>();
        services.AddSingleton<ISnsPublisher, SnsPublisher>();

        services.AddHealthChecks().AddCheck<SqsHealthProbe>("aws-sqs");

        return services;
    }
}
