using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;

namespace MarcusPrado.Platform.Configuration;

/// <summary>
/// Extension methods for adding encrypted configuration providers to an <see cref="IConfigurationBuilder"/>.
/// </summary>
public static class EncryptedConfigurationExtensions
{
    /// <summary>
    /// Adds an <see cref="EncryptedJsonConfigurationProvider"/> that reads a JSON file
    /// and decrypts any <c>ENC(...)</c> values using the specified decryptor.
    /// </summary>
    /// <param name="builder">The <see cref="IConfigurationBuilder"/> to add to.</param>
    /// <param name="path">The path to the JSON configuration file.</param>
    /// <param name="decryptor">A function that decrypts an <c>ENC(...)</c> value to plain text.</param>
    /// <param name="optional">Whether the file is optional.</param>
    /// <param name="reloadOnChange">Whether to reload the configuration when the file changes.</param>
    /// <returns>The <see cref="IConfigurationBuilder"/> for chaining.</returns>
    public static IConfigurationBuilder AddEncryptedJsonFile(
        this IConfigurationBuilder builder,
        string path,
        Func<string, string> decryptor,
        bool optional = false,
        bool reloadOnChange = false
    )
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(path);
        ArgumentNullException.ThrowIfNull(decryptor);

        var source = new JsonConfigurationSource
        {
            Path = path,
            Optional = optional,
            ReloadOnChange = reloadOnChange,
        };
        source.ResolveFileProvider();

        builder.Add(new EncryptedJsonConfigurationSource(source, decryptor));
        return builder;
    }

    /// <summary>
    /// Adds an <see cref="EncryptedEnvironmentVariableProvider"/> that reads environment variables
    /// and decrypts any <c>ENC(...)</c> values using the specified decryptor.
    /// </summary>
    /// <param name="builder">The <see cref="IConfigurationBuilder"/> to add to.</param>
    /// <param name="decryptor">A function that decrypts an <c>ENC(...)</c> value to plain text.</param>
    /// <returns>The <see cref="IConfigurationBuilder"/> for chaining.</returns>
    public static IConfigurationBuilder AddEncryptedEnvironmentVariables(
        this IConfigurationBuilder builder,
        Func<string, string> decryptor
    )
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(decryptor);

        builder.Add(new EncryptedEnvironmentVariableSource(decryptor));
        return builder;
    }
}
