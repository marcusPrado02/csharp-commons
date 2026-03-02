namespace MarcusPrado.Platform.TestKit.Helpers;

/// <summary>Polling helper for asserting eventual consistency in async scenarios.</summary>
public static class Eventually
{
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(10);
    private static readonly TimeSpan DefaultInterval = TimeSpan.FromMilliseconds(100);

    /// <summary>
    /// Polls <paramref name="condition"/> every <paramref name="interval"/> until it returns
    /// <c>true</c> or <paramref name="timeout"/> elapses, then throws if still false.
    /// </summary>
    public static async Task BecomesTrue(
        Func<bool> condition,
        TimeSpan? timeout = null,
        TimeSpan? interval = null,
        string? message = null)
    {
        var deadline = DateTimeOffset.UtcNow.Add(timeout ?? DefaultTimeout);
        var poll = interval ?? DefaultInterval;

        while (DateTimeOffset.UtcNow < deadline)
        {
            if (condition())
            {
                return;
            }

            await Task.Delay(poll);
        }

        throw new TimeoutException(
            message ?? $"Condition did not become true within {timeout ?? DefaultTimeout}.");
    }

    /// <summary>
    /// Async overload: polls <paramref name="condition"/> until it returns <c>true</c>
    /// or <paramref name="timeout"/> elapses.
    /// </summary>
    public static async Task BecomesTrue(
        Func<Task<bool>> condition,
        TimeSpan? timeout = null,
        TimeSpan? interval = null,
        string? message = null)
    {
        var deadline = DateTimeOffset.UtcNow.Add(timeout ?? DefaultTimeout);
        var poll = interval ?? DefaultInterval;

        while (DateTimeOffset.UtcNow < deadline)
        {
            if (await condition())
            {
                return;
            }

            await Task.Delay(poll);
        }

        throw new TimeoutException(
            message ?? $"Condition did not become true within {timeout ?? DefaultTimeout}.");
    }
}
