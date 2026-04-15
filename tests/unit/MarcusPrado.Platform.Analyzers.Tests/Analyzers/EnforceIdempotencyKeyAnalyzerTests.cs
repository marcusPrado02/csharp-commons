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
/// Tests for <see cref="EnforceIdempotencyKeyAnalyzer"/> (PLATFORM005).
/// </summary>
public sealed class EnforceIdempotencyKeyAnalyzerTests
{
    [Fact]
    public void Descriptor_HasCorrectId()
    {
        EnforceIdempotencyKeyAnalyzer.Descriptor.Id.Should().Be("PLATFORM005");
    }

    [Fact]
    public void Descriptor_HasWarningDefaultSeverity()
    {
        EnforceIdempotencyKeyAnalyzer.Descriptor.DefaultSeverity.Should().Be(DiagnosticSeverity.Warning);
    }

    [Fact]
    public void Descriptor_IsEnabledByDefault()
    {
        EnforceIdempotencyKeyAnalyzer.Descriptor.IsEnabledByDefault.Should().BeTrue();
    }

    [Fact]
    public void Analyzer_HasDiagnosticAnalyzerAttribute()
    {
        var attrs = typeof(EnforceIdempotencyKeyAnalyzer).GetCustomAttributes(
            typeof(DiagnosticAnalyzerAttribute),
            false
        );

        attrs.Should().NotBeEmpty();
    }

    [Fact]
    public void Analyzer_SupportedDiagnostics_ContainsPlatform005()
    {
        var analyzer = new EnforceIdempotencyKeyAnalyzer();
        analyzer.SupportedDiagnostics.Should().ContainSingle(d => d.Id == "PLATFORM005");
    }

    [Fact]
    public void Descriptor_Category_IsDesign()
    {
        EnforceIdempotencyKeyAnalyzer.Descriptor.Category.Should().Be("Design");
    }

    /// <summary>
    /// Positive test: a Command class without IdempotencyKey should trigger PLATFORM005.
    /// </summary>
    [Fact]
    public async Task CommandClassWithoutIdempotencyKey_ProducesDiagnostic()
    {
        var source = """
            namespace MyApp.Application.Orders
            {
                public class CreateOrderCommand
                {
                    public string OrderId { get; set; } = string.Empty;
                }
            }
            """;

        var diagnostics = await GetDiagnosticsAsync(source);
        diagnostics.Should().Contain(d => d.Id == "PLATFORM005");
    }

    /// <summary>
    /// Negative test: a Command class WITH IdempotencyKey should NOT trigger PLATFORM005.
    /// </summary>
    [Fact]
    public async Task CommandClassWithIdempotencyKey_NoDiagnostic()
    {
        var source = """
            namespace MyApp.Application.Orders
            {
                public class CreateOrderCommand
                {
                    public string OrderId { get; set; } = string.Empty;
                    public string IdempotencyKey { get; set; } = string.Empty;
                }
            }
            """;

        var diagnostics = await GetDiagnosticsAsync(source);
        diagnostics.Should().NotContain(d => d.Id == "PLATFORM005");
    }

    /// <summary>
    /// Negative test: a class NOT ending in "Command" should NOT trigger PLATFORM005.
    /// </summary>
    [Fact]
    public async Task NonCommandClass_NoDiagnostic()
    {
        var source = """
            namespace MyApp.Application.Orders
            {
                public class OrderHandler
                {
                    public string OrderId { get; set; } = string.Empty;
                }
            }
            """;

        var diagnostics = await GetDiagnosticsAsync(source);
        diagnostics.Should().NotContain(d => d.Id == "PLATFORM005");
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

        var analyzer = new EnforceIdempotencyKeyAnalyzer();
        var compilationWithAnalyzers = compilation.WithAnalyzers(
            System.Collections.Immutable.ImmutableArray.Create<DiagnosticAnalyzer>(analyzer)
        );

        var allDiagnostics = await compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync();
        return allDiagnostics;
    }
}
