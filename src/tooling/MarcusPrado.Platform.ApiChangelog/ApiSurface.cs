// <copyright file="ApiSurface.cs" company="MarcusPrado">
// Copyright (c) MarcusPrado. All rights reserved.
// </copyright>

namespace MarcusPrado.Platform.ApiChangelog;

/// <summary>
/// Represents a snapshot of the public API surface of an assembly.
/// </summary>
/// <param name="AssemblyName">The simple name of the assembly.</param>
/// <param name="Version">The assembly version string.</param>
/// <param name="Types">All public types exposed by the assembly.</param>
public sealed record ApiSurface(string AssemblyName, string Version, IReadOnlyList<ApiType> Types);

/// <summary>
/// Represents a single public type in an API surface snapshot.
/// </summary>
/// <param name="FullName">The fully-qualified type name.</param>
/// <param name="Kind">The kind of type: <c>class</c>, <c>interface</c>, <c>enum</c>, <c>struct</c>, or <c>record</c>.</param>
/// <param name="Members">All public members declared on this type.</param>
public sealed record ApiType(string FullName, string Kind, IReadOnlyList<ApiMember> Members);

/// <summary>
/// Represents a single public member (method, property, field, or event) on a type.
/// </summary>
/// <param name="Name">The simple member name.</param>
/// <param name="Signature">
/// The full member signature, e.g. <c>public string GetFoo(int id)</c>.
/// </param>
/// <param name="Kind">The member kind: <c>method</c>, <c>property</c>, <c>field</c>, or <c>event</c>.</param>
public sealed record ApiMember(string Name, string Signature, string Kind);
