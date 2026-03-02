namespace MarcusPrado.Platform.Runtime.Environment;

/// <summary>The deployment tier the service is running in.</summary>
public enum DeploymentEnvironment
{
    /// <summary>Local developer machine.</summary>
    Development,

    /// <summary>Shared staging / pre-production.</summary>
    Staging,

    /// <summary>Customer-facing production workload.</summary>
    Production,
}
