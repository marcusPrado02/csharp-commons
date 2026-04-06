// <copyright file="ApiChangelogTests.cs" company="MarcusPrado">
// Copyright (c) MarcusPrado. All rights reserved.
// </copyright>

using System.Reflection;
using FluentAssertions;
using MarcusPrado.Platform.ApiChangelog;
using Xunit;

namespace MarcusPrado.Platform.ApiChangelog.Tests;

public sealed class ApiChangelogTests
{
    // ── ApiSurfaceExtractor ────────────────────────────────────────────────────

    [Fact]
    public void ApiSurfaceExtractor_ExtractsPublicTypes()
    {
        // Arrange – use the library assembly under test
        var assembly = typeof(ApiSurfaceExtractor).Assembly;

        // Act
        var surface = ApiSurfaceExtractor.Extract(assembly);

        // Assert
        surface.Types.Should().NotBeEmpty();
        surface.Types.Select(t => t.FullName).Should()
            .Contain("MarcusPrado.Platform.ApiChangelog.ApiSurface");
        surface.Types.Select(t => t.FullName).Should()
            .Contain("MarcusPrado.Platform.ApiChangelog.ApiDiffEngine");
        surface.Types.Select(t => t.FullName).Should()
            .Contain("MarcusPrado.Platform.ApiChangelog.ChangelogRenderer");
    }

    [Fact]
    public void ApiSurfaceExtractor_IgnoresPrivateTypes()
    {
        // Arrange
        var assembly = typeof(ApiSurfaceExtractor).Assembly;

        // Act
        var surface = ApiSurfaceExtractor.Extract(assembly);

        // Assert – no type name should indicate a private/internal helper
        surface.Types.Should().AllSatisfy(t =>
        {
            var type = assembly.GetType(t.FullName);
            type.Should().NotBeNull();
            type!.IsPublic.Should().BeTrue(because: $"{t.FullName} must be public to appear in the surface");
        });
    }

