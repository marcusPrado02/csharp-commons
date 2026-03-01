using System.Text.Json;
using System.Text.Json.Serialization;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using MessagePack;
using Newtonsoft.Json;
using ProtoBuf;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace MarcusPrado.Platform.Benchmarks;

[JsonSerializable(typeof(MessageSerializerBenchmark.OrderPlacedEvent))]
internal partial class AppJsonContext : JsonSerializerContext { }

/// <summary>
/// Compares serializer throughput and allocation for a realistic platform
/// message envelope (matches <c>MessageEnvelope</c> in
/// <c>MarcusPrado.Platform.Messaging</c>).
///
/// Serializers compared:
///   - System.Text.Json (BCL, UTF-8, source-gen)
///   - Newtonsoft.Json  (reflection-based, JSON)
///   - MessagePack       (binary, attribute-based)
///   - protobuf-net      (binary, attribute-based)
///
/// Each benchmark is run for both serialization (object → bytes) and
/// deserialization (bytes → object).
/// </summary>
[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
public partial class MessageSerializerBenchmark
{
    // ── Sample payload ───────────────────────────────────────────────────────

    [MessagePackObject]
    [ProtoContract]
    public sealed class OrderPlacedEvent
    {
        [Key(0)] [property: ProtoMember(1)] public Guid OrderId { get; set; }
        [Key(1)] [property: ProtoMember(2)] public string CustomerId { get; set; } = default!;
        [Key(2)] [property: ProtoMember(3)] public decimal Amount { get; set; }
        [Key(3)] [property: ProtoMember(4)] public DateTime PlacedAt { get; set; }
        [Key(4)] [property: ProtoMember(5)] public string CurrencyCode { get; set; } = default!;
        [Key(5)] [property: ProtoMember(6)] public List<string> ProductIds { get; set; } = default!;
    }

    // ── Pre-baked instances and byte arrays ──────────────────────────────────

    private OrderPlacedEvent _payload = default!;

    private byte[] _systemTextJsonBytes = default!;
    private byte[] _newtonsoftBytes = default!;
    private byte[] _messagePackBytes = default!;
    private byte[] _protobufBytes = default!;

    private static readonly JsonSerializerOptions StjOptions = new()
    {
        TypeInfoResolver = AppJsonContext.Default,
    };

    [GlobalSetup]
    public void Setup()
    {
        _payload = new OrderPlacedEvent
        {
            OrderId = Guid.NewGuid(),
            CustomerId = "cust-12345",
            Amount = 199.99m,
            PlacedAt = DateTime.UtcNow,
            CurrencyCode = "USD",
            ProductIds = ["prod-001", "prod-002", "prod-003"],
        };

        _systemTextJsonBytes = JsonSerializer.SerializeToUtf8Bytes(_payload, StjOptions);
        _newtonsoftBytes = System.Text.Encoding.UTF8.GetBytes(
            JsonConvert.SerializeObject(_payload));
        _messagePackBytes = MessagePackSerializer.Serialize(_payload);

        using var ms = new MemoryStream();
        Serializer.Serialize(ms, _payload);
        _protobufBytes = ms.ToArray();
    }

    // ── Serialization ────────────────────────────────────────────────────────

    [Benchmark(Baseline = true, Description = "STJ serialize (source-gen)")]
    public byte[] Serialize_SystemTextJson() =>
        JsonSerializer.SerializeToUtf8Bytes(_payload, StjOptions);

    [Benchmark(Description = "Newtonsoft serialize")]
    public byte[] Serialize_Newtonsoft() =>
        System.Text.Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(_payload));

    [Benchmark(Description = "MessagePack serialize")]
    public byte[] Serialize_MessagePack() =>
        MessagePackSerializer.Serialize(_payload);

    [Benchmark(Description = "protobuf-net serialize")]
    public byte[] Serialize_Protobuf()
    {
        using var ms = new MemoryStream();
        Serializer.Serialize(ms, _payload);
        return ms.ToArray();
    }

    // ── Deserialization ──────────────────────────────────────────────────────

    [Benchmark(Description = "STJ deserialize (source-gen)")]
    public OrderPlacedEvent? Deserialize_SystemTextJson() =>
        JsonSerializer.Deserialize(_systemTextJsonBytes, AppJsonContext.Default.OrderPlacedEvent);

    [Benchmark(Description = "Newtonsoft deserialize")]
    public OrderPlacedEvent? Deserialize_Newtonsoft() =>
        JsonConvert.DeserializeObject<OrderPlacedEvent>(
            System.Text.Encoding.UTF8.GetString(_newtonsoftBytes));

    [Benchmark(Description = "MessagePack deserialize")]
    public OrderPlacedEvent Deserialize_MessagePack() =>
        MessagePackSerializer.Deserialize<OrderPlacedEvent>(_messagePackBytes);

    [Benchmark(Description = "protobuf-net deserialize")]
    public OrderPlacedEvent Deserialize_Protobuf()
    {
        using var ms = new MemoryStream(_protobufBytes);
        return Serializer.Deserialize<OrderPlacedEvent>(ms);
    }
}
