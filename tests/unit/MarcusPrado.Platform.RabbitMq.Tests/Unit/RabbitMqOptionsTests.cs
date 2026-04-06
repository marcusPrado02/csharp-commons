namespace MarcusPrado.Platform.RabbitMq.Tests.Unit;

public sealed class RabbitMqOptionsTests
{
    [Fact]
    public void RabbitMqOptions_DefaultConnectionString_IsAmqpLocalhost()
    {
        var opts = new RabbitMqOptions();

        opts.ConnectionString.Should().StartWith("amqp://");
        opts.ConnectionString.Should().Contain("localhost");
    }

    [Fact]
    public void RabbitMqOptions_DefaultExchange_IsNotEmpty()
    {
        var opts = new RabbitMqOptions();

        opts.Exchange.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void RabbitMqOptions_DefaultExchangeType_IsTopicOrDirect()
    {
        var opts = new RabbitMqOptions();

        var validTypes = new[] { "topic", "direct", "fanout", "headers" };
        validTypes.Should().Contain(opts.ExchangeType);
    }

    [Fact]
    public void RabbitMqOptions_DefaultPrefetchCount_IsPositive()
    {
        var opts = new RabbitMqOptions();

        opts.PrefetchCount.Should().BeGreaterThan((ushort)0);
    }

    [Fact]
    public void RabbitMqOptions_ConnectionString_CanBeCustomised()
    {
        var opts = new RabbitMqOptions { ConnectionString = "amqp://user:pass@rabbit:5672/vhost" };

        opts.ConnectionString.Should().Be("amqp://user:pass@rabbit:5672/vhost");
    }
}
