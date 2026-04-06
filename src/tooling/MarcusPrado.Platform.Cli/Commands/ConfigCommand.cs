// <copyright file="ConfigCommand.cs" company="MarcusPrado">
// Copyright (c) MarcusPrado. All rights reserved.
// </copyright>

using System.Text;

namespace MarcusPrado.Platform.Cli.Commands;

/// <summary>
/// Handles the <c>config</c> command.
/// </summary>
public static class ConfigCommand
{
    /// <summary>Executes the config command.</summary>
    /// <param name="args">Sub-arguments after "config".</param>
    /// <returns>Exit code.</returns>
    public static int Execute(string[] args)
    {
        if (args.Length < 2 || args[0] != "encrypt")
        {
            Console.Error.WriteLine("Usage: platform config encrypt <value>");
            return 1;
        }

        var value = args[1];
        var encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(value));
        Console.WriteLine($"ENC({encoded})");
        return 0;
    }
}
