namespace MarcusPrado.Platform.Secrets;

/// <summary>Reads secrets from environment variables. Variable name = secret name (uppercased, hyphens to underscores).</summary>
public sealed class EnvironmentSecretProvider : ISecretProvider
{
    public Task<string?> GetSecretAsync(string name, CancellationToken cancellationToken = default)
    {
        var envName = name.ToUpperInvariant().Replace('-', '_').Replace('/', '_');
        return Task.FromResult(Environment.GetEnvironmentVariable(envName));
    }

    public Task InvalidateCacheAsync(string name, CancellationToken cancellationToken = default) => Task.CompletedTask;
}
