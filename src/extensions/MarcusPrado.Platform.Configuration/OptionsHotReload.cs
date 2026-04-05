using Microsoft.Extensions.Options;

namespace MarcusPrado.Platform.Configuration;

/// <summary>
/// Implements <see cref="IOptionsHotReload{T}"/> by wrapping <see cref="IOptionsMonitor{TOptions}"/>
/// and logging changes via <see cref="ConfigurationChangeLogger"/>.
/// </summary>
/// <typeparam name="T">The options type.</typeparam>
public sealed class OptionsHotReload<T> : IOptionsHotReload<T>
    where T : class
{
    private readonly IOptionsMonitor<T> _monitor;
    private readonly ConfigurationChangeLogger _changeLogger;

    /// <summary>
    /// Initializes a new instance of <see cref="OptionsHotReload{T}"/>.
    /// </summary>
    /// <param name="monitor">The underlying options monitor.</param>
    /// <param name="changeLogger">The logger used to record configuration changes.</param>
    public OptionsHotReload(IOptionsMonitor<T> monitor, ConfigurationChangeLogger changeLogger)
    {
        _monitor = monitor ?? throw new ArgumentNullException(nameof(monitor));
        _changeLogger = changeLogger ?? throw new ArgumentNullException(nameof(changeLogger));
    }

    /// <inheritdoc />
    public T CurrentValue => _monitor.CurrentValue;

    /// <inheritdoc />
    public IDisposable OnChange(Action<T> listener)
    {
        ArgumentNullException.ThrowIfNull(listener);

        T? previousValue = null;

        return _monitor.OnChange(newValue =>
        {
            var old = previousValue ?? newValue;
            _changeLogger.LogChange(old, newValue);
            previousValue = newValue;
            listener(newValue);
        }) ?? new NullDisposable();
    }

    private sealed class NullDisposable : IDisposable
    {
        public void Dispose() { }
    }
}
