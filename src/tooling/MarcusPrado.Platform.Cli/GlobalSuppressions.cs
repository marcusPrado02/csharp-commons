// <copyright file="GlobalSuppressions.cs" company="MarcusPrado">
// Copyright (c) MarcusPrado. All rights reserved.
// </copyright>

// SA1516 fires on top-level statement files where blank lines between "elements"
// are not applicable in the same way as in normal C# files.
using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage(
    "StyleCop.CSharp.LayoutRules",
    "SA1516:Elements should be separated by blank line",
    Justification = "Top-level statements in Program.cs",
    Scope = "namespaceanddescendants",
    Target = "~N:"
)]
