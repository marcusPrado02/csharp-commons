// <copyright file="SqsHealthProbe.cs" company="MarcusPrado">
// Copyright (c) MarcusPrado. All rights reserved.
// </copyright>

namespace MarcusPrado.Platform.AwsSqs.Health;

/// <summary>Health check that verifies connectivity to AWS SQS by listing queues.</summary>
public sealed class SqsHealthProbe : IHealthCheck
{
    private readonly IAmazonSQS _client;

    /// <summary>Initialises a new instance of <see cref="SqsHealthProbe"/>.</summary>
    /// <param name="client">The <see cref="IAmazonSQS"/> client to probe.</param>
    public SqsHealthProbe(IAmazonSQS client)
    {
        ArgumentNullException.ThrowIfNull(client);
        _client = client;
    }

    /// <inheritdoc/>
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            await _client
                .ListQueuesAsync(new ListQueuesRequest { MaxResults = 1 }, cancellationToken)
                .ConfigureAwait(false);

            return HealthCheckResult.Healthy("AWS SQS is reachable.");
        }
#pragma warning disable CA1031 // Health checks must not surface unexpected exceptions to the host
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("AWS SQS is unreachable.", ex);
        }
#pragma warning restore CA1031
    }
}
