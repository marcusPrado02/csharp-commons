using Microsoft.Extensions.Logging.Abstractions;

namespace MarcusPrado.Platform.AwsSqs.Tests;

public sealed class SqsOptionsTests
{
    [Fact]
    public void SqsOptions_Defaults_AreCorrect()
    {
        var opts = new SqsOptions();

        opts.ServiceUrl.Should().BeNull();
        opts.Region.Should().Be("us-east-1");
        opts.MaxNumberOfMessages.Should().Be(10);
        opts.WaitTimeSeconds.Should().Be(20);
        opts.VisibilityTimeoutSeconds.Should().Be(30);
        opts.DlqSuffix.Should().Be("-dlq");
        opts.MaxReceiveCount.Should().Be(3);
    }

    [Fact]
    public void SqsOptions_Configure_AppliesValues()
    {
        var opts = new SqsOptions
        {
            ServiceUrl = "http://localhost:4566",
            Region = "eu-west-1",
            MaxNumberOfMessages = 5,
            WaitTimeSeconds = 10,
            VisibilityTimeoutSeconds = 60,
            DlqSuffix = "-dead",
            MaxReceiveCount = 5,
        };

        opts.ServiceUrl.Should().Be("http://localhost:4566");
        opts.Region.Should().Be("eu-west-1");
        opts.MaxNumberOfMessages.Should().Be(5);
        opts.WaitTimeSeconds.Should().Be(10);
        opts.VisibilityTimeoutSeconds.Should().Be(60);
        opts.DlqSuffix.Should().Be("-dead");
        opts.MaxReceiveCount.Should().Be(5);
    }
}

public sealed class SnsOptionsTests
{
    [Fact]
    public void SnsOptions_Defaults_AreCorrect()
    {
        var opts = new SnsOptions();

        opts.ServiceUrl.Should().BeNull();
        opts.Region.Should().Be("us-east-1");
    }
}

public sealed class SqsPublisherTests
{
    [Fact]
    public void SqsPublisher_NullClient_Throws()
    {
        var act = () => new SqsPublisher(null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("client");
    }

    [Fact]
    public void SqsPublisher_ValidClient_Constructs()
    {
        var client = Substitute.For<IAmazonSQS>();

        var act = () => new SqsPublisher(client);

        act.Should().NotThrow();
    }
}

public sealed class SqsConsumerTests
{
    private static IOptions<SqsOptions> DefaultOptions() =>
        Microsoft.Extensions.Options.Options.Create(new SqsOptions());

    [Fact]
    public void SqsConsumer_NullClient_Throws()
    {
        var act = () => new SqsConsumer(null!, DefaultOptions(), NullLogger<SqsConsumer>.Instance);

        act.Should().Throw<ArgumentNullException>().WithParameterName("client");
    }

    [Fact]
    public void SqsConsumer_NullOptions_Throws()
    {
        var client = Substitute.For<IAmazonSQS>();

        var act = () => new SqsConsumer(client, null!, NullLogger<SqsConsumer>.Instance);

        act.Should().Throw<ArgumentNullException>().WithParameterName("options");
    }

    [Fact]
    public void SqsConsumer_NullLogger_Throws()
    {
        var client = Substitute.For<IAmazonSQS>();

        var act = () => new SqsConsumer(client, DefaultOptions(), null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
    }
}

public sealed class SnsPublisherTests
{
    [Fact]
    public void SnsPublisher_NullClient_Throws()
    {
        var act = () => new SnsPublisher(null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("client");
    }

    [Fact]
    public void SnsPublisher_ValidClient_Constructs()
    {
        var client = Substitute.For<IAmazonSimpleNotificationService>();

        var act = () => new SnsPublisher(client);

        act.Should().NotThrow();
    }
}

public sealed class SqsHealthProbeTests
{
    [Fact]
    public void SqsHealthProbe_NullClient_Throws()
    {
        var act = () => new SqsHealthProbe(null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("client");
    }
}

public sealed class AwsSqsExtensionsTests
{
    [Fact]
    public void AddPlatformAwsSqs_RegistersPublisher()
    {
        var services = new ServiceCollection();

        services.AddPlatformAwsSqs(opts => opts.ServiceUrl = "http://localhost:4566");

        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(ISqsPublisher));
        descriptor.Should().NotBeNull();
        descriptor!.Lifetime.Should().Be(ServiceLifetime.Singleton);
    }

    [Fact]
    public void AddPlatformAwsSqs_RegistersConsumer()
    {
        var services = new ServiceCollection();

        services.AddPlatformAwsSqs(opts => opts.ServiceUrl = "http://localhost:4566");

        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(ISqsConsumer));
        descriptor.Should().NotBeNull();
        descriptor!.Lifetime.Should().Be(ServiceLifetime.Singleton);
    }

    [Fact]
    public void AddPlatformAwsSqs_RegistersSnsPublisher()
    {
        var services = new ServiceCollection();

        services.AddPlatformAwsSqs(opts => opts.ServiceUrl = "http://localhost:4566");

        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(ISnsPublisher));
        descriptor.Should().NotBeNull();
        descriptor!.Lifetime.Should().Be(ServiceLifetime.Singleton);
    }

    [Fact]
    public void AddPlatformAwsSqs_RegistersAmazonSqsClient()
    {
        var services = new ServiceCollection();

        services.AddPlatformAwsSqs(opts => opts.ServiceUrl = "http://localhost:4566");

        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IAmazonSQS));
        descriptor.Should().NotBeNull();
        descriptor!.Lifetime.Should().Be(ServiceLifetime.Singleton);
    }

    [Fact]
    public void AddPlatformAwsSqs_RegistersAmazonSnsClient()
    {
        var services = new ServiceCollection();

        services.AddPlatformAwsSqs(opts => opts.ServiceUrl = "http://localhost:4566");

        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IAmazonSimpleNotificationService));
        descriptor.Should().NotBeNull();
        descriptor!.Lifetime.Should().Be(ServiceLifetime.Singleton);
    }
}
