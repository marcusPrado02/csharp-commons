namespace MarcusPrado.Platform.BackupRestore;

/// <summary>Restores data from a previously created backup.</summary>
public interface IRestoreService
{
    /// <summary>Restores a backup archive to the target directory.</summary>
    Task<Result> RestoreAsync(BackupEntry entry, string targetPath, CancellationToken ct = default);

    /// <summary>Restores the most recent backup taken at or before <paramref name="pointInTime"/>.</summary>
    Task<Result> RestorePointInTimeAsync(string backupDirectory, DateTimeOffset pointInTime, string targetPath, CancellationToken ct = default);

    /// <summary>Validates that a backup can be restored (checksum + entry count).</summary>
    Task<Result<RestoreValidationResult>> ValidateRestoreAsync(BackupEntry entry, CancellationToken ct = default);
}

/// <summary>Outcome of a restore validation.</summary>
public sealed record RestoreValidationResult(bool IsValid, int EntryCount, string? Error);
