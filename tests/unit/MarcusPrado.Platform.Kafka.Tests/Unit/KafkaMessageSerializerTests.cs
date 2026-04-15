namespace MarcusPrado.Platform.Kafka.Tests.Unit;

public sealed class KafkaMessageSerializerTests
{
    private readonly KafkaMessageSerializer _serializer = new();

    [Fact]
    public void Serialize_Deserialize_RoundTrip_ReturnsOriginalPayload()
    {
        var envelope = new MessageEnvelope<SampleMessage> { Payload = new SampleMessage { Text = "hello kafka" } };

        var json = _serializer.Serialize(envelope);
        var result = _serializer.Deserialize<MessageEnvelope<SampleMessage>>(json);

        result.Should().NotBeNull();
        result!.Payload.Text.Should().Be("hello kafka");
    }

    [Fact]
    public void Serialize_Deserialize_PreservesMetadataMessageId()
    {
        var meta = new MessageMetadata { MessageId = "msg-abc-123" };
        var envelope = new MessageEnvelope<SampleMessage>
        {
            Metadata = meta,
            Payload = new SampleMessage { Text = "test" },
        };

        var json = _serializer.Serialize(envelope);
        var result = _serializer.Deserialize<MessageEnvelope<SampleMessage>>(json);

        result!.Metadata.MessageId.Should().Be("msg-abc-123");
    }

    [Fact]
    public void Serialize_ProducesValidJson()
    {
        var envelope = new MessageEnvelope<SampleMessage> { Payload = new SampleMessage { Text = "json-check" } };

        var json = _serializer.Serialize(envelope);

        json.Should().Contain("\"payload\"");
        json.Should().Contain("\"metadata\"");
        json.Should().Contain("json-check");
    }

    [Fact]
    public void Deserialize_InvalidJson_ThrowsJsonException()
    {
        var act = () => _serializer.Deserialize<MessageEnvelope<SampleMessage>>("not-valid-json");

        act.Should().Throw<System.Text.Json.JsonException>();
    }

    private sealed record SampleMessage
    {
        public string Text { get; init; } = string.Empty;
    }
}
