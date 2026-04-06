using MarcusPrado.Platform.Messaging.Abstractions;

namespace MarcusPrado.Platform.Kafka.Tests.Unit;

public sealed class KafkaExtensionsTests
{
    [Fact]
    public void AddPlatformKafka_RegistersKafkaOptions()
    {
        var services = new ServiceCollection();
        services.AddPlatformKafka();

        var provider = services.BuildServiceProvider();
        var opts = provider.GetService<KafkaOptions>();

        opts.Should().NotBeNull();
    }

    [Fact]
    public void AddPlatformKafka_RegistersIMessagePublisher()
    {
        var services = new ServiceCollection();
        services.AddPlatformKafka();

        var provider = services.BuildServiceProvider();
        var publisher = provider.GetService<IMessagePublisher>();

        publisher.Should().NotBeNull();
        publisher.Should().BeOfType<KafkaProducer>();
    }

    [Fact]
    public void AddPlatformKafka_AppliesConfigureAction()
    {
        var services = new ServiceCollection();
        services.AddPlatformKafka(opts =>
        {
            opts.BootstrapServers = "custom-broker:9093";
            opts.TopicPrefix = "test.";
        });

        var provider = services.BuildServiceProvider();
        var opts = provider.GetRequiredService<KafkaOptions>();

        opts.BootstrapServers.Should().Be("custom-broker:9093");
        opts.TopicPrefix.Should().Be("test.");
    }

    [Fact]
    public void AddPlatformKafka_WithoutConfigure_UsesDefaults()
    {
        var services = new ServiceCollection();
        services.AddPlatformKafka();

        var provider = services.BuildServiceProvider();
        var opts = provider.GetRequiredService<KafkaOptions>();

        opts.BootstrapServers.Should().Be("localhost:9092");
        opts.TopicPrefix.Should().BeEmpty();
    }
}
