using Microsoft.Extensions.Options;

namespace MarcusPrado.Platform.Configuration;

/// <summary>
/// Wraps <see cref="IOptionsMonitor{TOptions}"/> to provide hot-reload capability for configuration options.
/// </summary>
/// <typeparam name="T">The options type.</typeparam>
public interface IOptionsHotReload<out T>
    where T : class
{
    /// <summary>
    /// Gets the current value of the options.
    /// </summary>
    T CurrentValue { get; }

    /// <summary>
    /// Registers a listener to be called when the options instance changes.
    /// </summary>
    /// <param name="listener">The action to be invoked when the options change.</param>
    /// <returns>An <see cref="IDisposable"/> that should be disposed to stop listening for changes.</returns>
    IDisposable OnChange(Action<T> listener);
}
