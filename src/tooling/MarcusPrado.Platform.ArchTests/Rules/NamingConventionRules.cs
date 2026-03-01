using MarcusPrado.Platform.Application.CQRS;
using MarcusPrado.Platform.Application.Errors;
using MarcusPrado.Platform.Domain.SeedWork;

namespace MarcusPrado.Platform.ArchTests.Rules;

/// <summary>
/// Architecture tests that enforce naming conventions across all platform
/// assemblies.
///
/// Conventions enforced:
///   • Public interfaces start with "I".
///   • Exception classes end with "Exception".
///   • Command handler classes end with "Handler".
///   • Query handler classes end with "Handler".
///   • ICommand implementations end with "Command".
///   • IQuery implementations end with "Query".
/// </summary>
public sealed class NamingConventionRules
{
    // ── Interfaces ────────────────────────────────────────────────────────────

    [Fact]
    public void PublicInterfaces_ShouldHaveNameStartingWith_I()
    {
        var violations = KnownAssemblies.AllCore
            .SelectMany(a => a.GetExportedTypes())
            .Where(t => t.IsInterface && !t.Name.StartsWith("I", StringComparison.Ordinal))
            .Select(t => $"{t.Assembly.GetName().Name} → {t.FullName}")
            .OrderBy(n => n, StringComparer.Ordinal)
            .ToList();

        violations.Should().BeEmpty(
            because: "all public interfaces in the platform must follow the 'I' prefix "
            + "convention (e.g. ICommandBus, IRepository<T>)");
    }

    // ── Exceptions ────────────────────────────────────────────────────────────

    [Fact]
    public void ExceptionClasses_ShouldHaveNameEndingWith_Exception()
    {
        var violations = KnownAssemblies.AllCore
            .SelectMany(a => a.GetExportedTypes())
            .Where(t =>
                t.IsClass &&
                typeof(Exception).IsAssignableFrom(t) &&
                !t.Name.EndsWith("Exception", StringComparison.Ordinal))
            .Select(t => $"{t.Assembly.GetName().Name} → {t.FullName}")
            .OrderBy(n => n, StringComparer.Ordinal)
            .ToList();

        violations.Should().BeEmpty(
            because: "all exception classes must end with 'Exception' for discoverability");
    }

    [Fact]
    public void Application_ExceptionClasses_ShouldInheritAppException()
    {
        var violations = KnownAssemblies.Application
            .GetExportedTypes()
            .Where(t =>
                t.IsClass &&
                !t.IsAbstract &&
                t.Name.EndsWith("Exception", StringComparison.Ordinal) &&
                !typeof(AppException).IsAssignableFrom(t))
            .Select(t => t.FullName!)
            .ToList();

        violations.Should().BeEmpty(
            because: "all concrete exception classes in Application must inherit AppException (APP-07)");
    }

    // ── CQRS ─────────────────────────────────────────────────────────────────

    [Fact]
    public void CommandHandlerClasses_ShouldHaveNameEndingWith_Handler()
    {
        var violations = KnownAssemblies.Application
            .GetExportedTypes()
            .Where(t =>
                t.IsClass && !t.IsAbstract &&
                typeof(ICommandHandler).IsAssignableFrom(t))
            .Where(t => !t.Name.EndsWith("Handler", StringComparison.Ordinal))
            .Select(t => t.FullName!)
            .ToList();

        violations.Should().BeEmpty(
            because: "all ICommandHandler implementations must have names ending in 'Handler'");
    }

    [Fact]
    public void QueryHandlerClasses_ShouldHaveNameEndingWith_Handler()
    {
        var violations = KnownAssemblies.Application
            .GetExportedTypes()
            .Where(t =>
                t.IsClass && !t.IsAbstract &&
                typeof(IQueryHandler).IsAssignableFrom(t))
            .Where(t => !t.Name.EndsWith("Handler", StringComparison.Ordinal))
            .Select(t => t.FullName!)
            .ToList();

        violations.Should().BeEmpty(
            because: "all IQueryHandler implementations must have names ending in 'Handler'");
    }

    [Fact]
    public void CommandClasses_ShouldHaveNameEndingWith_Command()
    {
        var violations = KnownAssemblies.Application
            .GetExportedTypes()
            .Where(t =>
                t.IsClass && !t.IsAbstract &&
                typeof(ICommand).IsAssignableFrom(t) &&
                !t.Name.EndsWith("Command", StringComparison.Ordinal))
            .Select(t => t.FullName!)
            .ToList();

        violations.Should().BeEmpty(
            because: "all ICommand implementations must have names ending in 'Command'");
    }

    [Fact]
    public void QueryClasses_ShouldHaveNameEndingWith_Query()
    {
        var violations = KnownAssemblies.Application
            .GetExportedTypes()
            .Where(t =>
                t.IsClass && !t.IsAbstract &&
                typeof(IQuery).IsAssignableFrom(t) &&
                !t.Name.EndsWith("Query", StringComparison.Ordinal))
            .Select(t => t.FullName!)
            .ToList();

        violations.Should().BeEmpty(
            because: "all IQuery implementations must have names ending in 'Query'");
    }
}
