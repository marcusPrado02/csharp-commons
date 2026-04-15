using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;

namespace MarcusPrado.Platform.Configuration;

/// <summary>
/// A <see cref="IConfigurationProvider"/> that reads environment variables and decrypts
/// any values matching the <c>ENC(...)</c> pattern using a provided decryptor function.
/// </summary>
public sealed partial class EncryptedEnvironmentVariableProvider : ConfigurationProvider
{
    private static readonly Regex _encPattern = EncRegex();

    private readonly Func<string, string> _decryptor;

    /// <summary>
    /// Initializes a new instance of <see cref="EncryptedEnvironmentVariableProvider"/>.
    /// </summary>
    /// <param name="decryptor">A function that decrypts an <c>ENC(...)</c> value to plain text.</param>
    public EncryptedEnvironmentVariableProvider(Func<string, string> decryptor)
    {
        _decryptor = decryptor ?? throw new ArgumentNullException(nameof(decryptor));
    }

    /// <inheritdoc />
    public override void Load()
    {
        var envVars = System.Environment.GetEnvironmentVariables();
        foreach (System.Collections.DictionaryEntry entry in envVars)
        {
            var key = entry.Key?.ToString();
            var value = entry.Value?.ToString();

            if (key is null) continue;

            // Normalize key separator (environment variables use __ for hierarchy)
            var normalizedKey = key.Replace("__", ConfigurationPath.KeyDelimiter, StringComparison.Ordinal);

            if (value is not null && _encPattern.IsMatch(value))
            {
                Data[normalizedKey] = _decryptor(value);
            }
            else
            {
                Data[normalizedKey] = value;
            }
        }
    }

    [GeneratedRegex(@"^ENC\(.+\)$", RegexOptions.Compiled)]
    private static partial Regex EncRegex();
}
