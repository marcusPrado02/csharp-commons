using MarcusPrado.Platform.Messaging.Abstractions;

namespace MarcusPrado.Platform.RabbitMq.Tests.Unit;

public sealed class RabbitMqExtensionsTests
{
    [Fact]
    public void AddPlatformRabbitMq_RegistersRabbitMqOptions()
    {
        var services = new ServiceCollection();
        services.AddPlatformRabbitMq();

        var provider = services.BuildServiceProvider();
        var opts = provider.GetService<RabbitMqOptions>();

        opts.Should().NotBeNull();
    }

    [Fact]
    public void AddPlatformRabbitMq_RegistersIMessageSerializer()
    {
        var services = new ServiceCollection();
        services.AddPlatformRabbitMq();

        var provider = services.BuildServiceProvider();
        var serializer = provider.GetService<IMessageSerializer>();

        serializer.Should().NotBeNull();
    }

    [Fact]
    public void AddPlatformRabbitMq_AppliesConfigureAction()
    {
        var services = new ServiceCollection();
        services.AddPlatformRabbitMq(opts =>
        {
            opts.Exchange = "my.exchange";
            opts.ExchangeType = "direct";
        });

        var provider = services.BuildServiceProvider();
        var opts = provider.GetRequiredService<RabbitMqOptions>();

        opts.Exchange.Should().Be("my.exchange");
        opts.ExchangeType.Should().Be("direct");
    }

    [Fact]
    public void AddPlatformRabbitMq_WithoutConfigure_UsesDefaultExchange()
    {
        var services = new ServiceCollection();
        services.AddPlatformRabbitMq();

        var provider = services.BuildServiceProvider();
        var opts = provider.GetRequiredService<RabbitMqOptions>();

        opts.Exchange.Should().Be("platform.events");
    }
}
