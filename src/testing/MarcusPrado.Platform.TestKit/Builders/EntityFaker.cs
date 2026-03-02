namespace MarcusPrado.Platform.TestKit.Builders;

/// <summary>
/// Lightweight base class for building test entity instances with sensible
/// random defaults.  Override <see cref="Build"/> to produce the desired type.
/// </summary>
/// <typeparam name="T">The entity type to build.</typeparam>
public abstract class EntityFaker<T>
    where T : class
{
    private static readonly Random Rng = Random.Shared;

    // ── Helper generators ─────────────────────────────────────────────────────

    /// <summary>Returns a new deterministic <see cref="Guid"/> for use as an ID.</summary>
    protected static Guid NewId() => Guid.NewGuid();

    /// <summary>Returns a random alphanumeric string of the given length.</summary>
    protected static string RandomString(int length = 10)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        return new string(Enumerable.Range(0, length)
            .Select(_ => chars[Rng.Next(chars.Length)])
            .ToArray());
    }

    /// <summary>Returns a random email address.</summary>
    protected static string RandomEmail()
        => $"{RandomString(8).ToLowerInvariant()}@{RandomString(6).ToLowerInvariant()}.test";

    /// <summary>Returns a random decimal value between 0.01 and 9999.99.</summary>
    protected static decimal RandomDecimal(decimal min = 0.01m, decimal max = 9_999.99m)
        => Math.Round((decimal)(Rng.NextDouble() * (double)(max - min)) + min, 2);

    /// <summary>Returns a random positive integer.</summary>
    protected static int RandomInt(int min = 1, int max = 1000) => Rng.Next(min, max + 1);

    /// <summary>Returns a random past <see cref="DateTimeOffset"/> within the last year.</summary>
    protected static DateTimeOffset RandomPastDate()
        => DateTimeOffset.UtcNow.AddDays(-Rng.Next(1, 365));

    /// <summary>Returns a random future <see cref="DateTimeOffset"/> within the next year.</summary>
    protected static DateTimeOffset RandomFutureDate()
        => DateTimeOffset.UtcNow.AddDays(Rng.Next(1, 365));

    /// <summary>Picks a random element from a list of values.</summary>
    protected static TValue PickRandom<TValue>(params TValue[] values)
        => values[Rng.Next(values.Length)];

    // ── Build ─────────────────────────────────────────────────────────────────

    /// <summary>Builds a single instance of <typeparamref name="T"/> with random data.</summary>
    public abstract T Build();

    /// <summary>Builds a list of <paramref name="count"/> instances.</summary>
    public List<T> BuildMany(int count = 3)
        => Enumerable.Range(0, count).Select(_ => Build()).ToList();
}
