using System.Reflection;
using MarcusPrado.Platform.Abstractions.Errors;
using MarcusPrado.Platform.Application.CQRS;
using MarcusPrado.Platform.Contracts.Async;
using MarcusPrado.Platform.Domain.SeedWork;

namespace MarcusPrado.Platform.ArchTests;

/// <summary>
/// Provides the set of platform assemblies under architecture test.
/// Each assembly is loaded via a well-known anchor type to ensure the
/// correct build output is referenced.
/// </summary>
internal static class KnownAssemblies
{
    /// <summary>
    /// <c>MarcusPrado.Platform.Abstractions</c> — pure BCL; no platform
    /// dependencies allowed.
    /// </summary>
    public static readonly Assembly Abstractions = typeof(Error).Assembly;

    /// <summary>
    /// <c>MarcusPrado.Platform.Domain</c> — may only depend on Abstractions.
    /// </summary>
    public static readonly Assembly Domain = typeof(DomainException).Assembly;

    /// <summary>
    /// <c>MarcusPrado.Platform.Application</c> — may depend on Domain +
    /// Abstractions; must not depend on Extensions.
    /// </summary>
    public static readonly Assembly Application = typeof(ICommand).Assembly;

    /// <summary>
    /// <c>MarcusPrado.Platform.Contracts</c> — public API/event contracts.
    /// </summary>
    public static readonly Assembly Contracts = typeof(IEventContract).Assembly;

    /// <summary>
    /// All core assemblies combined (for cross-cutting naming checks).
    /// </summary>
    public static readonly IReadOnlyList<Assembly> AllCore = [Abstractions, Domain, Application, Contracts];

    /// <summary>
    /// Names of all platform assemblies, used to filter out BCL references
    /// when asserting on inter-platform dependencies.
    /// </summary>
    public static readonly IReadOnlySet<string> PlatformAssemblyNames = AllCore
        .Select(a => a.GetName().Name!)
        .ToHashSet(StringComparer.Ordinal);

    /// <summary>
    /// Returns the NuGet package / assembly names that represent known
    /// infrastructure packages which are forbidden in Core projects.
    /// </summary>
    public static class ForbiddenInCore
    {
        public const string EfCore = "Microsoft.EntityFrameworkCore";
        public const string EfCoreRelational = "Microsoft.EntityFrameworkCore.Relational";
        public const string AspNetCore = "Microsoft.AspNetCore";
        public const string Kafka = "Confluent.Kafka";
        public const string RabbitMq = "RabbitMQ.Client";
        public const string Serilog = "Serilog";
        public const string OpenTelemetry = "OpenTelemetry";
    }
}
