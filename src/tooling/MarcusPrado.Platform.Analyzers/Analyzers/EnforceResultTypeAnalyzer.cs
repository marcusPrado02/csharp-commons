using System.Collections.Immutable;
using System.Linq;
using MarcusPrado.Platform.Analyzers.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace MarcusPrado.Platform.Analyzers.Analyzers;

/// <summary>
/// PLATFORM004 — Reports a warning when a public method in the application layer returns
/// <c>void</c> or bare <c>Task</c> instead of <c>Result&lt;T&gt;</c> or <c>Task&lt;Result&lt;T&gt;&gt;</c>.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class EnforceResultTypeAnalyzer : DiagnosticAnalyzer
{
    /// <summary>The <see cref="DiagnosticDescriptor"/> for PLATFORM004.</summary>
    public static readonly DiagnosticDescriptor Descriptor = PlatformDiagnosticDescriptors.EnforceResultType;

    private static readonly string[] _applicationLayerKeywords = new[] { "Application", "Commands", "Handlers" };

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Descriptor);

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeMethodDeclaration, SyntaxKind.MethodDeclaration);
    }

    private static void AnalyzeMethodDeclaration(SyntaxNodeAnalysisContext context)
    {
        var methodDecl = (MethodDeclarationSyntax)context.Node;

        // Only public methods
        if (!methodDecl.Modifiers.Any(SyntaxKind.PublicKeyword))
        {
            return;
        }

        // Only in application layer namespaces
        if (!IsInsideApplicationContext(methodDecl))
        {
            return;
        }

        var returnTypeName = methodDecl.ReturnType.ToString().Trim();

        // Flag "void" and bare "Task" (no generic argument)
        var isBareVoid = returnTypeName == "void";
        var isBareTask = returnTypeName == "Task" || returnTypeName == "System.Threading.Tasks.Task";

        if (!isBareVoid && !isBareTask)
        {
            return;
        }

        context.ReportDiagnostic(
            Diagnostic.Create(
                Descriptor,
                methodDecl.Identifier.GetLocation(),
                methodDecl.Identifier.Text,
                returnTypeName
            )
        );
    }

    private static bool IsInsideApplicationContext(SyntaxNode node)
    {
        var syntaxRoot = node.SyntaxTree.GetRoot();

        var hasBlockNs = syntaxRoot
            .DescendantNodesAndSelf()
            .OfType<BaseNamespaceDeclarationSyntax>()
            .Any(ns => _applicationLayerKeywords.Any(kw => ns.Name.ToString().Contains(kw)));

        if (hasBlockNs)
        {
            return true;
        }

        return syntaxRoot
            .DescendantNodesAndSelf()
            .OfType<FileScopedNamespaceDeclarationSyntax>()
            .Any(fs => _applicationLayerKeywords.Any(kw => fs.Name.ToString().Contains(kw)));
    }
}
