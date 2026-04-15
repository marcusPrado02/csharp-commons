namespace MarcusPrado.Platform.Abstractions.ServiceDiscovery;

/// <summary>Locates services by name, returning healthy endpoints.</summary>
public interface IServiceDiscovery
{
    /// <summary>Resolves all healthy endpoints for the given service name.</summary>
    Task<IReadOnlyList<ServiceEndpoint>> ResolveAsync(string serviceName, CancellationToken ct = default);

    /// <summary>Registers a new service instance.</summary>
    Task RegisterAsync(ServiceRegistration registration, CancellationToken ct = default);

    /// <summary>Deregisters a service instance by its ID.</summary>
    Task DeregisterAsync(string serviceId, CancellationToken ct = default);
}

/// <summary>Address and metadata of a single service instance.</summary>
public sealed record ServiceEndpoint(
    string ServiceId,
    string ServiceName,
    string Address,
    int Port,
    IReadOnlyList<string> Tags,
    ServiceHealth Health
);

/// <summary>Registration payload when a service announces itself.</summary>
public sealed record ServiceRegistration(
    string ServiceId,
    string ServiceName,
    string Address,
    int Port,
    IReadOnlyList<string>? Tags = null,
    string? HealthUrl = null,
    TimeSpan? Interval = null
);

/// <summary>Health status of a service endpoint.</summary>
public enum ServiceHealth
{
    /// <summary>Service is healthy and accepting traffic.</summary>
    Passing,

    /// <summary>Service is degraded but still reachable.</summary>
    Warning,

    /// <summary>Service is not reachable.</summary>
    Critical,

    /// <summary>Health status is unknown.</summary>
    Unknown,
}
