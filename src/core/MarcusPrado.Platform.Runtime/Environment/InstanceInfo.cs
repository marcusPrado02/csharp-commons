using System.Net;

namespace MarcusPrado.Platform.Runtime.Environment;

/// <summary>
/// Immutable snapshot of runtime identity populated from standard Kubernetes
/// environment variables and <c>SERVICE_*</c> conventions.
/// </summary>
public sealed record InstanceInfo
{
    /// <summary>Logical service name (env: <c>SERVICE_NAME</c>).</summary>
    public string ServiceName { get; init; } = "unknown";

    /// <summary>Semantic version string (env: <c>SERVICE_VERSION</c>).</summary>
    public string ServiceVersion { get; init; } = "0.0.0";

    /// <summary>Kubernetes pod name (env: <c>POD_NAME</c>).</summary>
    public string PodName { get; init; } = "localhost";

    /// <summary>Kubernetes node name (env: <c>NODE_NAME</c>).</summary>
    public string NodeName { get; init; } = "localhost";

    /// <summary>Cloud region identifier (env: <c>REGION</c>).</summary>
    public string Region { get; init; } = "local";

    /// <summary>
    /// Populates an <see cref="InstanceInfo"/> from the process environment.
    /// Falls back to sensible defaults when variables are absent.
    /// </summary>
    public static InstanceInfo FromEnvironment()
    {
        var hostname = Dns.GetHostName();
        return new InstanceInfo
        {
            ServiceName    = Env("SERVICE_NAME",    "unknown"),
            ServiceVersion = Env("SERVICE_VERSION", "0.0.0"),
            PodName        = Env("POD_NAME",        hostname),
            NodeName       = Env("NODE_NAME",       hostname),
            Region         = Env("REGION",          "local"),
        };
    }

    private static string Env(string name, string fallback)
        => System.Environment.GetEnvironmentVariable(name) ?? fallback;
}
