namespace MarcusPrado.Platform.Security.Pii;

/// <summary>Marks a property or field as containing Personally Identifiable Information (PII).</summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public sealed class PiiDataAttribute : Attribute
{
    /// <summary>Type of PII for classification purposes.</summary>
    public PiiType Type { get; init; } = PiiType.Generic;
}

public enum PiiType
{
    Generic,
    Email,
    Phone,
    TaxId,
    Name,
    Address,
    DateOfBirth,
}
