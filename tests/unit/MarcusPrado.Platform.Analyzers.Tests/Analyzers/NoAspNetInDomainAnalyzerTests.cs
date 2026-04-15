using FluentAssertions;
using MarcusPrado.Platform.Analyzers.Analyzers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace MarcusPrado.Platform.Analyzers.Tests.Analyzers;

/// <summary>
/// Tests for <see cref="NoAspNetInDomainAnalyzer"/> (PLATFORM002).
/// </summary>
public sealed class NoAspNetInDomainAnalyzerTests
{
    [Fact]
    public void Descriptor_HasCorrectId()
    {
        NoAspNetInDomainAnalyzer.Descriptor.Id.Should().Be("PLATFORM002");
    }

    [Fact]
    public void Descriptor_HasWarningDefaultSeverity()
    {
        NoAspNetInDomainAnalyzer.Descriptor.DefaultSeverity.Should().Be(DiagnosticSeverity.Warning);
    }

    [Fact]
    public void Descriptor_IsEnabledByDefault()
    {
        NoAspNetInDomainAnalyzer.Descriptor.IsEnabledByDefault.Should().BeTrue();
    }

    [Fact]
    public void Analyzer_HasDiagnosticAnalyzerAttribute()
    {
        var attrs = typeof(NoAspNetInDomainAnalyzer).GetCustomAttributes(typeof(DiagnosticAnalyzerAttribute), false);

        attrs.Should().NotBeEmpty();
    }

    [Fact]
    public void Analyzer_SupportedDiagnostics_ContainsPlatform002()
    {
        var analyzer = new NoAspNetInDomainAnalyzer();
        analyzer.SupportedDiagnostics.Should().ContainSingle(d => d.Id == "PLATFORM002");
    }

    [Fact]
    public void Descriptor_Category_IsArchitecture()
    {
        NoAspNetInDomainAnalyzer.Descriptor.Category.Should().Be("Architecture");
    }
}
