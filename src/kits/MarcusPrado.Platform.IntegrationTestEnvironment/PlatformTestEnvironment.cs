using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Networks;
using Testcontainers.Kafka;
using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;
using Testcontainers.Redis;

namespace MarcusPrado.Platform.IntegrationTestEnvironment;

/// <summary>
/// Orchestrates all configured test containers in parallel and exposes their
/// connection strings after startup.
/// </summary>
public sealed class PlatformTestEnvironment : IAsyncDisposable
{
    private readonly bool _usePostgres;
    private readonly bool _useRedis;
    private readonly bool _useKafka;
    private readonly bool _useRabbitMq;

    private PostgreSqlContainer? _postgres;
    private RedisContainer? _redis;
    private KafkaContainer? _kafka;
    private RabbitMqContainer? _rabbitMq;

    private bool _started;

    internal PlatformTestEnvironment(bool usePostgres, bool useRedis, bool useKafka, bool useRabbitMq)
    {
        _usePostgres = usePostgres;
        _useRedis = useRedis;
        _useKafka = useKafka;
        _useRabbitMq = useRabbitMq;
    }

    /// <summary>
    /// Gets a static builder for configuring which containers to start.
    /// </summary>
    /// <returns>A new <see cref="PlatformTestEnvironmentBuilder"/> instance.</returns>
    public static PlatformTestEnvironmentBuilder CreateBuilder() => new();

    /// <summary>
    /// Gets the Postgres connection string. Only available after <see cref="StartAsync"/> completes
    /// and when Postgres was enabled via the builder.
    /// </summary>
    public string PostgresConnectionString =>
        _postgres?.GetConnectionString()
        ?? throw new InvalidOperationException("Postgres container is not configured or has not been started.");

    /// <summary>
    /// Gets the Redis connection string. Only available after <see cref="StartAsync"/> completes
    /// and when Redis was enabled via the builder.
    /// </summary>
    public string RedisConnectionString =>
        _redis?.GetConnectionString()
        ?? throw new InvalidOperationException("Redis container is not configured or has not been started.");

    /// <summary>
    /// Gets the Kafka bootstrap servers address. Only available after <see cref="StartAsync"/> completes
    /// and when Kafka was enabled via the builder.
    /// </summary>
    public string KafkaBootstrapServers =>
        _kafka?.GetBootstrapAddress()
        ?? throw new InvalidOperationException("Kafka container is not configured or has not been started.");

    /// <summary>
    /// Gets the RabbitMQ AMQP connection string. Only available after <see cref="StartAsync"/> completes
    /// and when RabbitMQ was enabled via the builder.
    /// </summary>
    public string RabbitMqConnectionString =>
        _rabbitMq?.GetConnectionString()
        ?? throw new InvalidOperationException("RabbitMQ container is not configured or has not been started.");

    /// <summary>
    /// Starts all configured containers in parallel.
    /// </summary>
    /// <param name="ct">A <see cref="CancellationToken"/> to observe while waiting for startup.</param>
    /// <returns>A <see cref="Task"/> that completes when all containers are running.</returns>
    public async Task StartAsync(CancellationToken ct = default)
    {
        if (_started)
        {
            return;
        }

        var tasks = new List<Task>();

        if (_usePostgres)
        {
            _postgres = new PostgreSqlBuilder().Build();
            tasks.Add(_postgres.StartAsync(ct));
        }

        if (_useRedis)
        {
            _redis = new RedisBuilder().Build();
            tasks.Add(_redis.StartAsync(ct));
        }

        if (_useKafka)
        {
            _kafka = new KafkaBuilder().Build();
            tasks.Add(_kafka.StartAsync(ct));
        }

        if (_useRabbitMq)
        {
            _rabbitMq = new RabbitMqBuilder().Build();
            tasks.Add(_rabbitMq.StartAsync(ct));
        }

        await Task.WhenAll(tasks).ConfigureAwait(false);
        _started = true;
    }

    /// <summary>
    /// Disposes all running containers asynchronously.
    /// </summary>
    /// <returns>A <see cref="ValueTask"/> that completes when all containers have been disposed.</returns>
    public async ValueTask DisposeAsync()
    {
        var tasks = new List<Task>();

        if (_postgres is not null)
        {
            tasks.Add(_postgres.DisposeAsync().AsTask());
        }

        if (_redis is not null)
        {
            tasks.Add(_redis.DisposeAsync().AsTask());
        }

        if (_kafka is not null)
        {
            tasks.Add(_kafka.DisposeAsync().AsTask());
        }

        if (_rabbitMq is not null)
        {
            tasks.Add(_rabbitMq.DisposeAsync().AsTask());
        }

        await Task.WhenAll(tasks).ConfigureAwait(false);
    }
}
