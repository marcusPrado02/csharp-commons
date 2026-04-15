using MarcusPrado.Platform.Domain.SeedWork;
using NetArchTest.Rules;

namespace MarcusPrado.Platform.ArchTests.Rules;

/// <summary>
/// Architecture tests that guard the Domain assembly against prohibited
/// dependencies and enforce the domain exception hierarchy.
///
/// Golden rules:
///   • Domain must not reference EF Core, ASP.NET Core, or any messaging broker SDK.
///   • All exception classes in Domain must inherit <see cref="DomainException"/>.
/// </summary>
public sealed class DomainDependencyRules
{
    // ── Infrastructure dependency guards ────────────────────────────────────

    [Fact]
    public void Domain_ShouldNotDependOnEfCore()
    {
        var result = Types
            .InAssembly(KnownAssemblies.Domain)
            .ShouldNot()
            .HaveDependencyOn(KnownAssemblies.ForbiddenInCore.EfCore)
            .GetResult();

        AssertSuccess(result, "Domain must not reference EF Core.");
    }

    [Fact]
    public void Domain_ShouldNotDependOnAspNetCore()
    {
        var result = Types
            .InAssembly(KnownAssemblies.Domain)
            .ShouldNot()
            .HaveDependencyOn(KnownAssemblies.ForbiddenInCore.AspNetCore)
            .GetResult();

        AssertSuccess(result, "Domain must not reference ASP.NET Core.");
    }

    [Fact]
    public void Domain_ShouldNotDependOnKafka()
    {
        var result = Types
            .InAssembly(KnownAssemblies.Domain)
            .ShouldNot()
            .HaveDependencyOn(KnownAssemblies.ForbiddenInCore.Kafka)
            .GetResult();

        AssertSuccess(result, "Domain must not reference Confluent.Kafka.");
    }

    [Fact]
    public void Domain_ShouldNotDependOnRabbitMq()
    {
        var result = Types
            .InAssembly(KnownAssemblies.Domain)
            .ShouldNot()
            .HaveDependencyOn(KnownAssemblies.ForbiddenInCore.RabbitMq)
            .GetResult();

        AssertSuccess(result, "Domain must not reference RabbitMQ.Client.");
    }

    [Fact]
    public void Domain_ShouldNotDependOnSerilog()
    {
        var result = Types
            .InAssembly(KnownAssemblies.Domain)
            .ShouldNot()
            .HaveDependencyOn(KnownAssemblies.ForbiddenInCore.Serilog)
            .GetResult();

        AssertSuccess(result, "Domain must not reference Serilog.");
    }

    [Fact]
    public void Domain_ShouldNotDependOnOpenTelemetry()
    {
        var result = Types
            .InAssembly(KnownAssemblies.Domain)
            .ShouldNot()
            .HaveDependencyOn(KnownAssemblies.ForbiddenInCore.OpenTelemetry)
            .GetResult();

        AssertSuccess(result, "Domain must not reference OpenTelemetry SDK.");
    }

    // ── Exception hierarchy ───────────────────────────────────────────

    [Fact]
    public void Domain_NonAbstractExceptionClasses_ShouldInheritDomainException()
    {
        // Use reflection for full inheritance chain (IsAssignableFrom walks the hierarchy)
        var violations = KnownAssemblies
            .Domain.GetExportedTypes()
            .Where(t =>
                t.IsClass
                && !t.IsAbstract
                && t.Name.EndsWith("Exception", StringComparison.Ordinal)
                && !typeof(DomainException).IsAssignableFrom(t)
            )
            .Select(t => t.FullName!)
            .ToList();

        violations
            .Should()
            .BeEmpty(
                because: "all concrete domain exception classes must inherit DomainException "
                    + "(see ADR-003 — DomainException itself is abstract)"
            );
    }

    [Fact]
    public void Domain_AbstractExceptionBase_ShouldInheritSystemException()
    {
        typeof(DomainException)
            .Should()
            .BeDerivedFrom<Exception>(because: "DomainException is the root of the domain exception hierarchy");
    }

    // ── IRepository usage ────────────────────────────────────────────

    [Fact]
    public void Domain_ShouldNotInstantiateConcreteRepositories()
    {
        var result = Types
            .InAssembly(KnownAssemblies.Domain)
            .ShouldNot()
            .HaveDependencyOn("MarcusPrado.Platform.EfCore")
            .GetResult();

        AssertSuccess(result, "Domain must not reference the EfCore extension.");
    }

    // ── Helpers ────────────────────────────────────────────────────────

    private static void AssertSuccess(TestResult result, string reason)
    {
        var failing = string.Join(", ", result.FailingTypeNames ?? []);
        result.IsSuccessful.Should().BeTrue(because: $"{reason} Failing types: [{failing}]");
    }
}
