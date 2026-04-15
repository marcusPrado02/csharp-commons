// <copyright file="Program.cs" company="MarcusPrado">
// Copyright (c) MarcusPrado. All rights reserved.
// </copyright>

// SA1516: top-level statements do not require blank-line separators between elements.
#pragma warning disable SA1516

using MarcusPrado.Platform.Cli.Commands;

var args2 = args;
if (args2.Length == 0)
{
    return PrintHelp();
}

return args2[0] switch
{
    "scaffold" => ScaffoldCommand.Execute(args2[1..]),
    "config" => ConfigCommand.Execute(args2[1..]),
    "catalog" => CatalogCommand.Execute(args2[1..]),
    "arch" => ArchCommand.Execute(args2[1..]),
    "dlq" => DlqCommand.Execute(args2[1..]),
    "health" => await HealthCommand.ExecuteAsync(args2[1..]).ConfigureAwait(false),
    _ => PrintHelp(),
};

static int PrintHelp()
{
    Console.WriteLine("platform — MarcusPrado Platform CLI");
    Console.WriteLine();
    Console.WriteLine("Commands:");
    Console.WriteLine("  scaffold <api|worker|domain|command> [name]  Scaffold from dotnet new template");
    Console.WriteLine("  config encrypt <value>                        Encrypt a configuration value");
    Console.WriteLine("  catalog errors                                List all errors from ErrorCatalog");
    Console.WriteLine("  arch validate                                 Run architecture validation checks");
    Console.WriteLine("  dlq inspect <url>                             Inspect a Dead Letter Queue");
    Console.WriteLine("  health <url>                                  Check health endpoint");
    return 1;
}
