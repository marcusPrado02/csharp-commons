// <copyright file="CatalogCommand.cs" company="MarcusPrado">
// Copyright (c) MarcusPrado. All rights reserved.
// </copyright>

namespace MarcusPrado.Platform.Cli.Commands;

/// <summary>
/// Handles the <c>catalog</c> command.
/// </summary>
public static class CatalogCommand
{
    /// <summary>Executes the catalog command.</summary>
    /// <param name="args">Sub-arguments after "catalog".</param>
    /// <returns>Exit code.</returns>
    public static int Execute(string[] args)
    {
        if (args.Length == 0 || args[0] != "errors")
        {
            Console.Error.WriteLine("Usage: platform catalog errors");
            return 1;
        }

        Console.WriteLine("Error Catalog:");
        Console.WriteLine("  PAYMENT_001  NotFound     Payment not found.");
        Console.WriteLine("  PAYMENT_002  Validation   Insufficient funds.");
        Console.WriteLine("  AUTH_001     Unauthorized Unauthenticated request.");
        Console.WriteLine("  AUTH_002     Forbidden    Access denied.");
        Console.WriteLine("  GENERIC_001  Internal     Unexpected server error.");
        return 0;
    }
}
