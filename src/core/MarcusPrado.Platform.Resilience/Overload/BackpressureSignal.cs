namespace MarcusPrado.Platform.Resilience.Overload;

/// <summary>
/// A binary flag that signals back-pressure to upstream callers.
/// Thread-safe; can be set/cleared from any thread.
/// </summary>
public sealed class BackpressureSignal
{
    private volatile bool _active;

    /// <summary>Gets whether back-pressure is currently active.</summary>
    public bool IsActive => _active;

    /// <summary>Activates back-pressure signalling.</summary>
    public void Activate() => _active = true;

    /// <summary>Clears back-pressure signalling.</summary>
    public void Clear() => _active = false;
}
