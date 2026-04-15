// <copyright file="ServiceBusExtensions.cs" company="MarcusPrado">
// Copyright (c) MarcusPrado. All rights reserved.
// </copyright>

using MarcusPrado.Platform.AzureServiceBus.Consumer;
using MarcusPrado.Platform.AzureServiceBus.Health;
using MarcusPrado.Platform.AzureServiceBus.Publisher;

namespace MarcusPrado.Platform.AzureServiceBus.Extensions;

/// <summary>Extension methods to register Azure Service Bus platform services.</summary>
public static class ServiceBusExtensions
{
    /// <summary>
    /// Registers <see cref="ServiceBusClient"/>, <see cref="IServiceBusPublisher"/>,
    /// <see cref="IServiceBusConsumer"/> and a health check into the service collection.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to configure.</param>
    /// <param name="configure">A delegate that configures <see cref="ServiceBusOptions"/>.</param>
    /// <returns>The original <paramref name="services"/> for chaining.</returns>
    public static IServiceCollection AddPlatformAzureServiceBus(
        this IServiceCollection services,
        Action<ServiceBusOptions> configure
    )
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configure);

        services.Configure(configure);

        services.AddSingleton(sp =>
        {
            var opts = sp.GetRequiredService<IOptions<ServiceBusOptions>>().Value;

            if (!string.IsNullOrWhiteSpace(opts.ConnectionString))
            {
                return new ServiceBusClient(opts.ConnectionString);
            }

            if (!string.IsNullOrWhiteSpace(opts.FullyQualifiedNamespace))
            {
                return new ServiceBusClient(opts.FullyQualifiedNamespace, new DefaultAzureCredential());
            }

            throw new InvalidOperationException(
                "Either ServiceBusOptions.ConnectionString or ServiceBusOptions.FullyQualifiedNamespace must be set."
            );
        });

        services.AddSingleton<IServiceBusPublisher, ServiceBusPublisher>();
        services.AddSingleton<IServiceBusConsumer, ServiceBusConsumer>();

        services.AddHealthChecks().AddCheck<ServiceBusHealthProbe>("azure-service-bus");

        return services;
    }
}
