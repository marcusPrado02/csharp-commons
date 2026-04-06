// <copyright file="ChangelogRenderer.cs" company="MarcusPrado">
// Copyright (c) MarcusPrado. All rights reserved.
// </copyright>

using System.Text;

namespace MarcusPrado.Platform.ApiChangelog;

/// <summary>
/// Renders an <see cref="ApiDiff"/> to a markdown API-CHANGELOG entry.
/// </summary>
public static class ChangelogRenderer
{
    /// <summary>
    /// Renders a markdown API changelog entry for the given diff.
    /// </summary>
    /// <param name="diff">The diff to render.</param>
    /// <param name="version">The version being released, e.g. <c>1.2.0</c>.</param>
    /// <param name="date">The release date.</param>
    /// <returns>A markdown string suitable for inclusion in <c>API-CHANGELOG.md</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="diff"/> or <paramref name="version"/> is <c>null</c>.</exception>
    public static string Render(ApiDiff diff, string version, DateTimeOffset date)
    {
        ArgumentNullException.ThrowIfNull(diff);
        ArgumentNullException.ThrowIfNull(version);

        var sb = new StringBuilder();
        sb.AppendLine($"## API Changes in v{version} ({date:yyyy-MM-dd})");
        sb.AppendLine();

        var hasBreaking = diff.RemovedTypes.Count > 0 || diff.RemovedMembers.Count > 0;
        var hasAdditions = diff.AddedTypes.Count > 0 || diff.AddedMembers.Count > 0;

        if (!hasBreaking && !hasAdditions)
        {
            sb.AppendLine("_No API changes in this release._");
            return sb.ToString();
        }

        if (hasBreaking)
        {
            sb.AppendLine("### Breaking Changes");
            foreach (var typeName in diff.RemovedTypes)
            {
                sb.AppendLine($"- Removed type `{typeName}`");
            }

            foreach (var member in diff.RemovedMembers)
            {
                sb.AppendLine($"- Removed member `{member.MemberSignature}` on `{member.TypeName}`");
            }

            sb.AppendLine();
        }

        if (hasAdditions)
        {
            sb.AppendLine("### Additions");
            foreach (var typeName in diff.AddedTypes)
            {
                sb.AppendLine($"- Added type `{typeName}`");
            }

            foreach (var member in diff.AddedMembers)
            {
                sb.AppendLine($"- Added member `{member.MemberSignature}` on `{member.TypeName}`");
            }

            sb.AppendLine();
        }

        return sb.ToString();
    }
}
