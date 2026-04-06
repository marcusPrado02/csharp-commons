using Microsoft.CodeAnalysis;

namespace MarcusPrado.Platform.Analyzers.Diagnostics;

/// <summary>
/// Contains all <see cref="DiagnosticDescriptor"/> instances for the Platform analyzers.
/// </summary>
public static class PlatformDiagnosticDescriptors
{
    private const string ArchitectureCategory = "Architecture";
    private const string DesignCategory = "Design";

    /// <summary>PLATFORM001 — EF Core types must not be used in Domain projects.</summary>
    public static readonly DiagnosticDescriptor NoEfCoreInDomain = new DiagnosticDescriptor(
        id: "PLATFORM001",
        title: "EF Core must not be used in Domain projects",
        messageFormat: "Using '{0}' from 'Microsoft.EntityFrameworkCore' is not allowed inside a Domain namespace",
        category: ArchitectureCategory,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Domain projects must remain persistence-ignorant. Remove any EF Core references from domain code.");

    /// <summary>PLATFORM002 — ASP.NET Core types must not be used in Domain projects.</summary>
    public static readonly DiagnosticDescriptor NoAspNetInDomain = new DiagnosticDescriptor(
        id: "PLATFORM002",
        title: "ASP.NET Core must not be used in Domain projects",
        messageFormat: "Using '{0}' from 'Microsoft.AspNetCore' is not allowed inside a Domain namespace",
        category: ArchitectureCategory,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Domain projects must remain framework-agnostic. Remove any ASP.NET Core references from domain code.");

    /// <summary>PLATFORM003 — Domain assemblies must not reference infrastructure namespaces.</summary>
    public static readonly DiagnosticDescriptor DomainNoInfraReference = new DiagnosticDescriptor(
        id: "PLATFORM003",
        title: "Domain must not reference infrastructure namespaces",
        messageFormat: "Using '{0}' (an infrastructure namespace) is not allowed inside a Domain namespace",
        category: ArchitectureCategory,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Domain assemblies must not depend on infrastructure concerns such as EF Core or Npgsql.");

    /// <summary>PLATFORM004 — Public methods in the application layer should return Result or Task of Result.</summary>
    public static readonly DiagnosticDescriptor EnforceResultType = new DiagnosticDescriptor(
        id: "PLATFORM004",
        title: "Application layer methods should return Result<T> or Task<Result<T>>",
        messageFormat: "Method '{0}' returns '{1}' — consider returning Result<T> or Task<Result<T>> to make failures explicit",
        category: DesignCategory,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Public methods in application layer classes should return Result<T> or Task<Result<T>> instead of void or bare Task.");

    /// <summary>PLATFORM005 — Command types must have an IdempotencyKey property.</summary>
    public static readonly DiagnosticDescriptor EnforceIdempotencyKey = new DiagnosticDescriptor(
        id: "PLATFORM005",
        title: "Command types must have an IdempotencyKey property",
        messageFormat: "Command class '{0}' does not have an 'IdempotencyKey' property",
        category: DesignCategory,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Command handler inputs must expose an IdempotencyKey property to enable safe retry semantics.");
}
