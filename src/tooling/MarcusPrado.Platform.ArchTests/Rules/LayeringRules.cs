using System.Reflection;
using NetArchTest.Rules;

namespace MarcusPrado.Platform.ArchTests.Rules;

/// <summary>
/// Architecture tests that enforce the layer dependency graph.
///
/// Allowed dependency directions (→ = "may depend on"):
///   Abstractions → nothing
///   Domain       → Abstractions
///   Application  → Domain, Abstractions
///   Extensions   → Core (never other Extensions)
///   Kits         → Core, Extensions
///   Samples      → anything
///
/// See docs/architecture/layer-rules.md for the full rule set.
/// </summary>
public sealed class LayeringRules
{
    // ── Abstractions ─────────────────────────────────────────────

    [Fact]
    public void Abstractions_ShouldHaveNoPlatformDependencies()
    {
        var platformRefs = GetPlatformReferences(KnownAssemblies.Abstractions);

        platformRefs.Should().BeEmpty(
            because: "Abstractions is the foundational layer; it must not "
            + "depend on any other MarcusPrado.Platform.* assembly (ABS-01)");
    }

    [Fact]
    public void Abstractions_ShouldNotDependOnEfCore()
    {
        var result = Types.InAssembly(KnownAssemblies.Abstractions)
            .ShouldNot()
            .HaveDependencyOn(KnownAssemblies.ForbiddenInCore.EfCore)
            .GetResult();

        AssertSuccess(result, "Abstractions must not reference EF Core (ABS-03).");
    }

    [Fact]
    public void Abstractions_ShouldNotDependOnAspNetCore()
    {
        var result = Types.InAssembly(KnownAssemblies.Abstractions)
            .ShouldNot()
            .HaveDependencyOn(KnownAssemblies.ForbiddenInCore.AspNetCore)
            .GetResult();

        AssertSuccess(result, "Abstractions must not reference ASP.NET Core (ABS-03).");
    }

    // ── Domain ─────────────────────────────────────────────────

    [Fact]
    public void Domain_ShouldOnlyDependOnAbstractions_WithinPlatform()
    {
        var allowedPlatformRefs = new HashSet<string>(StringComparer.Ordinal)
        {
            "MarcusPrado.Platform.Abstractions",
        };

        var platformRefs = GetPlatformReferences(KnownAssemblies.Domain);
        var violations = platformRefs.Except(allowedPlatformRefs).ToList();

        violations.Should().BeEmpty(
            because: $"Domain may only depend on Abstractions within the platform (DOM-01). "
            + $"Unexpected references: [{string.Join(", ", violations)}]");
    }

    [Fact]
    public void Domain_ShouldNotDependOnApplication()
    {
        var result = Types.InAssembly(KnownAssemblies.Domain)
            .ShouldNot()
            .HaveDependencyOn("MarcusPrado.Platform.Application")
            .GetResult();

        AssertSuccess(result, "Domain must not depend on Application (DOM-01).");
    }

    // ── Application ────────────────────────────────────────────

    [Fact]
    public void Application_ShouldNotDependOnExtensions()
    {
        var extensionPrefixes = new[]
        {
            "MarcusPrado.Platform.AspNetCore",
            "MarcusPrado.Platform.EfCore",
            "MarcusPrado.Platform.Postgres",
            "MarcusPrado.Platform.Redis",
            "MarcusPrado.Platform.Kafka",
            "MarcusPrado.Platform.RabbitMq",
            "MarcusPrado.Platform.OpenTelemetry",
            "MarcusPrado.Platform.Serilog",
            "MarcusPrado.Platform.HealthChecks",
        };

        var appRefs = GetAllReferences(KnownAssemblies.Application);
        var violations = appRefs
            .Where(r => extensionPrefixes.Any(p =>
                r.StartsWith(p, StringComparison.Ordinal)))
            .ToList();

        violations.Should().BeEmpty(
            because: "Application must not reference Extensions (APP-02). "
            + $"Violations: [{string.Join(", ", violations)}]");
    }

    [Fact]
    public void Application_ShouldNotDependOnEfCore()
    {
        var result = Types.InAssembly(KnownAssemblies.Application)
            .ShouldNot()
            .HaveDependencyOn(KnownAssemblies.ForbiddenInCore.EfCore)
            .GetResult();

        AssertSuccess(result, "Application must not reference EF Core (APP-03).");
    }

    [Fact]
    public void Application_ShouldNotDependOnAspNetCore()
    {
        var result = Types.InAssembly(KnownAssemblies.Application)
            .ShouldNot()
            .HaveDependencyOn(KnownAssemblies.ForbiddenInCore.AspNetCore)
            .GetResult();

        AssertSuccess(result, "Application must not reference ASP.NET Core (APP-03).");
    }

    // ── Contracts ──────────────────────────────────────────────

    [Fact]
    public void Contracts_ShouldNotDependOnEfCore()
    {
        var result = Types.InAssembly(KnownAssemblies.Contracts)
            .ShouldNot()
            .HaveDependencyOn(KnownAssemblies.ForbiddenInCore.EfCore)
            .GetResult();

        AssertSuccess(result, "Contracts must not reference EF Core.");
    }

    [Fact]
    public void Contracts_ShouldNotDependOnAspNetCore()
    {
        var result = Types.InAssembly(KnownAssemblies.Contracts)
            .ShouldNot()
            .HaveDependencyOn(KnownAssemblies.ForbiddenInCore.AspNetCore)
            .GetResult();

        AssertSuccess(result, "Contracts must not reference ASP.NET Core.");
    }

    // ── Helpers ────────────────────────────────────────────────────────

    private static List<string> GetPlatformReferences(Assembly assembly) =>
        assembly.GetReferencedAssemblies()
            .Select(r => r.Name!)
            .Where(n => n.StartsWith("MarcusPrado.Platform.", StringComparison.Ordinal))
            .OrderBy(n => n, StringComparer.Ordinal)
            .ToList();

    private static List<string> GetAllReferences(Assembly assembly) =>
        assembly.GetReferencedAssemblies()
            .Select(r => r.Name!)
            .OrderBy(n => n, StringComparer.Ordinal)
            .ToList();

    private static void AssertSuccess(TestResult result, string reason)
    {
        var failing = string.Join(", ", result.FailingTypeNames ?? []);
        result.IsSuccessful.Should().BeTrue(
            because: $"{reason} Failing types: [{failing}]");
    }
}
