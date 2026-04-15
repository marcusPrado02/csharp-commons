namespace MarcusPrado.Platform.IntegrationTestEnvironment;

/// <summary>
/// Fluent builder for configuring and creating a <see cref="PlatformTestEnvironment"/>.
/// </summary>
public sealed class PlatformTestEnvironmentBuilder
{
    private bool _usePostgres;
    private bool _useRedis;
    private bool _useKafka;
    private bool _useRabbitMq;

    /// <summary>
    /// Enables a Postgres container in the test environment.
    /// </summary>
    /// <returns>The same builder instance for chaining.</returns>
    public PlatformTestEnvironmentBuilder WithPostgres()
    {
        _usePostgres = true;
        return this;
    }

    /// <summary>
    /// Enables a Redis container in the test environment.
    /// </summary>
    /// <returns>The same builder instance for chaining.</returns>
    public PlatformTestEnvironmentBuilder WithRedis()
    {
        _useRedis = true;
        return this;
    }

    /// <summary>
    /// Enables a Kafka container in the test environment.
    /// </summary>
    /// <returns>The same builder instance for chaining.</returns>
    public PlatformTestEnvironmentBuilder WithKafka()
    {
        _useKafka = true;
        return this;
    }

    /// <summary>
    /// Enables a RabbitMQ container in the test environment.
    /// </summary>
    /// <returns>The same builder instance for chaining.</returns>
    public PlatformTestEnvironmentBuilder WithRabbitMq()
    {
        _useRabbitMq = true;
        return this;
    }

    /// <summary>
    /// Builds and starts a <see cref="PlatformTestEnvironment"/> with the configured containers.
    /// </summary>
    /// <param name="ct">A <see cref="CancellationToken"/> to observe while waiting for startup.</param>
    /// <returns>A started <see cref="PlatformTestEnvironment"/>.</returns>
    public async Task<PlatformTestEnvironment> BuildAsync(CancellationToken ct = default)
    {
        var env = new PlatformTestEnvironment(_usePostgres, _useRedis, _useKafka, _useRabbitMq);

        await env.StartAsync(ct).ConfigureAwait(false);

        return env;
    }

    /// <summary>
    /// Builds a <see cref="PlatformTestEnvironment"/> with the configured containers without starting it.
    /// Useful for testing builder configuration independently.
    /// </summary>
    /// <returns>An unstarted <see cref="PlatformTestEnvironment"/>.</returns>
    public PlatformTestEnvironment Build() => new(_usePostgres, _useRedis, _useKafka, _useRabbitMq);
}
