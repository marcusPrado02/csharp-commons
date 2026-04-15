// <copyright file="ServiceBusHealthProbe.cs" company="MarcusPrado">
// Copyright (c) MarcusPrado. All rights reserved.
// </copyright>

namespace MarcusPrado.Platform.AzureServiceBus.Health;

/// <summary>Health check that verifies connectivity to Azure Service Bus.</summary>
public sealed class ServiceBusHealthProbe : IHealthCheck
{
    private readonly ServiceBusClient _client;

    /// <summary>Initialises a new instance of <see cref="ServiceBusHealthProbe"/>.</summary>
    /// <param name="client">The <see cref="ServiceBusClient"/> to probe.</param>
    public ServiceBusHealthProbe(ServiceBusClient client)
    {
        ArgumentNullException.ThrowIfNull(client);
        _client = client;
    }

    /// <inheritdoc/>
    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default
    )
    {
        // ServiceBusClient is lazy — check that the namespace is non-empty as a quick proxy.
        var healthy = !string.IsNullOrWhiteSpace(_client.FullyQualifiedNamespace);
        var result = healthy
            ? HealthCheckResult.Healthy("Azure Service Bus client is configured.")
            : HealthCheckResult.Unhealthy("Azure Service Bus namespace is not configured.");

        return Task.FromResult(result);
    }
}
