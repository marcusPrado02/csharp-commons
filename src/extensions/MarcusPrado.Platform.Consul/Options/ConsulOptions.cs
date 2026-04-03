namespace MarcusPrado.Platform.Consul.Options;

/// <summary>Configuration for the Consul service discovery adapter.</summary>
public sealed class ConsulOptions
{
    /// <summary>Gets or sets the Consul agent address (e.g. <c>http://localhost:8500</c>).</summary>
    public string Address { get; set; } = "http://localhost:8500";

    /// <summary>Gets or sets an optional Consul ACL token.</summary>
    public string? Token { get; set; }

    /// <summary>Gets or sets the default health check interval.</summary>
    public TimeSpan DefaultCheckInterval { get; set; } = TimeSpan.FromSeconds(10);
}
