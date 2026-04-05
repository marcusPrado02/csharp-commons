namespace MarcusPrado.Platform.Secrets;

public interface ISecretProvider
{
    /// <summary>Retrieves a secret value by name.</summary>
    Task<string?> GetSecretAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>Forces refresh of a cached secret.</summary>
    Task InvalidateCacheAsync(string name, CancellationToken cancellationToken = default);
}
