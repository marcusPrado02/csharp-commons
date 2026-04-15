using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Networks;

namespace MarcusPrado.Platform.IntegrationTestEnvironment;

/// <summary>
/// Creates and manages a shared Docker network for test containers so that
/// containers within the same test environment can communicate by container name.
/// </summary>
public sealed class TestNetworkBuilder : IAsyncDisposable
{
    private INetwork? _network;

    /// <summary>
    /// Creates a new Docker network with the specified name and returns it.
    /// If a network was previously created by this instance, it is replaced.
    /// </summary>
    /// <param name="name">The name to assign to the Docker network.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> to observe while creating the network.</param>
    /// <returns>The created <see cref="INetwork"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="name"/> is <see langword="null"/>.</exception>
    public async Task<INetwork> CreateNetworkAsync(string name, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(name);

        if (_network is not null)
        {
            await _network.DisposeAsync().ConfigureAwait(false);
        }

        _network = new NetworkBuilder().WithName(name).Build();

        await _network.CreateAsync(ct).ConfigureAwait(false);

        return _network;
    }

    /// <summary>
    /// Disposes the Docker network created by this builder, if any.
    /// </summary>
    /// <returns>A <see cref="ValueTask"/> that completes when the network has been disposed.</returns>
    public async ValueTask DisposeAsync()
    {
        if (_network is not null)
        {
            await _network.DisposeAsync().ConfigureAwait(false);
            _network = null;
        }
    }
}
