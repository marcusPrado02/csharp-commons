namespace MarcusPrado.Platform.Contracts.Http;

/// <summary>
/// Marks a type as a public API contract. Types decorated with this attribute
/// are subject to backward-compatibility enforcement by architecture tests:
/// properties must not be removed between versions (only added).
///
/// Apply to DTOs, request/response types, and event contracts that are part
/// of a published API surface.
/// </summary>
/// <example>
/// <code>
/// [ApiContract]
/// public sealed record CreateOrderRequest(Guid CustomerId, decimal Amount);
/// </code>
/// </example>
[AttributeUsage(
    AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Struct,
    Inherited = true,
    AllowMultiple = false)]
public sealed class ApiContractAttribute : Attribute { }
