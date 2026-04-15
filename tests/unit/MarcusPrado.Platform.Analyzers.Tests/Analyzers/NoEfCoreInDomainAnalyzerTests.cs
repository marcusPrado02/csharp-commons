using FluentAssertions;
using MarcusPrado.Platform.Analyzers.Analyzers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace MarcusPrado.Platform.Analyzers.Tests.Analyzers;

/// <summary>
/// Tests for <see cref="NoEfCoreInDomainAnalyzer"/> (PLATFORM001).
/// </summary>
public sealed class NoEfCoreInDomainAnalyzerTests
{
    [Fact]
    public void Descriptor_HasCorrectId()
    {
        NoEfCoreInDomainAnalyzer.Descriptor.Id.Should().Be("PLATFORM001");
    }

    [Fact]
    public void Descriptor_HasWarningDefaultSeverity()
    {
        NoEfCoreInDomainAnalyzer.Descriptor.DefaultSeverity.Should().Be(DiagnosticSeverity.Warning);
    }

    [Fact]
    public void Descriptor_IsEnabledByDefault()
    {
        NoEfCoreInDomainAnalyzer.Descriptor.IsEnabledByDefault.Should().BeTrue();
    }

    [Fact]
    public void Analyzer_HasDiagnosticAnalyzerAttribute()
    {
        var attrs = typeof(NoEfCoreInDomainAnalyzer).GetCustomAttributes(typeof(DiagnosticAnalyzerAttribute), false);

        attrs.Should().NotBeEmpty();
    }

    [Fact]
    public void Analyzer_SupportedDiagnostics_ContainsPlatform001()
    {
        var analyzer = new NoEfCoreInDomainAnalyzer();
        analyzer.SupportedDiagnostics.Should().ContainSingle(d => d.Id == "PLATFORM001");
    }

    [Fact]
    public void Descriptor_Category_IsArchitecture()
    {
        NoEfCoreInDomainAnalyzer.Descriptor.Category.Should().Be("Architecture");
    }
}
