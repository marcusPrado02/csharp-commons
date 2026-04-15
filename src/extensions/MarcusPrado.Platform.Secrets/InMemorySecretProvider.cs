namespace MarcusPrado.Platform.Secrets;

public sealed class InMemorySecretProvider : ISecretProvider
{
    private readonly Dictionary<string, string> _secrets;

    public InMemorySecretProvider(IReadOnlyDictionary<string, string>? initialSecrets = null) =>
        _secrets = initialSecrets is not null ? new Dictionary<string, string>(initialSecrets) : [];

    public void Set(string name, string value) => _secrets[name] = value;

    public Task<string?> GetSecretAsync(string name, CancellationToken cancellationToken = default) =>
        Task.FromResult(_secrets.TryGetValue(name, out var v) ? v : (string?)null);

    public Task InvalidateCacheAsync(string name, CancellationToken cancellationToken = default) => Task.CompletedTask;
}
