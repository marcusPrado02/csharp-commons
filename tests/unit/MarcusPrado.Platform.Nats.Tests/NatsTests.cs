using MarcusPrado.Platform.Nats.Extensions;

namespace MarcusPrado.Platform.Nats.Tests;

// ──────────────────────────────────────────────────────────────────────────────
// NatsOptions tests
// ──────────────────────────────────────────────────────────────────────────────
public sealed class NatsOptionsTests
{
    [Fact]
    public void NatsOptions_Defaults_AreCorrect()
    {
        var opts = new NatsOptions();

        opts.Url.Should().Be("nats://localhost:4222");
        opts.MaxReconnectAttempts.Should().Be(3);
        opts.JetStream.Should().BeFalse();
    }

    [Fact]
    public void NatsOptions_Configure_AppliesValues()
    {
        var opts = new NatsOptions();

        opts.Url = "nats://my-server:4222";
        opts.MaxReconnectAttempts = 10;
        opts.JetStream = true;

        opts.Url.Should().Be("nats://my-server:4222");
        opts.MaxReconnectAttempts.Should().Be(10);
        opts.JetStream.Should().BeTrue();
    }
}

// ──────────────────────────────────────────────────────────────────────────────
// NatsPublisher tests
// ──────────────────────────────────────────────────────────────────────────────
public sealed class NatsPublisherTests
{
    [Fact]
    public void NatsPublisher_NullConnection_Throws()
    {
        var act = () => new NatsPublisher(null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("connection");
    }

    [Fact]
    public async Task NatsPublisher_PublishAsync_NullMessage_Throws()
    {
        var conn = Substitute.For<INatsConnection>();
        var publisher = new NatsPublisher(conn);

        var act = async () => await publisher.PublishAsync<SampleMessage>("subject", null!);

        await act.Should().ThrowAsync<ArgumentNullException>().WithParameterName("message");
    }

    [Fact]
    public async Task NatsPublisher_PublishAsync_EmptySubject_Throws()
    {
        var conn = Substitute.For<INatsConnection>();
        var publisher = new NatsPublisher(conn);

        var act = async () => await publisher.PublishAsync("", new SampleMessage("hello"));

        await act.Should().ThrowAsync<ArgumentException>().WithParameterName("subject");
    }

    [Fact]
    public async Task NatsPublisher_PublishAsync_CallsConnection()
    {
        var conn = Substitute.For<INatsConnection>();
        var publisher = new NatsPublisher(conn);
        var message = new SampleMessage("hello-world");

        await publisher.PublishAsync("test.subject", message);

        // The publisher serializes and calls PublishAsync on the connection.
        await conn.Received(1).PublishAsync("test.subject", Arg.Is<string>(s => s.Contains("hello-world")));
    }
}

// ──────────────────────────────────────────────────────────────────────────────
// NatsConsumer tests
// ──────────────────────────────────────────────────────────────────────────────
public sealed class NatsConsumerTests
{
    [Fact]
    public void NatsConsumer_NullConnection_Throws()
    {
        var act = () => new NatsConsumer(null!, new NatsOptions());

        act.Should().Throw<ArgumentNullException>().WithParameterName("connection");
    }

