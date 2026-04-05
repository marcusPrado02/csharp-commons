using Microsoft.Extensions.Configuration;

namespace MarcusPrado.Platform.Configuration;

/// <summary>
/// An <see cref="IConfigurationSource"/> that creates <see cref="EncryptedEnvironmentVariableProvider"/> instances.
/// </summary>
internal sealed class EncryptedEnvironmentVariableSource : IConfigurationSource
{
    private readonly Func<string, string> _decryptor;

    /// <summary>
    /// Initializes a new instance of <see cref="EncryptedEnvironmentVariableSource"/>.
    /// </summary>
    /// <param name="decryptor">The decryptor function for <c>ENC(...)</c> values.</param>
    public EncryptedEnvironmentVariableSource(Func<string, string> decryptor)
    {
        _decryptor = decryptor ?? throw new ArgumentNullException(nameof(decryptor));
    }

    /// <inheritdoc />
    public IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        return new EncryptedEnvironmentVariableProvider(_decryptor);
    }
}
