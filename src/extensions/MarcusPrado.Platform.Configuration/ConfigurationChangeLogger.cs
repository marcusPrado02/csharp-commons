using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace MarcusPrado.Platform.Configuration;

/// <summary>
/// Logs configuration changes by serializing old and new option values using <see cref="JsonSerializer"/>.
/// </summary>
public sealed partial class ConfigurationChangeLogger
{
    private readonly ILogger<ConfigurationChangeLogger> _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="ConfigurationChangeLogger"/>.
    /// </summary>
    /// <param name="logger">The logger to write change information to.</param>
    public ConfigurationChangeLogger(ILogger<ConfigurationChangeLogger> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Logs a configuration change event, displaying old and new values as JSON.
    /// </summary>
    /// <typeparam name="T">The options type.</typeparam>
    /// <param name="oldValue">The previous options value.</param>
    /// <param name="newValue">The new options value.</param>
    public void LogChange<T>(T oldValue, T newValue)
    {
        if (!_logger.IsEnabled(LogLevel.Information))
            return;

        var oldJson = JsonSerializer.Serialize(oldValue, JsonSerializerOptions.Web);
        var newJson = JsonSerializer.Serialize(newValue, JsonSerializerOptions.Web);

        LogConfigurationChanged(_logger, typeof(T).Name, oldJson, newJson);
    }

    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Information,
        Message = "Configuration changed for {OptionsType}. Old: {OldValue} | New: {NewValue}"
    )]
    private static partial void LogConfigurationChanged(
        ILogger logger,
        string optionsType,
        string oldValue,
        string newValue
    );
}
