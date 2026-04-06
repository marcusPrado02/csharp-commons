namespace MarcusPrado.Platform.Nats.Publisher;

/// <summary>NATS-backed <see cref="INatsPublisher"/> that serializes payloads as JSON.</summary>
public sealed class NatsPublisher : INatsPublisher
{
    private static readonly JsonSerializerOptions SerializerOptions =
        new(JsonSerializerDefaults.Web);

    private readonly INatsConnection _connection;

    /// <summary>Initialises the publisher with an injected NATS connection.</summary>
    /// <param name="connection">The NATS connection to use for publishing.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="connection"/> is <see langword="null"/>.
    /// </exception>
    public NatsPublisher(INatsConnection connection)
    {
        ArgumentNullException.ThrowIfNull(connection);
        _connection = connection;
    }

    /// <inheritdoc/>
    public async Task PublishAsync<T>(string subject, T message, CancellationToken ct = default)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(message);
        ArgumentException.ThrowIfNullOrWhiteSpace(subject);

        var json = JsonSerializer.Serialize(message, SerializerOptions);
        await _connection.PublishAsync(subject, json, cancellationToken: ct).ConfigureAwait(false);
    }
}
