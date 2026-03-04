namespace MarcusPrado.Platform.BackupRestore;

/// <summary>Creates and manages data backups.</summary>
public interface IBackupService
{
    /// <summary>Creates a full backup of the given source directory.</summary>
    Task<Result<BackupEntry>> CreateFullBackupAsync(string sourcePath, string backupDirectory, CancellationToken ct = default);

    /// <summary>Creates an incremental backup (only files changed since last backup).</summary>
    Task<Result<BackupEntry>> CreateIncrementalBackupAsync(string sourcePath, string backupDirectory, DateTimeOffset since, CancellationToken ct = default);

    /// <summary>Lists all backup entries in the given directory.</summary>
    Task<IReadOnlyList<BackupEntry>> ListBackupsAsync(string backupDirectory, CancellationToken ct = default);

    /// <summary>Verifies the SHA-256 checksum of a backup archive.</summary>
    Task<Result<bool>> VerifyBackupAsync(BackupEntry entry, CancellationToken ct = default);

    /// <summary>Deletes a backup archive and its metadata.</summary>
    Task<Result> DeleteBackupAsync(BackupEntry entry, CancellationToken ct = default);
}
