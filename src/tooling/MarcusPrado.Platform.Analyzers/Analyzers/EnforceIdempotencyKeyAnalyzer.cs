using System.Collections.Immutable;
using System.Linq;
using MarcusPrado.Platform.Analyzers.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace MarcusPrado.Platform.Analyzers.Analyzers;

/// <summary>
/// PLATFORM005 — Reports a warning when a class ending in "Command" does not expose
/// an <c>IdempotencyKey</c> property.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class EnforceIdempotencyKeyAnalyzer : DiagnosticAnalyzer
{
    /// <summary>The <see cref="DiagnosticDescriptor"/> for PLATFORM005.</summary>
    public static readonly DiagnosticDescriptor Descriptor = PlatformDiagnosticDescriptors.EnforceIdempotencyKey;

    private const string CommandSuffix = "Command";
    private const string IdempotencyKeyPropertyName = "IdempotencyKey";

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Descriptor);

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeClassDeclaration, SyntaxKind.ClassDeclaration);
    }

    private static void AnalyzeClassDeclaration(SyntaxNodeAnalysisContext context)
    {
        var classDecl = (ClassDeclarationSyntax)context.Node;
        var className = classDecl.Identifier.Text;

        // Only classes whose name ends with "Command"
        if (!className.EndsWith(CommandSuffix, System.StringComparison.Ordinal))
        {
            return;
        }

        // Check if the class (directly) declares a property named "IdempotencyKey"
        var hasIdempotencyKey = classDecl
            .Members.OfType<PropertyDeclarationSyntax>()
            .Any(p => p.Identifier.Text == IdempotencyKeyPropertyName);

        if (hasIdempotencyKey)
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(Descriptor, classDecl.Identifier.GetLocation(), className));
    }
}
