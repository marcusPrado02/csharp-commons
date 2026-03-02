using Testcontainers.Kafka;

namespace MarcusPrado.Platform.TestKit.Containers;

/// <summary>Wraps a Kafka Testcontainer (Confluent cp-kafka).</summary>
public sealed class KafkaTestContainer : IAsyncDisposable
{
    private readonly KafkaContainer _container;

    /// <summary>Initialises the container with Confluent Kafka image.</summary>
    public KafkaTestContainer()
    {
        _container = new KafkaBuilder()
            .WithCleanUp(true)
            .Build();
    }

    /// <summary>Gets the bootstrap server address once the container is started.</summary>
    public string BootstrapServers => _container.GetBootstrapAddress();

    /// <summary>Starts the container.</summary>
    public Task StartAsync() => _container.StartAsync();

    /// <summary>Stops the container.</summary>
    public Task StopAsync() => _container.StopAsync();

    /// <inheritdoc/>
    public ValueTask DisposeAsync() => _container.DisposeAsync();
}
