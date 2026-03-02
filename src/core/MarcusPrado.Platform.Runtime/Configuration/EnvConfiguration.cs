using System.Globalization;
using Microsoft.Extensions.Configuration;

namespace MarcusPrado.Platform.Runtime.Configuration;

/// <summary>
/// <see cref="IAppConfiguration"/> implementation that layers environment variables
/// on top of the standard <see cref="IConfiguration"/> pipeline (12-Factor pattern).
/// Environment variable names are derived from the config key by replacing
/// <c>:</c> with <c>__</c> and uppercasing (standard .NET convention).
/// </summary>
public sealed class EnvConfiguration : IAppConfiguration
{
    private readonly IConfiguration _config;

    /// <summary>Initialises with the ambient <see cref="IConfiguration"/>.</summary>
    public EnvConfiguration(IConfiguration config) => _config = config;

    /// <inheritdoc />
    public T? Get<T>(ConfigurationKey<T> key)
    {
        // Env var lookup (overrides file-based config)
        var envName = key.Name
            .Replace(":", "__", StringComparison.Ordinal)
            .Replace("-", "_", StringComparison.Ordinal)
            .ToUpperInvariant();

        var envValue = System.Environment.GetEnvironmentVariable(envName);
        if (!string.IsNullOrWhiteSpace(envValue))
        {
            return (T)Convert.ChangeType(envValue, typeof(T), CultureInfo.InvariantCulture);
        }

        return _config.GetValue<T>(key.Name);
    }

    /// <inheritdoc />
    public TSection GetSection<TSection>(string sectionKey)
        where TSection : class
    {
        var section = _config.GetRequiredSection(sectionKey);
        return section.Get<TSection>()
            ?? throw new InvalidOperationException(
                $"Configuration section '{sectionKey}' could not be bound to type '{typeof(TSection).Name}'.");
    }
}
