using StackExchange.Redis;
using Testcontainers.Redis;

namespace MarcusPrado.Platform.TestKit.Containers;

/// <summary>
/// Wraps a Redis Testcontainer. Calls <see cref="FlushAllAsync"/> in TearDown
/// to ensure test isolation.
/// </summary>
public sealed class RedisTestContainer : IAsyncDisposable
{
    private readonly RedisContainer _container;

    /// <summary>Initialises the container with default Redis latest image.</summary>
    public RedisTestContainer()
    {
        _container = new RedisBuilder()
            .WithImage("redis:7-alpine")
            .WithCleanUp(true)
            .Build();
    }

    /// <summary>Gets the connection string once the container is started.</summary>
    public string ConnectionString => _container.GetConnectionString();

    /// <summary>Starts the container.</summary>
    public Task StartAsync() => _container.StartAsync();

    /// <summary>
    /// Flushes all keys from the Redis instance to restore a clean state between tests.
    /// </summary>
    public async Task FlushAllAsync()
    {
        using var mux = await ConnectionMultiplexer.ConnectAsync(ConnectionString);
        await mux.GetDatabase().ExecuteAsync("FLUSHALL");
    }

    /// <summary>Stops the container.</summary>
    public Task StopAsync() => _container.StopAsync();

    /// <inheritdoc/>
    public ValueTask DisposeAsync() => _container.DisposeAsync();
}
