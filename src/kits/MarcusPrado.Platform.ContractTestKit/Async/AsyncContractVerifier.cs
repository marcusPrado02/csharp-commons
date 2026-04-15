using System.Text.Json;
using MarcusPrado.Platform.ContractTestKit.Pact;

namespace MarcusPrado.Platform.ContractTestKit.Async;

/// <summary>
/// Verifies message/event contracts by validating the schema of an <see cref="EventContractEnvelope"/>
/// payload against a JSON schema document.
/// </summary>
public static class AsyncContractVerifier
{
    /// <summary>
    /// Verifies that the payload in <paramref name="envelope"/> satisfies all required properties
    /// defined in <paramref name="schemaDocument"/>.
    /// </summary>
    /// <param name="envelope">The event contract envelope containing the payload to verify.</param>
    /// <param name="schemaDocument">
    /// A <see cref="JsonDocument"/> whose root object's <c>required</c> array lists the property
    /// names that must be present and non-null in the payload, and whose <c>properties</c> object
    /// optionally describes the expected <c>type</c> of each property.
    /// </param>
    /// <returns>
    /// A <see cref="ContractVerificationResult"/> describing whether the payload matches the schema.
    /// </returns>
    public static ContractVerificationResult Verify(EventContractEnvelope envelope, JsonDocument schemaDocument)
    {
        ArgumentNullException.ThrowIfNull(envelope);
        ArgumentNullException.ThrowIfNull(schemaDocument);

        var interactionLabel = $"{envelope.ProducerId}:{envelope.EventType}";

        try
        {
            return VerifyInternal(envelope, schemaDocument, interactionLabel);
        }
        catch (KeyNotFoundException ex)
        {
            return new ContractVerificationResult(interactionLabel, false, ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return new ContractVerificationResult(interactionLabel, false, ex.Message);
        }
    }

    private static ContractVerificationResult VerifyInternal(
        EventContractEnvelope envelope,
        JsonDocument schemaDocument,
        string interactionLabel)
    {
        var schema = schemaDocument.RootElement;
        var payload = envelope.Payload;

        if (payload.ValueKind != JsonValueKind.Object)
        {
            return new ContractVerificationResult(
                interactionLabel,
                false,
                $"Payload must be a JSON object but was '{payload.ValueKind}'.");
        }

        // Validate required properties
        if (schema.TryGetProperty("required", out var required) &&
            required.ValueKind == JsonValueKind.Array)
        {
            foreach (var reqProp in required.EnumerateArray())
            {
                var propName = reqProp.GetString();
                if (propName is null)
                    continue;

                if (!payload.TryGetProperty(propName, out var propValue) ||
                    propValue.ValueKind == JsonValueKind.Null ||
                    propValue.ValueKind == JsonValueKind.Undefined)
                {
                    return new ContractVerificationResult(
                        interactionLabel,
                        false,
                        $"Required property '{propName}' is missing or null in the payload.");
                }
            }
        }

        // Validate types declared under "properties"
        if (schema.TryGetProperty("properties", out var properties) &&
            properties.ValueKind == JsonValueKind.Object)
        {
            foreach (var schemaProp in properties.EnumerateObject())
            {
                if (!payload.TryGetProperty(schemaProp.Name, out var payloadProp))
                    continue; // Not present; required check handles mandatory fields

                if (!schemaProp.Value.TryGetProperty("type", out var typeElement))
                    continue;

                var expectedType = typeElement.GetString();
                var error = ValidateType(schemaProp.Name, payloadProp, expectedType);
                if (error is not null)
                    return new ContractVerificationResult(interactionLabel, false, error);
            }
        }

        return new ContractVerificationResult(interactionLabel, true, null);
    }

    private static string? ValidateType(string propName, JsonElement value, string? expectedType)
    {
        if (expectedType is null)
            return null;

        var actualKind = value.ValueKind;

        var valid = expectedType switch
        {
            "string" => actualKind == JsonValueKind.String,
            "number" => actualKind == JsonValueKind.Number,
            "integer" => actualKind == JsonValueKind.Number && IsInteger(value),
            "boolean" => actualKind is JsonValueKind.True or JsonValueKind.False,
            "object" => actualKind == JsonValueKind.Object,
            "array" => actualKind == JsonValueKind.Array,
            "null" => actualKind == JsonValueKind.Null,
            _ => true
        };

        return valid
            ? null
            : $"Property '{propName}' expected type '{expectedType}' but got '{actualKind}'.";
    }

    private static bool IsInteger(JsonElement element)
    {
        if (element.TryGetInt64(out _))
            return true;
        var raw = element.GetRawText();
        return !raw.Contains('.') && !raw.Contains('e') && !raw.Contains('E');
    }
}
