using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using MarcusPrado.Platform.Analyzers.Analyzers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace MarcusPrado.Platform.Analyzers.Tests.Analyzers;

/// <summary>
/// Tests for <see cref="EnforceResultTypeAnalyzer"/> (PLATFORM004).
/// </summary>
public sealed class EnforceResultTypeAnalyzerTests
{
    [Fact]
    public void Descriptor_HasCorrectId()
    {
        EnforceResultTypeAnalyzer.Descriptor.Id.Should().Be("PLATFORM004");
    }

    [Fact]
    public void Descriptor_HasWarningDefaultSeverity()
    {
        EnforceResultTypeAnalyzer.Descriptor.DefaultSeverity.Should().Be(DiagnosticSeverity.Warning);
    }

    [Fact]
    public void Descriptor_IsEnabledByDefault()
    {
        EnforceResultTypeAnalyzer.Descriptor.IsEnabledByDefault.Should().BeTrue();
    }

    [Fact]
    public void Analyzer_HasDiagnosticAnalyzerAttribute()
    {
        var attrs = typeof(EnforceResultTypeAnalyzer).GetCustomAttributes(typeof(DiagnosticAnalyzerAttribute), false);

        attrs.Should().NotBeEmpty();
    }

    [Fact]
    public void Analyzer_SupportedDiagnostics_ContainsPlatform004()
    {
        var analyzer = new EnforceResultTypeAnalyzer();
        analyzer.SupportedDiagnostics.Should().ContainSingle(d => d.Id == "PLATFORM004");
    }

    [Fact]
    public void Descriptor_Category_IsDesign()
    {
        EnforceResultTypeAnalyzer.Descriptor.Category.Should().Be("Design");
    }

    /// <summary>
    /// Positive test: a public void method inside an Application namespace should trigger PLATFORM004.
    /// </summary>
    [Fact]
    public async Task PublicVoidMethod_InApplicationNamespace_ProducesDiagnostic()
    {
        var source = """
            namespace MyApp.Application.Orders
            {
                public class OrderCommandHandler
                {
                    public void Handle(object command) { }
                }
            }
            """;

        var diagnostics = await GetDiagnosticsAsync(source);
        diagnostics.Should().Contain(d => d.Id == "PLATFORM004");
    }

    /// <summary>
    /// Negative test: a public method returning Task&lt;Result&lt;int&gt;&gt; should NOT trigger PLATFORM004.
    /// </summary>
    [Fact]
    public async Task PublicMethodReturningTaskOfResult_InApplicationNamespace_NoDiagnostic()
    {
        var source = """
            using System.Threading.Tasks;
            namespace MyApp.Application.Orders
            {
                public class Result<T> { }
                public class OrderCommandHandler
                {
                    public Task<Result<int>> HandleAsync(object command) => default!;
                }
            }
            """;

        var diagnostics = await GetDiagnosticsAsync(source);
        diagnostics.Should().NotContain(d => d.Id == "PLATFORM004");
    }

    /// <summary>
    /// Negative test: a public void method outside application namespaces should NOT trigger PLATFORM004.
    /// </summary>
    [Fact]
    public async Task PublicVoidMethod_OutsideApplicationNamespace_NoDiagnostic()
    {
        var source = """
            namespace MyApp.Infrastructure
            {
                public class SomeService
                {
                    public void DoWork() { }
                }
            }
            """;

        var diagnostics = await GetDiagnosticsAsync(source);
        diagnostics.Should().NotContain(d => d.Id == "PLATFORM004");
    }

    private static async Task<IEnumerable<Diagnostic>> GetDiagnosticsAsync(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source);
        var compilation = CSharpCompilation.Create(
            "TestAssembly",
            new[] { syntaxTree },
            new[] { MetadataReference.CreateFromFile(typeof(object).Assembly.Location) },
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
        );

        var analyzer = new EnforceResultTypeAnalyzer();
        var compilationWithAnalyzers = compilation.WithAnalyzers(
            System.Collections.Immutable.ImmutableArray.Create<DiagnosticAnalyzer>(analyzer)
        );

        var allDiagnostics = await compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync();
        return allDiagnostics;
    }
}
