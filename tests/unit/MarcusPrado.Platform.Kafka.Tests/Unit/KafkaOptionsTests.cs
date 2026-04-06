namespace MarcusPrado.Platform.Kafka.Tests.Unit;

public sealed class KafkaOptionsTests
{
    [Fact]
    public void KafkaOptions_DefaultBootstrapServers_IsLocalhost()
    {
        var opts = new KafkaOptions();

        opts.BootstrapServers.Should().Be("localhost:9092");
    }

    [Fact]
    public void KafkaOptions_DefaultConsumerGroupId_IsNotEmpty()
    {
        var opts = new KafkaOptions();

        opts.ConsumerGroupId.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void KafkaOptions_DefaultMaxRetries_IsPositive()
    {
        var opts = new KafkaOptions();

        opts.MaxRetries.Should().BeGreaterThan(0);
    }

    [Fact]
    public void KafkaOptions_DefaultDlqSuffix_IsNotEmpty()
    {
        var opts = new KafkaOptions();

        opts.DlqSuffix.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void KafkaOptions_SetBootstrapServers_Persists()
    {
        var opts = new KafkaOptions { BootstrapServers = "broker:9092" };

        opts.BootstrapServers.Should().Be("broker:9092");
    }

    [Fact]
    public void KafkaOptions_TopicPrefix_CanBeSet()
    {
        var opts = new KafkaOptions { TopicPrefix = "myapp." };

        opts.TopicPrefix.Should().Be("myapp.");
    }
}
