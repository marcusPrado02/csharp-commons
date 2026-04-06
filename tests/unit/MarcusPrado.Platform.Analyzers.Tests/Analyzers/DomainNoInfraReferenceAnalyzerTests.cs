using FluentAssertions;
using MarcusPrado.Platform.Analyzers.Analyzers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace MarcusPrado.Platform.Analyzers.Tests.Analyzers;

/// <summary>
/// Tests for <see cref="DomainNoInfraReferenceAnalyzer"/> (PLATFORM003).
/// </summary>
public sealed class DomainNoInfraReferenceAnalyzerTests
{
    [Fact]
    public void Descriptor_HasCorrectId()
    {
        DomainNoInfraReferenceAnalyzer.Descriptor.Id.Should().Be("PLATFORM003");
    }

    [Fact]
    public void Descriptor_HasWarningDefaultSeverity()
    {
        DomainNoInfraReferenceAnalyzer.Descriptor.DefaultSeverity.Should().Be(DiagnosticSeverity.Warning);
    }

    [Fact]
    public void Descriptor_IsEnabledByDefault()
    {
        DomainNoInfraReferenceAnalyzer.Descriptor.IsEnabledByDefault.Should().BeTrue();
    }

    [Fact]
    public void Analyzer_HasDiagnosticAnalyzerAttribute()
    {
        var attrs = typeof(DomainNoInfraReferenceAnalyzer)
            .GetCustomAttributes(typeof(DiagnosticAnalyzerAttribute), false);

        attrs.Should().NotBeEmpty();
    }

    [Fact]
    public void Analyzer_SupportedDiagnostics_ContainsPlatform003()
    {
        var analyzer = new DomainNoInfraReferenceAnalyzer();
        analyzer.SupportedDiagnostics.Should().ContainSingle(d => d.Id == "PLATFORM003");
    }

    [Fact]
    public void Descriptor_Category_IsArchitecture()
    {
        DomainNoInfraReferenceAnalyzer.Descriptor.Category.Should().Be("Architecture");
    }
}