    [Fact]
    public void ApiSurfaceExtractor_ThrowsOnNullAssembly()
    {
        var act = () => ApiSurfaceExtractor.Extract(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    // ── ApiDiffEngine ──────────────────────────────────────────────────────────

    [Fact]
    public void ApiDiffEngine_NoChanges_ReturnsEmptyDiff()
    {
        // Arrange
        var surface = BuildSurface("MyLib", "1.0.0",
        [
            new ApiType("MyLib.Foo", "class",
            [
                new ApiMember("DoThing", "public void DoThing(string s)", "method"),
            ]),
        ]);

        // Act
        var diff = ApiDiffEngine.Compare(surface, surface);

        // Assert
        diff.AddedTypes.Should().BeEmpty();
        diff.RemovedTypes.Should().BeEmpty();
        diff.AddedMembers.Should().BeEmpty();
        diff.RemovedMembers.Should().BeEmpty();
        diff.HasBreakingChanges.Should().BeFalse();
    }

    [Fact]
    public void ApiDiffEngine_AddedType_ReportedAsAddition()
    {
        // Arrange
        var baseline = BuildSurface("MyLib", "1.0.0",
        [
            new ApiType("MyLib.Foo", "class", []),
        ]);

        var current = BuildSurface("MyLib", "1.1.0",
        [
            new ApiType("MyLib.Foo", "class", []),
            new ApiType("MyLib.Bar", "class", []),
        ]);

        // Act
        var diff = ApiDiffEngine.Compare(baseline, current);

        // Assert
        diff.AddedTypes.Should().ContainSingle().Which.Should().Be("MyLib.Bar");
        diff.RemovedTypes.Should().BeEmpty();
        diff.HasBreakingChanges.Should().BeFalse();
    }

    [Fact]
    public void ApiDiffEngine_RemovedType_ReportedAsBreaking()
    {
        // Arrange
        var baseline = BuildSurface("MyLib", "1.0.0",
        [
            new ApiType("MyLib.Foo", "class", []),
            new ApiType("MyLib.Bar", "class", []),
        ]);

        var current = BuildSurface("MyLib", "2.0.0",
        [
            new ApiType("MyLib.Foo", "class", []),
        ]);

        // Act
        var diff = ApiDiffEngine.Compare(baseline, current);

        // Assert
        diff.RemovedTypes.Should().ContainSingle().Which.Should().Be("MyLib.Bar");
        diff.AddedTypes.Should().BeEmpty();
        diff.HasBreakingChanges.Should().BeTrue();
    }

    [Fact]
    public void ApiDiffEngine_AddedMember_ReportedAsAddition()
    {
        // Arrange
        var baseline = BuildSurface("MyLib", "1.0.0",
        [
            new ApiType("MyLib.Foo", "class", []),
        ]);

        var current = BuildSurface("MyLib", "1.1.0",
        [
            new ApiType("MyLib.Foo", "class",
            [
                new ApiMember("NewMethod", "public void NewMethod()", "method"),
            ]),
        ]);

        // Act
        var diff = ApiDiffEngine.Compare(baseline, current);

        // Assert
        diff.AddedMembers.Should().ContainSingle()
            .Which.MemberSignature.Should().Be("public void NewMethod()");
        diff.RemovedMembers.Should().BeEmpty();
        diff.HasBreakingChanges.Should().BeFalse();
    }

    [Fact]
    public void ApiDiffEngine_RemovedMember_ReportedAsBreaking()
    {
        // Arrange
        var baseline = BuildSurface("MyLib", "1.0.0",
        [
            new ApiType("MyLib.Foo", "class",
            [
                new ApiMember("DoThing", "public void DoThing(string s)", "method"),
            ]),
        ]);

        var current = BuildSurface("MyLib", "2.0.0",
        [
            new ApiType("MyLib.Foo", "class", []),
        ]);

        // Act
        var diff = ApiDiffEngine.Compare(baseline, current);

        // Assert
        diff.RemovedMembers.Should().ContainSingle()
            .Which.MemberSignature.Should().Be("public void DoThing(string s)");
        diff.HasBreakingChanges.Should().BeTrue();
    }

    // ── ChangelogRenderer ──────────────────────────────────────────────────────

    [Fact]
    public void ChangelogRenderer_WithBreakingChanges_ContainsBreakingSection()
    {
        // Arrange
        var diff = new ApiDiff(
            AddedTypes: [],
            RemovedTypes: ["MyLib.Foo"],
            AddedMembers: [],
            RemovedMembers: [new ApiMemberDiff("MyLib.Bar", "public void DoThing(string s)")],
            HasBreakingChanges: true);

        // Act
        var markdown = ChangelogRenderer.Render(diff, "2.0.0", new DateTimeOffset(2026, 4, 6, 0, 0, 0, TimeSpan.Zero));

        // Assert
        markdown.Should().Contain("### Breaking Changes");
        markdown.Should().Contain("Removed type `MyLib.Foo`");
        markdown.Should().Contain("Removed member `public void DoThing(string s)`");
    }

    [Fact]
    public void ChangelogRenderer_WithAdditions_ContainsAdditionSection()
    {
        // Arrange
        var diff = new ApiDiff(
            AddedTypes: ["MyLib.NewThing"],
            RemovedTypes: [],
            AddedMembers: [new ApiMemberDiff("MyLib.Foo", "public void NewMethod()")],
            RemovedMembers: [],
            HasBreakingChanges: false);

        // Act
        var markdown = ChangelogRenderer.Render(diff, "1.1.0", new DateTimeOffset(2026, 4, 6, 0, 0, 0, TimeSpan.Zero));

        // Assert
        markdown.Should().Contain("### Additions");
        markdown.Should().Contain("Added type `MyLib.NewThing`");
        markdown.Should().Contain("Added member `public void NewMethod()`");
        markdown.Should().NotContain("### Breaking Changes");
    }

    [Fact]
    public void ChangelogRenderer_NoChanges_ContainsNoChangesSummary()
    {
        // Arrange
        var diff = new ApiDiff(
            AddedTypes: [],
            RemovedTypes: [],
            AddedMembers: [],
            RemovedMembers: [],
            HasBreakingChanges: false);

        // Act
        var markdown = ChangelogRenderer.Render(diff, "1.0.1", new DateTimeOffset(2026, 4, 6, 0, 0, 0, TimeSpan.Zero));

        // Assert
        markdown.Should().Contain("No API changes");
        markdown.Should().NotContain("### Breaking Changes");
        markdown.Should().NotContain("### Additions");
    }

    [Fact]
    public void ChangelogRenderer_IncludesVersionAndDate()
    {
        // Arrange
        var diff = new ApiDiff([], [], [], [], false);
        var date = new DateTimeOffset(2026, 4, 6, 0, 0, 0, TimeSpan.Zero);

        // Act
        var markdown = ChangelogRenderer.Render(diff, "3.0.0", date);

        // Assert
        markdown.Should().Contain("v3.0.0");
        markdown.Should().Contain("2026-04-06");
    }

    // ── Helpers ────────────────────────────────────────────────────────────────

    private static ApiSurface BuildSurface(string name, string version, IReadOnlyList<ApiType> types) =>
        new(name, version, types);
}
