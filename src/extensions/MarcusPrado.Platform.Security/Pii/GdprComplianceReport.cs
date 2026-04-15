using System.Reflection;

namespace MarcusPrado.Platform.Security.Pii;

public sealed record PiiPropertyInfo(string TypeName, string PropertyName, PiiType PiiType);

public static class GdprComplianceReport
{
    /// <summary>Scans the given assemblies and returns all types that contain [PiiData] properties.</summary>
    public static IReadOnlyList<PiiPropertyInfo> Scan(params Assembly[] assemblies)
    {
        return assemblies
            .SelectMany(a => a.GetTypes())
            .SelectMany(t =>
                t.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Select(p => (Type: t, Prop: p, Attr: p.GetCustomAttribute<PiiDataAttribute>()))
                    .Where(x => x.Attr is not null)
                    .Select(x => new PiiPropertyInfo(x.Type.FullName ?? x.Type.Name, x.Prop.Name, x.Attr!.Type))
            )
            .ToList();
    }
}
