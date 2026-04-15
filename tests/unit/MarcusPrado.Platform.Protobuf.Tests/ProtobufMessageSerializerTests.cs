namespace MarcusPrado.Platform.Protobuf.Tests;

[ProtoContract]
public sealed class SampleMessage
{
    [ProtoMember(1)]
    public int Id { get; set; }

    [ProtoMember(2)]
    public string Name { get; set; } = string.Empty;

    [ProtoMember(3)]
    public bool Active { get; set; }
}

public sealed class ProtobufMessageSerializerTests
{
    private readonly ProtobufMessageSerializer _sut = new();

    [Fact]
    public void Serialize_ThenDeserialize_RoundTrips()
    {
        var msg = new SampleMessage
        {
            Id = 42,
            Name = "hello",
            Active = true,
        };
        var data = _sut.Serialize(msg);
        var result = _sut.Deserialize<SampleMessage>(data);
        result!.Id.Should().Be(42);
        result.Name.Should().Be("hello");
        result.Active.Should().BeTrue();
    }

    [Fact]
    public void Serialize_ReturnsBase64String()
    {
        var msg = new SampleMessage { Id = 1, Name = "x" };
        var data = _sut.Serialize(msg);
        var act = () => Convert.FromBase64String(data);
        act.Should().NotThrow("serialised output should be valid Base64");
    }

    [Fact]
    public void Serialize_ProducesNonEmptyString()
    {
        var data = _sut.Serialize(new SampleMessage { Id = 1, Name = "x" });
        data.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Serialize_Null_ThrowsArgumentNullException()
    {
        var act = () => _sut.Serialize<SampleMessage>(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Deserialize_NullData_ThrowsArgumentNullException()
    {
        var act = () => _sut.Deserialize<SampleMessage>(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void RoundTrip_DefaultValues_OK()
    {
        var msg = new SampleMessage();
        var data = _sut.Serialize(msg);
        var result = _sut.Deserialize<SampleMessage>(data);
        result!.Id.Should().Be(0);
        result.Name.Should().Be(string.Empty);
    }

    [Fact]
    public void MultipleSerializations_ProduceDeterministicOutput()
    {
        var msg = new SampleMessage { Id = 99, Name = "det" };
        var data1 = _sut.Serialize(msg);
        var data2 = _sut.Serialize(msg);
        data1.Should().Be(data2);
    }

    [Fact]
    public void Serialize_SmallMessage_Base64SmallerThanJson()
    {
        var msg = new SampleMessage
        {
            Id = 1,
            Name = "test",
            Active = true,
        };
        var pbBase64 = _sut.Serialize(msg);
        var pbBytes = Convert.FromBase64String(pbBase64);
        var jsonBytes = System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(msg);
        pbBytes.Length.Should().BeLessThan(jsonBytes.Length);
    }
}
