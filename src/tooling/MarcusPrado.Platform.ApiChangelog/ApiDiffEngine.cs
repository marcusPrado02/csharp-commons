// <copyright file="ApiDiffEngine.cs" company="MarcusPrado">
// Copyright (c) MarcusPrado. All rights reserved.
// </copyright>

namespace MarcusPrado.Platform.ApiChangelog;

/// <summary>
/// Represents the diff between two <see cref="ApiSurface"/> snapshots.
/// </summary>
/// <param name="AddedTypes">Fully-qualified names of types added in <c>current</c> but absent in <c>baseline</c>.</param>
/// <param name="RemovedTypes">Fully-qualified names of types present in <c>baseline</c> but absent in <c>current</c> (breaking).</param>
/// <param name="AddedMembers">Members added in <c>current</c> but absent in <c>baseline</c>.</param>
/// <param name="RemovedMembers">Members present in <c>baseline</c> but absent in <c>current</c> (breaking).</param>
/// <param name="HasBreakingChanges">
/// <c>true</c> when there are removed types or removed members; otherwise <c>false</c>.
/// </param>
public sealed record ApiDiff(
    IReadOnlyList<string> AddedTypes,
    IReadOnlyList<string> RemovedTypes,
    IReadOnlyList<ApiMemberDiff> AddedMembers,
    IReadOnlyList<ApiMemberDiff> RemovedMembers,
    bool HasBreakingChanges);

/// <summary>
/// Identifies a single member change within a diff.
/// </summary>
/// <param name="TypeName">The fully-qualified name of the owning type.</param>
/// <param name="MemberSignature">The full member signature.</param>
public sealed record ApiMemberDiff(string TypeName, string MemberSignature);

/// <summary>
/// Compares two <see cref="ApiSurface"/> snapshots and produces an <see cref="ApiDiff"/>.
/// </summary>
public static class ApiDiffEngine
{
    /// <summary>
    /// Compares <paramref name="baseline"/> against <paramref name="current"/> and returns a diff.
    /// </summary>
    /// <param name="baseline">The earlier (reference) API surface.</param>
    /// <param name="current">The newer API surface to compare against the baseline.</param>
    /// <returns>An <see cref="ApiDiff"/> describing additions and removals.</returns>
    /// <exception cref="ArgumentNullException">Thrown when either argument is <c>null</c>.</exception>
    public static ApiDiff Compare(ApiSurface baseline, ApiSurface current)
    {
        ArgumentNullException.ThrowIfNull(baseline);
        ArgumentNullException.ThrowIfNull(current);

        var baselineTypes = baseline.Types.ToDictionary(t => t.FullName, StringComparer.Ordinal);
        var currentTypes = current.Types.ToDictionary(t => t.FullName, StringComparer.Ordinal);

        var addedTypes = currentTypes.Keys
            .Except(baselineTypes.Keys, StringComparer.Ordinal)
            .OrderBy(n => n, StringComparer.Ordinal)
            .ToList();

        var removedTypes = baselineTypes.Keys
            .Except(currentTypes.Keys, StringComparer.Ordinal)
            .OrderBy(n => n, StringComparer.Ordinal)
            .ToList();

        var addedMembers = new List<ApiMemberDiff>();
        var removedMembers = new List<ApiMemberDiff>();

        // Only compare members for types present in both snapshots
        var commonTypeNames = baselineTypes.Keys
            .Intersect(currentTypes.Keys, StringComparer.Ordinal);

        foreach (var typeName in commonTypeNames)
        {
            var baselineMembers = baselineTypes[typeName].Members
                .Select(m => m.Signature)
                .ToHashSet(StringComparer.Ordinal);

            var currentMembers = currentTypes[typeName].Members
                .Select(m => m.Signature)
                .ToHashSet(StringComparer.Ordinal);

            foreach (var sig in currentMembers.Except(baselineMembers, StringComparer.Ordinal).OrderBy(s => s, StringComparer.Ordinal))
            {
                addedMembers.Add(new ApiMemberDiff(typeName, sig));
            }

            foreach (var sig in baselineMembers.Except(currentMembers, StringComparer.Ordinal).OrderBy(s => s, StringComparer.Ordinal))
            {
                removedMembers.Add(new ApiMemberDiff(typeName, sig));
            }
        }

        var hasBreakingChanges = removedTypes.Count > 0 || removedMembers.Count > 0;

        return new ApiDiff(addedTypes, removedTypes, addedMembers, removedMembers, hasBreakingChanges);
    }
}
