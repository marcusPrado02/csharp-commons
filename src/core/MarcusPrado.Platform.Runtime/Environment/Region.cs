namespace MarcusPrado.Platform.Runtime.Environment;

/// <summary>
/// Identifies the cloud region where the service instance is running.
/// Reads the <c>REGION</c> environment variable; falls back to <c>local</c>.
/// </summary>
public sealed record Region(string Value)
{
    /// <summary>Used for local development / unit tests.</summary>
    public static readonly Region Local = new("local");

    /// <summary>Creates a <see cref="Region"/> from the <c>REGION</c> env var.</summary>
    public static Region FromEnvironment() => new(System.Environment.GetEnvironmentVariable("REGION") ?? "local");

    /// <inheritdoc />
    public override string ToString() => Value;
}
