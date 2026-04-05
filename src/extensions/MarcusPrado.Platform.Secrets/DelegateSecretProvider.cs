namespace MarcusPrado.Platform.Secrets;

/// <summary>
/// Generic adapter that uses a delegate to retrieve secrets.
/// Models Azure Key Vault, AWS Secrets Manager, and HashiCorp Vault adapters —
/// in production code, the delegate would call the cloud SDK.
/// </summary>
public sealed class DelegateSecretProvider : ISecretProvider
{
    private readonly Func<string, CancellationToken, Task<string?>> _retriever;
    private readonly Action<string>? _onRotation;

    public DelegateSecretProvider(
        Func<string, CancellationToken, Task<string?>> retriever,
        Action<string>? onRotation = null)
    {
        _retriever = retriever;
        _onRotation = onRotation;
    }

    public Task<string?> GetSecretAsync(string name, CancellationToken cancellationToken = default)
        => _retriever(name, cancellationToken);

    public Task InvalidateCacheAsync(string name, CancellationToken cancellationToken = default)
    {
        _onRotation?.Invoke(name);
        return Task.CompletedTask;
    }
}
