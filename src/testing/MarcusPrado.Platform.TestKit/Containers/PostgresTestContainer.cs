using DotNet.Testcontainers.Builders;
using Testcontainers.PostgreSql;

namespace MarcusPrado.Platform.TestKit.Containers;

/// <summary>
/// Wraps a PostgreSQL Testcontainer with an isolated database per test run.
/// </summary>
public sealed class PostgresTestContainer : IAsyncDisposable
{
    private readonly PostgreSqlContainer _container;

    /// <summary>Initialises the container with default PostgreSQL latest image.</summary>
    public PostgresTestContainer()
    {
        _container = new PostgreSqlBuilder()
            .WithImage("postgres:16-alpine")
            .WithDatabase("testdb")
            .WithUsername("testuser")
            .WithPassword("testpass")
            .WithCleanUp(true)
            .Build();
    }

    /// <summary>Gets the connection string once the container is started.</summary>
    public string ConnectionString => _container.GetConnectionString();

    /// <summary>Starts the container.</summary>
    public Task StartAsync() => _container.StartAsync();

    /// <summary>Stops the container.</summary>
    public Task StopAsync() => _container.StopAsync();

    /// <inheritdoc/>
    public ValueTask DisposeAsync() => _container.DisposeAsync();
}
