using Microsoft.Extensions.Logging.Abstractions;

namespace MarcusPrado.Platform.AzureServiceBus.Tests;

public sealed class ServiceBusOptionsTests
{
    [Fact]
    public void ServiceBusOptions_Defaults_AreCorrect()
    {
        var opts = new ServiceBusOptions();

        opts.ConnectionString.Should().BeNull();
        opts.FullyQualifiedNamespace.Should().BeNull();
        opts.MaxConcurrentCalls.Should().Be(10);
        opts.MaxAutoLockRenewalDuration.Should().Be(TimeSpan.FromMinutes(5));
    }

    [Fact]
    public void ServiceBusOptions_Configure_AppliesValues()
    {
        var opts = new ServiceBusOptions
        {
            ConnectionString =
                "Endpoint=sb://test.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=abc123",
            FullyQualifiedNamespace = "test.servicebus.windows.net",
            MaxConcurrentCalls = 5,
            MaxAutoLockRenewalDuration = TimeSpan.FromMinutes(2),
        };

        opts.ConnectionString.Should().NotBeNullOrEmpty();
        opts.FullyQualifiedNamespace.Should().Be("test.servicebus.windows.net");
        opts.MaxConcurrentCalls.Should().Be(5);
        opts.MaxAutoLockRenewalDuration.Should().Be(TimeSpan.FromMinutes(2));
    }
}

public sealed class ServiceBusPublisherTests
{
    [Fact]
    public void ServiceBusPublisher_NullClient_Throws()
    {
        var act = () => new ServiceBusPublisher(null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("client");
    }

    [Fact]
    public void ServiceBusPublisher_ValidClient_Constructs()
    {
        var client = Substitute.For<ServiceBusClient>();

        var act = () => new ServiceBusPublisher(client);

        act.Should().NotThrow();
    }
}

public sealed class ServiceBusConsumerTests
{
    private static IOptions<ServiceBusOptions> DefaultOptions() =>
        Microsoft.Extensions.Options.Options.Create(new ServiceBusOptions());

    [Fact]
    public void ServiceBusConsumer_NullClient_Throws()
    {
        var act = () => new ServiceBusConsumer(null!, DefaultOptions(), NullLogger<ServiceBusConsumer>.Instance);

        act.Should().Throw<ArgumentNullException>().WithParameterName("client");
    }

    [Fact]
    public void ServiceBusConsumer_NullOptions_Throws()
    {
        var client = Substitute.For<ServiceBusClient>();

        var act = () => new ServiceBusConsumer(client, null!, NullLogger<ServiceBusConsumer>.Instance);

        act.Should().Throw<ArgumentNullException>().WithParameterName("options");
    }

    [Fact]
    public void ServiceBusConsumer_NullLogger_Throws()
    {
        var client = Substitute.For<ServiceBusClient>();

        var act = () => new ServiceBusConsumer(client, DefaultOptions(), null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
    }
}

public sealed class ServiceBusHealthProbeTests
{
    [Fact]
    public void ServiceBusHealthProbe_NullClient_Throws()
    {
        var act = () => new ServiceBusHealthProbe(null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("client");
    }
}

public sealed class ServiceBusExtensionsTests
{
    [Fact]
    public void AddPlatformAzureServiceBus_RegistersPublisher()
    {
        var services = new ServiceCollection();

        services.AddPlatformAzureServiceBus(opts =>
            opts.ConnectionString =
                "Endpoint=sb://test.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=abc123="
        );

        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IServiceBusPublisher));
        descriptor.Should().NotBeNull();
        descriptor!.Lifetime.Should().Be(ServiceLifetime.Singleton);
    }

    [Fact]
    public void AddPlatformAzureServiceBus_RegistersConsumer()
    {
        var services = new ServiceCollection();

        services.AddPlatformAzureServiceBus(opts =>
            opts.ConnectionString =
                "Endpoint=sb://test.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=abc123="
        );

        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IServiceBusConsumer));
        descriptor.Should().NotBeNull();
        descriptor!.Lifetime.Should().Be(ServiceLifetime.Singleton);
    }

    [Fact]
    public void AddPlatformAzureServiceBus_WithConnectionString_RegistersClient()
    {
        var services = new ServiceCollection();

        services.AddPlatformAzureServiceBus(opts =>
            opts.ConnectionString =
                "Endpoint=sb://test.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=abc123="
        );

        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(ServiceBusClient));
        descriptor.Should().NotBeNull();
        descriptor!.Lifetime.Should().Be(ServiceLifetime.Singleton);
    }

    [Fact]
    public void AddPlatformAzureServiceBus_NullConfigure_Throws()
    {
        var services = new ServiceCollection();

        var act = () => services.AddPlatformAzureServiceBus(null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("configure");
    }
}
