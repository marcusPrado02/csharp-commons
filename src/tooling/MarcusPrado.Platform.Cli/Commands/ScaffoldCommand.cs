// <copyright file="ScaffoldCommand.cs" company="MarcusPrado">
// Copyright (c) MarcusPrado. All rights reserved.
// </copyright>

namespace MarcusPrado.Platform.Cli.Commands;

/// <summary>
/// Handles the <c>scaffold</c> command.
/// </summary>
public static class ScaffoldCommand
{
    /// <summary>Executes the scaffold command.</summary>
    /// <param name="args">Sub-arguments after "scaffold".</param>
    /// <returns>Exit code.</returns>
    public static int Execute(string[] args)
    {
        if (args.Length == 0)
        {
            Console.Error.WriteLine("Usage: platform scaffold <api|worker|domain|command> [name]");
            return 1;
        }

        var template = $"platform-{args[0]}";
        var name = args.Length > 1 ? args[1] : "MyService";
        Console.WriteLine($"Scaffolding {template} as {name}...");
        Console.WriteLine($"Run: dotnet new {template} --name {name}");
        return 0;
    }
}
