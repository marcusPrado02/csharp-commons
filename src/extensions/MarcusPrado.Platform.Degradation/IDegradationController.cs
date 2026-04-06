namespace MarcusPrado.Platform.Degradation;

/// <summary>
/// Reads and writes the current <see cref="DegradationMode"/> for the running application.
/// </summary>
public interface IDegradationController
{
    /// <summary>
    /// Sets the current degradation mode.
    /// </summary>
    /// <param name="mode">The new degradation mode to apply.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> to observe.</param>
    Task SetModeAsync(DegradationMode mode, CancellationToken ct = default);

    /// <summary>
    /// Returns the current degradation mode.
    /// </summary>
    /// <param name="ct">A <see cref="CancellationToken"/> to observe.</param>
    Task<DegradationMode> GetModeAsync(CancellationToken ct = default);
}
