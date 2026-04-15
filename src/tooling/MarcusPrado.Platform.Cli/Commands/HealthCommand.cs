// <copyright file="HealthCommand.cs" company="MarcusPrado">
// Copyright (c) MarcusPrado. All rights reserved.
// </copyright>

namespace MarcusPrado.Platform.Cli.Commands;

/// <summary>
/// Handles the <c>health</c> command.
/// </summary>
public static class HealthCommand
{
    /// <summary>Executes the health command asynchronously.</summary>
    /// <param name="args">Sub-arguments after "health".</param>
    /// <returns>Exit code.</returns>
    public static async Task<int> ExecuteAsync(string[] args)
    {
        if (args.Length == 0)
        {
            await Console.Error.WriteLineAsync("Usage: platform health <url>").ConfigureAwait(false);
            return 1;
        }

        var url = args[0].TrimEnd('/') + "/health";
        try
        {
            using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
            var response = await client.GetAsync(url).ConfigureAwait(false);
            await Console
                .Out.WriteLineAsync($"{url}: {(int)response.StatusCode} {response.ReasonPhrase}")
                .ConfigureAwait(false);
            return response.IsSuccessStatusCode ? 0 : 2;
        }
        catch (HttpRequestException ex)
        {
            await Console.Error.WriteLineAsync($"Failed to connect to {url}: {ex.Message}").ConfigureAwait(false);
            return 1;
        }
        catch (TaskCanceledException)
        {
            await Console.Error.WriteLineAsync($"Timeout connecting to {url}").ConfigureAwait(false);
            return 1;
        }
    }
}
