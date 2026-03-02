namespace MarcusPrado.Platform.FeatureFlags.Internal;

/// <summary>Provides a stable, platform-independent hash for strings.</summary>
internal static class DeterministicHashExtensions
{
    /// <summary>Returns a deterministic 32-bit hash that is stable across process restarts.</summary>
    internal static int GetDeterministicHash(this string value)
    {
        unchecked
        {
            var hash = 17;
            foreach (var c in value)
            {
                hash = (hash * 31) + c;
            }

            return hash;
        }
    }
}
