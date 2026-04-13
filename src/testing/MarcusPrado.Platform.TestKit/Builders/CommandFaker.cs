namespace MarcusPrado.Platform.TestKit.Builders;

/// <summary>
/// Lightweight base class for building test command / DTO instances with
/// sensible random defaults.
/// </summary>
/// <typeparam name="TCommand">The command or request type to build.</typeparam>
public abstract class CommandFaker<TCommand>
    where TCommand : class
{
    /// <summary>Returns a new random <see cref="Guid"/> for IDs.</summary>
    protected static Guid NewId() => Guid.NewGuid();

    /// <summary>Returns a random alphanumeric string of the given length.</summary>
    protected static string RandomString(int length = 10)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        return new string(Enumerable.Range(0, length)
            .Select(_ => chars[Random.Shared.Next(chars.Length)])
            .ToArray());
    }

    /// <summary>Returns a random valid-looking email address.</summary>
    protected static string RandomEmail()
        => $"{RandomString(8).ToLowerInvariant()}@{RandomString(6).ToLowerInvariant()}.test";

    /// <summary>Returns a random decimal.</summary>
    protected static decimal RandomDecimal(decimal min = 0.01m, decimal max = 9_999.99m)
        => Math.Round((decimal)(Random.Shared.NextDouble() * (double)(max - min)) + min, 2);

    /// <summary>Returns a random positive integer.</summary>
    protected static int RandomInt(int min = 1, int max = 1000) => Random.Shared.Next(min, max + 1);

    /// <summary>Picks a random element from the provided values.</summary>
    protected static T PickRandom<T>(params T[] values)
        => values[Random.Shared.Next(values.Length)];

    /// <summary>Builds a single instance with random valid data.</summary>
    public abstract TCommand Build();

    /// <summary>Builds <paramref name="count"/> instances.</summary>
    public List<TCommand> BuildMany(int count = 3)
        => Enumerable.Range(0, count).Select(_ => Build()).ToList();
}
