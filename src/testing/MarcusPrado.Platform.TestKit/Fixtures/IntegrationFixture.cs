using Xunit;

namespace MarcusPrado.Platform.TestKit.Fixtures;

/// <summary>
/// Base class for integration test collections that manage the lifecycle of
/// Testcontainers. Extend this class and add the containers you need.
/// </summary>
public abstract class IntegrationFixture : IAsyncLifetime
{
    /// <summary>Starts all registered containers.</summary>
    public virtual async Task InitializeAsync()
    {
        await StartContainersAsync();
    }

    /// <summary>Stops and disposes all registered containers.</summary>
    public virtual async Task DisposeAsync()
    {
        await StopContainersAsync();
    }

    /// <summary>Override to start specific containers.</summary>
    protected virtual Task StartContainersAsync() => Task.CompletedTask;

    /// <summary>Override to stop specific containers.</summary>
    protected virtual Task StopContainersAsync() => Task.CompletedTask;
}
