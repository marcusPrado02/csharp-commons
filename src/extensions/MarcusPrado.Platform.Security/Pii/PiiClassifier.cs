using System.Reflection;

namespace MarcusPrado.Platform.Security.Pii;

public static class PiiClassifier
{
    /// <summary>
    /// Returns a dictionary of property names → PII type for all properties
    /// decorated with [PiiData] on the given type.
    /// </summary>
    public static IReadOnlyDictionary<string, PiiType> GetPiiProperties(Type type)
    {
        ArgumentNullException.ThrowIfNull(type);
        return type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Select(p => (p.Name, Attr: p.GetCustomAttribute<PiiDataAttribute>()))
            .Where(x => x.Attr is not null)
            .ToDictionary(x => x.Name, x => x.Attr!.Type);
    }

    /// <summary>
    /// Returns a redacted copy of the object's PII properties as a dictionary
    /// (non-PII properties are also included with original values for audit purposes).
    /// </summary>
    public static Dictionary<string, object?> Redact(object obj)
    {
        ArgumentNullException.ThrowIfNull(obj);
        var type = obj.GetType();
        var piiProps = GetPiiProperties(type);
        var result = new Dictionary<string, object?>();

        foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            var rawValue = prop.GetValue(obj)?.ToString();
            result[prop.Name] = piiProps.TryGetValue(prop.Name, out var piiType)
                ? PiiRedactor.Redact(rawValue, piiType)
                : prop.GetValue(obj);
        }

        return result;
    }
}
