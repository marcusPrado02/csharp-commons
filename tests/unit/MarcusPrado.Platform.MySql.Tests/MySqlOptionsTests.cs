namespace MarcusPrado.Platform.MySql.Tests;

public sealed class MySqlOptionsTests
{
    [Fact]
    public void DefaultMaxRetryCount_ShouldBeThree()
    {
        var opts = new MySqlOptions();
        opts.MaxRetryCount.Should().Be(3);
    }

    [Fact]
    public void DefaultServerVersion_ShouldNotBeEmpty()
    {
        var opts = new MySqlOptions();
        opts.ServerVersion.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void DefaultMaxRetryDelay_ShouldBeFiveSeconds()
    {
        var opts = new MySqlOptions();
        opts.MaxRetryDelay.Should().Be(TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void DefaultConnectionString_ShouldBeEmpty()
    {
        var opts = new MySqlOptions();
        opts.ConnectionString.Should().BeEmpty();
    }

    [Fact]
    public void Properties_ShouldBeSettable()
    {
        var opts = new MySqlOptions
        {
            ConnectionString = "Server=db;Database=mydb;User=sa;Password=pass;",
            ServerVersion = "9.0.0",
            MaxRetryCount = 5,
            MaxRetryDelay = TimeSpan.FromSeconds(10),
        };

        opts.ConnectionString.Should().Contain("db");
        opts.ServerVersion.Should().Be("9.0.0");
        opts.MaxRetryCount.Should().Be(5);
        opts.MaxRetryDelay.Should().Be(TimeSpan.FromSeconds(10));
    }
}
