namespace MarcusPrado.Platform.MySql.Tests;

public sealed class MySqlExtensionsTests
{
    [Fact]
    public void AddPlatformMySql_ShouldRegisterConnectionFactory()
    {
        var sp = new ServiceCollection()
            .AddOptions()
            .AddPlatformMySql(o => o.ConnectionString = "Server=localhost;Database=test;User=root;Password=root;")
            .BuildServiceProvider();

        var factory = sp.GetRequiredService<IMySqlConnectionFactory>();
        factory.Should().NotBeNull();
        factory.Should().BeOfType<MySqlConnectionFactory>();
    }

    [Fact]
    public void AddPlatformMySql_ShouldConfigureOptions()
    {
        var sp = new ServiceCollection()
            .AddOptions()
            .AddPlatformMySql(o =>
            {
                o.ConnectionString = "Server=localhost;Database=test;User=root;Password=root;";
                o.ServerVersion = "8.0.33";
                o.MaxRetryCount = 5;
            })
            .BuildServiceProvider();

        var opts = sp.GetRequiredService<IOptions<MySqlOptions>>().Value;
        opts.ConnectionString.Should().Contain("localhost");
        opts.ServerVersion.Should().Be("8.0.33");
        opts.MaxRetryCount.Should().Be(5);
    }

    [Fact]
    public void AddPlatformMySql_ThrowsOnNullServices()
    {
        IServiceCollection services = null!;
        var act = () => services.AddPlatformMySql(o => o.ConnectionString = "x");
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void AddPlatformMySql_ThrowsOnNullConfigure()
    {
        var services = new ServiceCollection();
        var act = () => services.AddPlatformMySql(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void AddMySqlHealthCheck_ShouldRegisterHealthProbe()
    {
        var sp = new ServiceCollection()
            .AddOptions()
            .AddPlatformMySql(o => o.ConnectionString = "Server=localhost;")
            .AddHealthChecks()
            .AddMySqlHealthCheck("mysql-test")
            .Services
            .BuildServiceProvider();

        var probe = sp.GetRequiredService<MySqlHealthProbe>();
        probe.Should().NotBeNull();
    }

    [Fact]
    public void AddPlatformMySql_CalledTwice_ConnectionFactoryStillResolvable()
    {
        var sp = new ServiceCollection()
            .AddOptions()
            .AddPlatformMySql(o => o.ConnectionString = "Server=a;")
            .AddPlatformMySql(o => o.ConnectionString = "Server=b;")
            .BuildServiceProvider();

        // Should not throw - last registration wins for Configure, factory is present
        var factory = sp.GetRequiredService<IMySqlConnectionFactory>();
        factory.Should().NotBeNull();
    }
}
