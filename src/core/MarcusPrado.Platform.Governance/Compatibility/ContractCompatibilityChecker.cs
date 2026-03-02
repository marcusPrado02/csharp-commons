using System.Text.Json;

namespace MarcusPrado.Platform.Governance.Compatibility;

/// <summary>
/// Compares two JSON schema strings and produces a <see cref="CompatibilityReport"/>
/// that lists breaking changes (field removals, type changes).
/// </summary>
/// <remarks>
/// The checker treats the comparison schema as a flat object where each property
/// represents a top-level field with a string representation of its JSON type as
/// value, e.g. <c>{ "orderId": "string", "amount": "number" }</c>.
/// </remarks>
public static class ContractCompatibilityChecker
{
    /// <summary>
    /// Checks whether <paramref name="currentSchemaJson"/> is backward-compatible
    /// with <paramref name="previousSchemaJson"/>.
    /// </summary>
    /// <param name="previousSchemaJson">The older schema (baseline).</param>
    /// <param name="currentSchemaJson">The newer schema to validate.</param>
    /// <returns>A <see cref="CompatibilityReport"/> with any detected violations.</returns>
    public static CompatibilityReport Check(string previousSchemaJson, string currentSchemaJson)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(previousSchemaJson);
        ArgumentException.ThrowIfNullOrWhiteSpace(currentSchemaJson);

        using var previous = JsonDocument.Parse(previousSchemaJson);
        using var current = JsonDocument.Parse(currentSchemaJson);

        var violations = new List<CompatibilityViolation>();

        if (previous.RootElement.ValueKind != JsonValueKind.Object
            || current.RootElement.ValueKind != JsonValueKind.Object)
        {
            return CompatibilityReport.Compatible();
        }

        foreach (var prevField in previous.RootElement.EnumerateObject())
        {
            if (!current.RootElement.TryGetProperty(prevField.Name, out var currValue))
            {
                violations.Add(new CompatibilityViolation(
                    ViolationType.FieldRemoved,
                    prevField.Name,
                    $"Field '{prevField.Name}' was removed in the new schema."));

                continue;
            }

            var prevType = prevField.Value.ValueKind == JsonValueKind.String
                ? prevField.Value.GetString() ?? prevField.Value.ValueKind.ToString()
                : prevField.Value.ValueKind.ToString();
            var currType = currValue.ValueKind == JsonValueKind.String
                ? currValue.GetString() ?? currValue.ValueKind.ToString()
                : currValue.ValueKind.ToString();

            if (!string.Equals(prevType, currType, StringComparison.Ordinal))
            {
                violations.Add(new CompatibilityViolation(
                    ViolationType.TypeChanged,
                    prevField.Name,
                    $"Field '{prevField.Name}' type changed from '{prevType}' to '{currType}'."));
            }
        }

        return new CompatibilityReport(violations);
    }
}
