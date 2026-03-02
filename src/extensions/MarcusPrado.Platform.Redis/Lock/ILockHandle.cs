namespace MarcusPrado.Platform.Redis.Lock;

/// <summary>Represents a held distributed lock; dispose to release it.</summary>
public interface ILockHandle : IAsyncDisposable
{
    /// <summary>The resource key that was locked.</summary>
    string Key { get; }

    /// <summary>Unique fencing token for this acquisition.</summary>
    string Token { get; }

    /// <summary>Whether the lock is still logically held by this handle.</summary>
    bool IsHeld { get; }
}
