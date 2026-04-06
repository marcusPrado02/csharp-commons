namespace MarcusPrado.Platform.Degradation;

/// <summary>
/// An in-memory implementation of <see cref="IDegradationController"/> suitable for
/// testing and single-process scenarios.
/// </summary>
public sealed class InMemoryDegradationController : IDegradationController
{
    private volatile DegradationMode _mode = DegradationMode.None;

    /// <inheritdoc />
    public Task SetModeAsync(DegradationMode mode, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        _mode = mode;
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<DegradationMode> GetModeAsync(CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        return Task.FromResult(_mode);
    }
}
