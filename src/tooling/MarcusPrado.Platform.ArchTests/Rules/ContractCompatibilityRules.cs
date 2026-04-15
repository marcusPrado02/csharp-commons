using System.Reflection;
using MarcusPrado.Platform.Contracts.Async;
using MarcusPrado.Platform.Contracts.Http;
using NetArchTest.Rules;

namespace MarcusPrado.Platform.ArchTests.Rules;

/// <summary>
/// Architecture tests that enforce the public API and event contract conventions.
///
/// Rules enforced:
///   • Event contracts (name ends with "Event") implement <see cref="IEventContract"/>.
///   • Types marked <see cref="ApiContractAttribute"/> are public.
///   • Types marked <see cref="ApiContractAttribute"/> reside in a
///     "*.Contracts.*" namespace.
///   • [ApiContract] classes must not be sealed.
/// </summary>
public sealed class ContractCompatibilityRules
{
    // ── Event contracts ──────────────────────────────────────────────

    [Fact]
    public void EventContracts_ShouldImplement_IEventContract()
    {
        // Only check concrete (non-abstract) event classes in the Contracts assembly.
        // DomainEvent in the Domain assembly is an abstract domain primitive and is
        // intentionally excluded — it is not a messaging contract.
        var violations = KnownAssemblies
            .Contracts.GetExportedTypes()
            .Where(t =>
                t.IsClass
                && !t.IsAbstract
                && t.Name.EndsWith("Event", StringComparison.Ordinal)
                && !typeof(IEventContract).IsAssignableFrom(t)
            )
            .Select(t => $"{t.Assembly.GetName().Name} → {t.FullName}")
            .OrderBy(n => n, StringComparer.Ordinal)
            .ToList();

        violations
            .Should()
            .BeEmpty(
                because: "all concrete event classes in the Contracts assembly must implement "
                    + "IEventContract to participate in the platform's schema-versioning pipeline"
            );
    }

    // ── [ApiContract] types ─────────────────────────────────────────────

    [Fact]
    public void ApiContractTypes_ShouldBePublic()
    {
        var violations = KnownAssemblies
            .AllCore.SelectMany(a => a.GetTypes())
            .Where(t => t.GetCustomAttribute<ApiContractAttribute>() is not null)
            .Where(t => !t.IsPublic && !t.IsNestedPublic)
            .Select(t => t.FullName!)
            .ToList();

        violations
            .Should()
            .BeEmpty(
                because: "[ApiContract] must only be applied to public types "
                    + "(internal types are invisible to consumers)"
            );
    }

    [Fact]
    public void ApiContractTypes_ShouldResideInContractsNamespace()
    {
        var violations = KnownAssemblies
            .AllCore.SelectMany(a => a.GetExportedTypes())
            .Where(t => t.GetCustomAttribute<ApiContractAttribute>() is not null)
            .Where(t => !ContainsContractsSegment(t.Namespace ?? string.Empty))
            .Select(t => $"{t.Namespace} → {t.FullName}")
            .OrderBy(n => n, StringComparer.Ordinal)
            .ToList();

        violations
            .Should()
            .BeEmpty(
                because: "[ApiContract] types must reside in a namespace containing "
                    + "'Contracts' for discoverability"
            );
    }

    [Fact]
    public void ApiContractTypes_ShouldNotBeSealed()
    {
        var violations = KnownAssemblies
            .AllCore.SelectMany(a => a.GetExportedTypes())
            .Where(t => t.GetCustomAttribute<ApiContractAttribute>() is not null)
            .Where(t => t.IsSealed && !t.IsValueType)
            .Select(t => t.FullName!)
            .ToList();

        violations
            .Should()
            .BeEmpty(
                because: "[ApiContract] classes must not be sealed; minor-version " + "additions extend via inheritance"
            );
    }

    // ── NetArchTest infrastructure check ───────────────────────────────────

    [Fact]
    public void Contracts_ShouldNotHaveDependencyOnInfrastructure()
    {
        foreach (
            var forbidden in new[]
            {
                KnownAssemblies.ForbiddenInCore.EfCore,
                KnownAssemblies.ForbiddenInCore.Kafka,
                KnownAssemblies.ForbiddenInCore.RabbitMq,
            }
        )
        {
            var result = Types
                .InAssembly(KnownAssemblies.Contracts)
                .ShouldNot()
                .HaveDependencyOn(forbidden)
                .GetResult();

            var failing = string.Join(", ", result.FailingTypeNames ?? []);
            result
                .IsSuccessful.Should()
                .BeTrue(because: $"Contracts must not reference {forbidden}. Failing: [{failing}]");
        }
    }

    // ── Helpers ────────────────────────────────────────────────────────

    private static bool ContainsContractsSegment(string namespaceName) =>
        namespaceName.Split('.').Any(seg => seg.Equals("Contracts", StringComparison.Ordinal));
}
