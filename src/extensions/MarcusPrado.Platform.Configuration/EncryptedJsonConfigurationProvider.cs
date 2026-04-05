using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;

namespace MarcusPrado.Platform.Configuration;

/// <summary>
/// A <see cref="JsonConfigurationProvider"/> that decrypts configuration values
/// matching the <c>ENC(...)</c> pattern using a provided decryptor function.
/// </summary>
public sealed partial class EncryptedJsonConfigurationProvider : JsonConfigurationProvider
{
    private static readonly Regex EncPattern = EncRegex();

    private readonly Func<string, string> _decryptor;

    /// <summary>
    /// Initializes a new instance of <see cref="EncryptedJsonConfigurationProvider"/>.
    /// </summary>
    /// <param name="source">The JSON configuration source.</param>
    /// <param name="decryptor">A function that decrypts an <c>ENC(...)</c> value to plain text.</param>
    public EncryptedJsonConfigurationProvider(JsonConfigurationSource source, Func<string, string> decryptor)
        : base(source)
    {
        _decryptor = decryptor ?? throw new ArgumentNullException(nameof(decryptor));
    }

    /// <inheritdoc />
    public override void Load()
    {
        base.Load();
        DecryptValues();
    }

    private void DecryptValues()
    {
        var keys = Data.Keys.ToList();
        foreach (var key in keys)
        {
            var value = Data[key];
            if (value is not null && EncPattern.IsMatch(value))
            {
                Data[key] = _decryptor(value);
            }
        }
    }

    [GeneratedRegex(@"^ENC\(.+\)$", RegexOptions.Compiled)]
    private static partial Regex EncRegex();
}
