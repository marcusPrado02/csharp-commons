// <copyright file="DlqCommand.cs" company="MarcusPrado">
// Copyright (c) MarcusPrado. All rights reserved.
// </copyright>

namespace MarcusPrado.Platform.Cli.Commands;

/// <summary>
/// Handles the <c>dlq</c> command.
/// </summary>
public static class DlqCommand
{
    /// <summary>Executes the dlq command.</summary>
    /// <param name="args">Sub-arguments after "dlq".</param>
    /// <returns>Exit code.</returns>
    public static int Execute(string[] args)
    {
        if (args.Length < 2 || args[0] != "inspect")
        {
            Console.Error.WriteLine("Usage: platform dlq inspect <url>");
            return 1;
        }

        Console.WriteLine($"DLQ at {args[1]}: (stub — connect to actual broker for real data)");
        return 0;
    }
}
