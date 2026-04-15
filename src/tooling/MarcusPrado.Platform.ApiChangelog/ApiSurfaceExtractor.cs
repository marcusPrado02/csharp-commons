// <copyright file="ApiSurfaceExtractor.cs" company="MarcusPrado">
// Copyright (c) MarcusPrado. All rights reserved.
// </copyright>

using System.Reflection;
using System.Text;

namespace MarcusPrado.Platform.ApiChangelog;

/// <summary>
/// Extracts the public API surface from an assembly via reflection.
/// </summary>
public static class ApiSurfaceExtractor
{
    /// <summary>
    /// Extracts the public API surface from the given assembly.
    /// </summary>
    /// <param name="assembly">The assembly to inspect.</param>
    /// <returns>An <see cref="ApiSurface"/> snapshot for the assembly.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="assembly"/> is <c>null</c>.</exception>
    public static ApiSurface Extract(Assembly assembly)
    {
        ArgumentNullException.ThrowIfNull(assembly);

        var version = assembly.GetName().Version?.ToString() ?? "0.0.0.0";
        var assemblyName = assembly.GetName().Name ?? assembly.FullName ?? string.Empty;

        var types = assembly
            .GetExportedTypes()
            .Where(t => !t.IsNested)
            .OrderBy(t => t.FullName, StringComparer.Ordinal)
            .Select(ExtractType)
            .ToList();

        return new ApiSurface(assemblyName, version, types);
    }

    private static ApiType ExtractType(Type type)
    {
        var kind = GetTypeKind(type);
        var members = ExtractMembers(type);
        return new ApiType(type.FullName ?? type.Name, kind, members);
    }

    private static string GetTypeKind(Type type)
    {
        if (type.IsEnum)
        {
            return "enum";
        }

        if (type.IsInterface)
        {
            return "interface";
        }

        if (type.IsValueType)
        {
            return "struct";
        }

        // Check for record: compiler emits <Clone>$ method on record classes
        if (type.IsClass && type.GetMethod("<Clone>$") is not null)
        {
            return "record";
        }

        return "class";
    }

    private static List<ApiMember> ExtractMembers(Type type)
    {
        const BindingFlags Flags =
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;

        var members = new List<ApiMember>();

        // Properties
        foreach (var prop in type.GetProperties(Flags).OrderBy(p => p.Name, StringComparer.Ordinal))
        {
            var sig = BuildPropertySignature(prop);
            members.Add(new ApiMember(prop.Name, sig, "property"));
        }

        // Methods (exclude property accessors, event accessors, record-generated methods)
        foreach (
            var method in type.GetMethods(Flags)
                .Where(m => !m.IsSpecialName)
                .OrderBy(m => m.Name, StringComparer.Ordinal)
                .ThenBy(m => m.ToString(), StringComparer.Ordinal)
        )
        {
            var sig = BuildMethodSignature(method);
            members.Add(new ApiMember(method.Name, sig, "method"));
        }

        // Fields
        foreach (var field in type.GetFields(Flags).OrderBy(f => f.Name, StringComparer.Ordinal))
        {
            var sig = BuildFieldSignature(field);
            members.Add(new ApiMember(field.Name, sig, "field"));
        }

        // Events
        foreach (var ev in type.GetEvents(Flags).OrderBy(e => e.Name, StringComparer.Ordinal))
        {
            var sig = $"public event {GetFriendlyTypeName(ev.EventHandlerType)} {ev.Name}";
            members.Add(new ApiMember(ev.Name, sig, "event"));
        }

        return members;
    }

    private static string BuildPropertySignature(PropertyInfo prop)
    {
        var sb = new StringBuilder("public ");
        sb.Append(GetFriendlyTypeName(prop.PropertyType));
        sb.Append(' ');
        sb.Append(prop.Name);
        sb.Append(" { ");
        if (prop.CanRead)
        {
            sb.Append("get; ");
        }

        if (prop.CanWrite)
        {
            sb.Append("set; ");
        }

        sb.Append('}');
        return sb.ToString();
    }

    private static string BuildMethodSignature(MethodInfo method)
    {
        var sb = new StringBuilder("public ");
        if (method.IsStatic)
        {
            sb.Append("static ");
        }

        sb.Append(GetFriendlyTypeName(method.ReturnType));
        sb.Append(' ');
        sb.Append(method.Name);
        sb.Append('(');

        var parameters = method.GetParameters();
        for (var i = 0; i < parameters.Length; i++)
        {
            if (i > 0)
            {
                sb.Append(", ");
            }

            sb.Append(GetFriendlyTypeName(parameters[i].ParameterType));
            sb.Append(' ');
            sb.Append(parameters[i].Name);
        }

        sb.Append(')');
        return sb.ToString();
    }

    private static string BuildFieldSignature(FieldInfo field)
    {
        var sb = new StringBuilder("public ");
        if (field.IsStatic)
        {
            sb.Append("static ");
        }

        if (field.IsInitOnly)
        {
            sb.Append("readonly ");
        }

        sb.Append(GetFriendlyTypeName(field.FieldType));
        sb.Append(' ');
        sb.Append(field.Name);
        return sb.ToString();
    }

    private static string GetFriendlyTypeName(Type? type)
    {
        if (type is null)
        {
            return "void";
        }

        if (type == typeof(void))
        {
            return "void";
        }

        if (type == typeof(string))
        {
            return "string";
        }

        if (type == typeof(bool))
        {
            return "bool";
        }

        if (type == typeof(int))
        {
            return "int";
        }

        if (type == typeof(long))
        {
            return "long";
        }

        if (type == typeof(double))
        {
            return "double";
        }

        if (type == typeof(float))
        {
            return "float";
        }

        if (type == typeof(decimal))
        {
            return "decimal";
        }

        if (type == typeof(object))
        {
            return "object";
        }

        if (type == typeof(byte))
        {
            return "byte";
        }

        if (type == typeof(char))
        {
            return "char";
        }

        if (type == typeof(short))
        {
            return "short";
        }

        if (type == typeof(uint))
        {
            return "uint";
        }

        if (type == typeof(ulong))
        {
            return "ulong";
        }

        if (type.IsGenericType)
        {
            var genericDef = type.GetGenericTypeDefinition();
            var genericName = genericDef.Name;
            var backtickIndex = genericName.IndexOf('`', StringComparison.Ordinal);
            if (backtickIndex > 0)
            {
                genericName = genericName[..backtickIndex];
            }

            var args = type.GetGenericArguments().Select(GetFriendlyTypeName);
            return $"{genericName}<{string.Join(", ", args)}>";
        }

        return type.Name;
    }
}
