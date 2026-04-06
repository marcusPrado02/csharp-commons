using System.Collections.Immutable;
using System.Linq;
using MarcusPrado.Platform.Analyzers.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace MarcusPrado.Platform.Analyzers.Analyzers;

/// <summary>
/// PLATFORM003 — Reports a warning when infrastructure namespaces (EF Core, Npgsql) are referenced
/// inside a Domain namespace.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class DomainNoInfraReferenceAnalyzer : DiagnosticAnalyzer
{
    /// <summary>The <see cref="DiagnosticDescriptor"/> for PLATFORM003.</summary>
    public static readonly DiagnosticDescriptor Descriptor = PlatformDiagnosticDescriptors.DomainNoInfraReference;

    private static readonly string[] InfraNamespacePrefixes = new[]
    {
        "Microsoft.EntityFrameworkCore",
        "Npgsql",
    };

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(Descriptor);

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeUsingDirective, SyntaxKind.UsingDirective);
    }

    private static void AnalyzeUsingDirective(SyntaxNodeAnalysisContext context)
    {
        var usingDirective = (UsingDirectiveSyntax)context.Node;
        var namespaceName = usingDirective.Name?.ToString();

        if (namespaceName == null)
        {
            return;
        }

        var isInfra = InfraNamespacePrefixes.Any(p =>
            namespaceName.StartsWith(p, System.StringComparison.Ordinal));

        if (!isInfra)
        {
            return;
        }

        if (!IsInsideDomainContext(usingDirective))
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(
            Descriptor,
            usingDirective.GetLocation(),
            namespaceName));
    }

    private static bool IsInsideDomainContext(SyntaxNode node)
    {
        var syntaxRoot = node.SyntaxTree.GetRoot();

        var hasBlockNs = syntaxRoot
            .DescendantNodesAndSelf()
            .OfType<BaseNamespaceDeclarationSyntax>()
            .Any(ns => ns.Name.ToString().Contains("Domain"));

        if (hasBlockNs)
        {
            return true;
        }

        return syntaxRoot
            .DescendantNodesAndSelf()
            .OfType<FileScopedNamespaceDeclarationSyntax>()
            .Any(fs => fs.Name.ToString().Contains("Domain"));
    }
}
