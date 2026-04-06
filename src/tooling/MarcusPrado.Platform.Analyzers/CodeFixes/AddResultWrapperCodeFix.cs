using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using MarcusPrado.Platform.Analyzers.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MarcusPrado.Platform.Analyzers.CodeFixes;

/// <summary>
/// Code fix provider for PLATFORM004 (<see cref="PlatformDiagnosticDescriptors.EnforceResultType"/>).
/// Wraps the return type of a <c>void</c> or bare <c>Task</c> method with <c>Task&lt;Result&lt;Unit&gt;&gt;</c>.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(AddResultWrapperCodeFix))]
[Shared]
public sealed class AddResultWrapperCodeFix : CodeFixProvider
{
    /// <inheritdoc />
    public override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create(PlatformDiagnosticDescriptors.EnforceResultType.Id);

    /// <inheritdoc />
    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    /// <inheritdoc />
    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root == null)
        {
            return;
        }

        foreach (var diagnostic in context.Diagnostics)
        {
            var diagnosticSpan = diagnostic.Location.SourceSpan;
            var token = root.FindToken(diagnosticSpan.Start);
            var methodDecl = token.Parent?.AncestorsAndSelf().OfType<MethodDeclarationSyntax>().FirstOrDefault();

            if (methodDecl == null)
            {
                continue;
            }

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: "Wrap return type with Task<Result<Unit>>",
                    createChangedDocument: ct => WrapReturnTypeAsync(context.Document, methodDecl, ct),
                    equivalenceKey: "AddResultWrapper"),
                diagnostic);
        }
    }

    private static async Task<Document> WrapReturnTypeAsync(
        Document document,
        MethodDeclarationSyntax methodDecl,
        CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (root == null)
        {
            return document;
        }

        var newReturnType = SyntaxFactory
            .ParseTypeName("Task<Result<Unit>>")
            .WithTriviaFrom(methodDecl.ReturnType);

        // Remove async modifier if present on void methods (they become Task-returning)
        var newMethod = methodDecl.WithReturnType(newReturnType);

        var newRoot = root.ReplaceNode(methodDecl, newMethod);
        return document.WithSyntaxRoot(newRoot);
    }
}
