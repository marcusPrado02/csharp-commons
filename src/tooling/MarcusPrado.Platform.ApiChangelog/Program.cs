// <copyright file="Program.cs" company="MarcusPrado">
// Copyright (c) MarcusPrado. All rights reserved.
// </copyright>

// SA1516: top-level statements do not require blank-line separators.
#pragma warning disable SA1516

using System.Reflection;
using System.Text.Json;
using MarcusPrado.Platform.ApiChangelog;

if (args.Length == 0 || args[0] is "--help" or "-h")
{
    return PrintHelp();
}

return args[0] switch
{
    "extract" => Extract(args[1..]),
    "diff"    => Diff(args[1..]),
    _         => PrintError($"Unknown command '{args[0]}'. Run with --help for usage."),
};

// ── Commands ──────────────────────────────────────────────────────────────────

static int Extract(string[] args)
{
    if (args.Length == 0 || args[0].StartsWith('-'))
    {
        return PrintError("extract requires a path to the assembly DLL.");
    }

    var dllPath = args[0];
    var output  = GetArg(args[1..], "--output");

    if (!File.Exists(dllPath))
    {
        return PrintError($"Assembly not found: {dllPath}");
    }

    var assembly = Assembly.LoadFrom(dllPath);
    var surface  = ApiSurfaceExtractor.Extract(assembly);
    var json     = JsonSerializer.Serialize(surface, new JsonSerializerOptions { WriteIndented = true });

    if (output is not null)
    {
        var dir = Path.GetDirectoryName(output);
        if (!string.IsNullOrEmpty(dir))
        {
            Directory.CreateDirectory(dir);
        }

        File.WriteAllText(output, json);
        Console.Error.WriteLine($"API surface written to {output}");
    }
    else
    {
        Console.WriteLine(json);
    }

    return 0;
}

static int Diff(string[] args)
{
    var baseline = GetArg(args, "--baseline");
    var current  = GetArg(args, "--current");
    var version  = GetArg(args, "--version") ?? "current";
    var output   = GetArg(args, "--output");

    if (baseline is null) return PrintError("--baseline is required");
    if (current  is null) return PrintError("--current is required");
    if (!File.Exists(baseline)) return PrintError($"Baseline not found: {baseline}");
    if (!File.Exists(current))  return PrintError($"Current not found: {current}");

    var baselineSurface = JsonSerializer.Deserialize<ApiSurface>(File.ReadAllText(baseline))!;
    var currentSurface  = JsonSerializer.Deserialize<ApiSurface>(File.ReadAllText(current))!;

    var diff     = ApiDiffEngine.Compare(baselineSurface, currentSurface);
    var markdown = ChangelogRenderer.Render(diff, version, DateTimeOffset.UtcNow);

    if (output is not null)
    {
        File.AppendAllText(output, markdown);
    }
    else
    {
        Console.Write(markdown);
    }

    // Non-zero exit code on breaking changes so CI can block the PR.
    return diff.HasBreakingChanges ? 2 : 0;
}

// ── Helpers ───────────────────────────────────────────────────────────────────

static string? GetArg(string[] args, string flag)
{
    var idx = Array.IndexOf(args, flag);
    return idx >= 0 && idx + 1 < args.Length ? args[idx + 1] : null;
}

static int PrintError(string message)
{
    Console.Error.WriteLine($"error: {message}");
    return 1;
}

static int PrintHelp()
{
    Console.WriteLine("platform-api — MarcusPrado Platform API Surface Tool");
    Console.WriteLine();
    Console.WriteLine("Commands:");
    Console.WriteLine("  extract <dll> [--output <path>]");
    Console.WriteLine("      Extract the public API surface of an assembly to JSON.");
    Console.WriteLine();
    Console.WriteLine("  diff --baseline <json> --current <json> [--version <ver>] [--output <path>]");
    Console.WriteLine("      Diff two API surfaces and render a markdown changelog.");
    Console.WriteLine("      Exits with code 2 when breaking changes are detected.");
    Console.WriteLine();
    Console.WriteLine("Examples:");
    Console.WriteLine("  platform-api extract ./MarcusPrado.Platform.Abstractions.dll --output baseline.json");
    Console.WriteLine("  platform-api diff --baseline baseline.json --current current.json --version 1.3.0");
    return 0;
}
