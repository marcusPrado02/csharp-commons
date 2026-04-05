namespace MarcusPrado.Platform.MySql.Tests;

public sealed class MySqlHealthProbeTests
{
    private static HealthCheckContext MakeContext(IHealthCheck probe) =>
        new()
        {
            Registration = new HealthCheckRegistration("mysql", probe, failureStatus: null, tags: null)
        };

    [Fact]
    public async Task CheckHealthAsync_WhenFactorySucceeds_ReturnsHealthy()
    {
        var mockConn = Substitute.For<IDbConnection>();
        var mockFactory = Substitute.For<IMySqlConnectionFactory>();
        mockFactory.CreateOpenConnectionAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(mockConn));

        var probe = new MySqlHealthProbe(mockFactory);
        var ctx = MakeContext(probe);

        var result = await probe.CheckHealthAsync(ctx);

        result.Status.Should().Be(HealthStatus.Healthy);
        result.Description.Should().Contain("OK");
    }

    [Fact]
    public async Task CheckHealthAsync_WhenFactoryThrows_ReturnsUnhealthy()
    {
        var mockFactory = Substitute.For<IMySqlConnectionFactory>();
        mockFactory.CreateOpenConnectionAsync(Arg.Any<CancellationToken>())
            .Returns<Task<IDbConnection>>(Task.FromException<IDbConnection>(
                new InvalidOperationException("connection refused")));

        var probe = new MySqlHealthProbe(mockFactory);
        var ctx = MakeContext(probe);

        var result = await probe.CheckHealthAsync(ctx);

        result.Status.Should().Be(HealthStatus.Unhealthy);
        result.Exception.Should().NotBeNull();
        result.Exception!.Message.Should().Contain("connection refused");
    }

    [Fact]
    public async Task CheckHealthAsync_WhenCancelled_ReturnsUnhealthy()
    {
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        var mockFactory = Substitute.For<IMySqlConnectionFactory>();
        mockFactory.CreateOpenConnectionAsync(Arg.Any<CancellationToken>())
            .Returns<Task<IDbConnection>>(callInfo =>
                Task.FromException<IDbConnection>(new OperationCanceledException(cts.Token)));

        var probe = new MySqlHealthProbe(mockFactory);
        var ctx = MakeContext(probe);

        var result = await probe.CheckHealthAsync(ctx, cts.Token);

        result.Status.Should().Be(HealthStatus.Unhealthy);
    }

    [Fact]
    public void Constructor_ThrowsOnNullFactory()
    {
        var act = () => new MySqlHealthProbe(null!);
        act.Should().Throw<ArgumentNullException>();
    }
}
