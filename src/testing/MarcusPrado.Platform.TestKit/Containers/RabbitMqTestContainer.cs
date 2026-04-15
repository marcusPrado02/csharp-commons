using Testcontainers.RabbitMq;

namespace MarcusPrado.Platform.TestKit.Containers;

/// <summary>Wraps a RabbitMQ management Testcontainer.</summary>
public sealed class RabbitMqTestContainer : IAsyncDisposable
{
    private readonly RabbitMqContainer _container;

    /// <summary>Initialises the container with RabbitMQ management image.</summary>
    public RabbitMqTestContainer()
    {
        _container = new RabbitMqBuilder().WithImage("rabbitmq:3-management-alpine").WithCleanUp(true).Build();
    }

    /// <summary>Gets the AMQP connection string once the container is started.</summary>
    public string ConnectionString => _container.GetConnectionString();

    /// <summary>Starts the container.</summary>
    public Task StartAsync() => _container.StartAsync();

    /// <summary>Stops the container.</summary>
    public Task StopAsync() => _container.StopAsync();

    /// <inheritdoc/>
    public ValueTask DisposeAsync() => _container.DisposeAsync();
}