    [Fact]
    public void NatsConsumer_NullOptions_Throws()
    {
        var conn = Substitute.For<INatsConnection>();

        var act = () => new NatsConsumer(conn, null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("options");
    }

    [Fact]
    public async Task NatsConsumer_SubscribeAsync_NullHandler_Throws()
    {
        var conn = Substitute.For<INatsConnection>();
        var consumer = new NatsConsumer(conn, new NatsOptions());

        var act = async () =>
            await consumer.SubscribeAsync<SampleMessage>("test.subject", null!, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentNullException>().WithParameterName("handler");
    }

    [Fact]
    public async Task NatsConsumer_SubscribeAsync_EmptySubject_Throws()
    {
        var conn = Substitute.For<INatsConnection>();
        var consumer = new NatsConsumer(conn, new NatsOptions());

        var act = async () =>
            await consumer.SubscribeAsync<SampleMessage>(
                string.Empty,
                (_, _) => Task.CompletedTask,
                CancellationToken.None
            );

        await act.Should().ThrowAsync<ArgumentException>().WithParameterName("subject");
    }
}

// ──────────────────────────────────────────────────────────────────────────────
// NatsHealthProbe tests
// ──────────────────────────────────────────────────────────────────────────────
public sealed class NatsHealthProbeTests
{
    [Fact]
    public void NatsHealthProbe_NullConnection_Throws()
    {
        var act = () => new NatsHealthProbe(null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("connection");
    }

    [Fact]
    public async Task NatsHealthProbe_WhenConnectionOpen_ReturnsHealthy()
    {
        var conn = Substitute.For<INatsConnection>();
        conn.ConnectionState.Returns(NatsConnectionState.Open);
        conn.PingAsync(Arg.Any<CancellationToken>()).Returns(ValueTask.FromResult(TimeSpan.FromMilliseconds(3)));

        var probe = new NatsHealthProbe(conn);
        var context = BuildContext();

        var result = await probe.CheckHealthAsync(context, CancellationToken.None);

        result.Status.Should().Be(HealthStatus.Healthy);
        result.Description.Should().Contain("RTT");
    }

    [Fact]
    public async Task NatsHealthProbe_WhenConnectionClosed_ReturnsUnhealthy()
    {
        var conn = Substitute.For<INatsConnection>();
        conn.ConnectionState.Returns(NatsConnectionState.Closed);

        var probe = new NatsHealthProbe(conn);
        var context = BuildContext();

        var result = await probe.CheckHealthAsync(context, CancellationToken.None);

        result.Status.Should().Be(HealthStatus.Unhealthy);
        result.Description.Should().Contain("Closed");
    }

    [Fact]
    public async Task NatsHealthProbe_WhenPingThrows_ReturnsUnhealthy()
    {
        var conn = Substitute.For<INatsConnection>();
        conn.ConnectionState.Returns(NatsConnectionState.Open);
        conn.When(c => c.PingAsync(Arg.Any<CancellationToken>())).Throw(new NatsException("server unreachable"));

        var probe = new NatsHealthProbe(conn);
        var context = BuildContext();

        var result = await probe.CheckHealthAsync(context, CancellationToken.None);

        result.Status.Should().Be(HealthStatus.Unhealthy);
    }

    private static HealthCheckContext BuildContext() =>
        new()
        {
            Registration = new HealthCheckRegistration(
                "nats",
                Substitute.For<IHealthCheck>(),
                HealthStatus.Unhealthy,
                []
            ),
        };
}

// ──────────────────────────────────────────────────────────────────────────────
// NatsExtensions (DI registration) tests
// ──────────────────────────────────────────────────────────────────────────────
public sealed class NatsExtensionsTests
{
    [Fact]
    public void AddPlatformNats_RegistersPublisher()
    {
        var services = new ServiceCollection();
        services.AddPlatformNats();

        var sp = services.BuildServiceProvider();

        sp.GetRequiredService<INatsPublisher>().Should().BeOfType<NatsPublisher>();
    }

    [Fact]
    public void AddPlatformNats_RegistersConsumer()
    {
        var services = new ServiceCollection();
        services.AddPlatformNats();

        var sp = services.BuildServiceProvider();

        sp.GetRequiredService<INatsConsumer>().Should().BeOfType<NatsConsumer>();
    }

    [Fact]
    public void AddPlatformNats_RegistersHealthCheck()
    {
        var services = new ServiceCollection();
        services.AddPlatformNats();

        // HealthCheckServiceOptions holds the registrations.
        var sp = services.BuildServiceProvider();
        var healthOptions = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<HealthCheckServiceOptions>>();

        healthOptions.Value.Registrations.Should().Contain(r => r.Name == "nats");
    }

    [Fact]
    public void AddPlatformNats_NullServices_Throws()
    {
        IServiceCollection services = null!;

        var act = () => services.AddPlatformNats();

        act.Should().Throw<ArgumentNullException>().WithParameterName("services");
    }

    [Fact]
    public void AddPlatformNats_ConfigureAppliesOptions()
    {
        var services = new ServiceCollection();
        services.AddPlatformNats(o =>
        {
            o.Url = "nats://custom:4222";
            o.MaxReconnectAttempts = 5;
            o.JetStream = true;
        });

        var sp = services.BuildServiceProvider();
        var opts = sp.GetRequiredService<NatsOptions>();

        opts.Url.Should().Be("nats://custom:4222");
        opts.MaxReconnectAttempts.Should().Be(5);
        opts.JetStream.Should().BeTrue();
    }
}

// ──────────────────────────────────────────────────────────────────────────────
// Shared fixture types
// ──────────────────────────────────────────────────────────────────────────────
internal sealed record SampleMessage(string Content);
