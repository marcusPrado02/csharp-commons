using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;

namespace MarcusPrado.Platform.Configuration;

/// <summary>
/// An <see cref="IConfigurationSource"/> that creates <see cref="EncryptedJsonConfigurationProvider"/> instances.
/// </summary>
internal sealed class EncryptedJsonConfigurationSource : IConfigurationSource
{
    private readonly JsonConfigurationSource _inner;
    private readonly Func<string, string> _decryptor;

    /// <summary>
    /// Initializes a new instance of <see cref="EncryptedJsonConfigurationSource"/>.
    /// </summary>
    /// <param name="inner">The underlying JSON configuration source.</param>
    /// <param name="decryptor">The decryptor function for <c>ENC(...)</c> values.</param>
    public EncryptedJsonConfigurationSource(JsonConfigurationSource inner, Func<string, string> decryptor)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        _decryptor = decryptor ?? throw new ArgumentNullException(nameof(decryptor));
    }

    /// <inheritdoc />
    public IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        return new EncryptedJsonConfigurationProvider(_inner, _decryptor);
    }
}
