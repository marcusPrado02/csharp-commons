using System.Text.Json;

namespace MarcusPrado.Platform.ContractTestKit.Async;

/// <summary>
/// Represents a message/event contract envelope used for async contract verification.
/// </summary>
/// <param name="EventType">The fully-qualified type name of the event.</param>
/// <param name="ProducerId">The identifier of the service or component that produces this event.</param>
/// <param name="Payload">The JSON payload of the event.</param>
public record EventContractEnvelope(string EventType, string ProducerId, JsonElement Payload);
