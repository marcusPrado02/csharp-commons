using global::Consul;
using MarcusPrado.Platform.Abstractions.ServiceDiscovery;
using MarcusPrado.Platform.Consul.Options;

namespace MarcusPrado.Platform.Consul.ServiceDiscovery;

/// <summary>Implements <see cref="IServiceDiscovery"/> via Consul.</summary>
public sealed class ConsulServiceDiscovery : IServiceDiscovery
{
    private readonly IConsulClient _client;
    private readonly ConsulOptions _options;

    /// <summary>Initializes a new instance of <see cref="ConsulServiceDiscovery"/>.</summary>
    public ConsulServiceDiscovery(IConsulClient client, ConsulOptions options)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(options);
        _client = client;
        _options = options;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ServiceEndpoint>> ResolveAsync(string serviceName, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(serviceName);

        var result = await _client
            .Health.Service(serviceName, string.Empty, passingOnly: false, ct)
            .ConfigureAwait(false);

        return result
            .Response.Select(e => new ServiceEndpoint(
                e.Service.ID,
                e.Service.Service,
                e.Service.Address,
                e.Service.Port,
                e.Service.Tags ?? [],
                MapHealth(e.Checks)
            ))
            .ToList();
    }

    /// <inheritdoc />
    public async Task RegisterAsync(ServiceRegistration registration, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(registration);

        var interval = registration.Interval ?? _options.DefaultCheckInterval;
        var agentReg = new AgentServiceRegistration
        {
            ID = registration.ServiceId,
            Name = registration.ServiceName,
            Address = registration.Address,
            Port = registration.Port,
            Tags = registration.Tags?.ToArray(),
            Check = registration.HealthUrl is not null
                ? new AgentServiceCheck
                {
                    HTTP = registration.HealthUrl,
                    Interval = interval,
                    DeregisterCriticalServiceAfter = TimeSpan.FromMinutes(1),
                }
                : null,
        };

        await _client.Agent.ServiceRegister(agentReg, ct).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task DeregisterAsync(string serviceId, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(serviceId);
        await _client.Agent.ServiceDeregister(serviceId, ct).ConfigureAwait(false);
    }

    private static ServiceHealth MapHealth(HealthCheck[] checks)
    {
        if (checks is null || checks.Length == 0)
            return ServiceHealth.Unknown;

        if (checks.Any(c => c.Status == HealthStatus.Critical))
            return ServiceHealth.Critical;

        if (checks.Any(c => c.Status == HealthStatus.Warning))
            return ServiceHealth.Warning;

        return checks.All(c => c.Status == HealthStatus.Passing) ? ServiceHealth.Passing : ServiceHealth.Unknown;
    }
}
