namespace MarcusPrado.Platform.Runtime.Lifecycle;

/// <summary>
/// Lifecycle callbacks that a service can implement to receive notifications
/// at application start, graceful-stop initiation, and full stop.
/// </summary>
public interface IHostedLifecycle
{
    /// <summary>Called after the application host has fully started.</summary>
    Task OnStartedAsync(CancellationToken cancellationToken);

    /// <summary>Called when a graceful-stop signal has been received.</summary>
    Task OnStoppingAsync(CancellationToken cancellationToken);

    /// <summary>Called after all hosted services have stopped.</summary>
    Task OnStoppedAsync(CancellationToken cancellationToken);
}
