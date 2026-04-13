using System.IO.Compression;
using System.Security.Cryptography;
using System.Text.Json;

namespace MarcusPrado.Platform.BackupRestore;

/// <summary>
/// Filesystem-based backup service. Archives source directories as ZIP files and
/// stores a <c>.meta.json</c> sidecar with checksum information.
/// </summary>
public sealed class FilesystemBackupService : IBackupService, IRestoreService
{
    private static readonly JsonSerializerOptions MetaJsonOptions = new() { WriteIndented = true };

    // ── IBackupService ────────────────────────────────────────────────────────

    /// <inheritdoc />
    public async Task<Result<BackupEntry>> CreateFullBackupAsync(
        string sourcePath,
        string backupDirectory,
        CancellationToken ct = default)
    {
        if (!Directory.Exists(sourcePath))
        {
            return Error.Validation(
                "BACKUP.SOURCE_NOT_FOUND",
                $"Source directory '{sourcePath}' does not exist.");
        }

        Directory.CreateDirectory(backupDirectory);

        var id = Guid.NewGuid();
        var archiveName = $"{id:N}_full.zip";
        var archivePath = Path.Combine(backupDirectory, archiveName);

        await ZipFile.CreateFromDirectoryAsync(
            sourcePath,
            archivePath,
            CompressionLevel.Optimal,
            includeBaseDirectory: false,
            cancellationToken: ct).ConfigureAwait(false);

        var checksum = await ComputeChecksumAsync(archivePath, ct).ConfigureAwait(false);
        var info = new FileInfo(archivePath);

        var entry = new BackupEntry(id, archivePath, BackupType.Full, DateTimeOffset.UtcNow, info.Length, checksum, sourcePath);
        await WriteMetaAsync(entry, ct).ConfigureAwait(false);
        return entry;
    }

    /// <inheritdoc />
    public async Task<Result<BackupEntry>> CreateIncrementalBackupAsync(
        string sourcePath,
        string backupDirectory,
        DateTimeOffset since,
        CancellationToken ct = default)
    {
        if (!Directory.Exists(sourcePath))
        {
            return Error.Validation(
                "BACKUP.SOURCE_NOT_FOUND",
                $"Source directory '{sourcePath}' does not exist.");
        }

        var files = Directory
            .EnumerateFiles(sourcePath, "*", SearchOption.AllDirectories)
            .Where(f => File.GetLastWriteTimeUtc(f) > since.UtcDateTime)
            .ToList();

        if (files.Count == 0)
        {
            return Error.Validation("BACKUP.NOTHING_TO_BACKUP", "No files changed since the last backup.");
        }

        Directory.CreateDirectory(backupDirectory);

        var id = Guid.NewGuid();
        var archiveName = $"{id:N}_incr.zip";
        var archivePath = Path.Combine(backupDirectory, archiveName);

        await using (var archive = await ZipFile.OpenAsync(archivePath, ZipArchiveMode.Create, ct).ConfigureAwait(false))
        {
            foreach (var file in files)
            {
                var relative = Path.GetRelativePath(sourcePath, file);
                await archive.CreateEntryFromFileAsync(file, relative, CompressionLevel.Optimal, ct).ConfigureAwait(false);
            }
        }

        var checksum = await ComputeChecksumAsync(archivePath, ct).ConfigureAwait(false);
        var info = new FileInfo(archivePath);

        var entry = new BackupEntry(
            id,
            archivePath,
            BackupType.Incremental,
            DateTimeOffset.UtcNow,
            info.Length,
            checksum,
            sourcePath);

        await WriteMetaAsync(entry, ct).ConfigureAwait(false);
        return entry;
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<BackupEntry>> ListBackupsAsync(
        string backupDirectory,
        CancellationToken ct = default)
    {
        if (!Directory.Exists(backupDirectory))
        {
            return Task.FromResult<IReadOnlyList<BackupEntry>>([]);
        }

        var entries = Directory
            .EnumerateFiles(backupDirectory, "*.meta.json")
            .Select(metaPath =>
            {
                try
                {
                    var json = File.ReadAllText(metaPath);
                    return JsonSerializer.Deserialize<BackupEntry>(json);
                }
#pragma warning disable CA1031 // Intentional: skip unreadable meta files
                catch (Exception)
                {
                    return null;
                }
#pragma warning restore CA1031
            })
            .Where(e => e is not null)
            .Cast<BackupEntry>()
            .OrderByDescending(e => e.CreatedAt)
            .ToList();

        return Task.FromResult<IReadOnlyList<BackupEntry>>(entries);
    }

    /// <inheritdoc />
    public async Task<Result<bool>> VerifyBackupAsync(
        BackupEntry entry,
        CancellationToken ct = default)
    {
        if (!File.Exists(entry.ArchivePath))
        {
            return Error.NotFound("BACKUP.ARCHIVE_MISSING", $"Archive '{entry.ArchivePath}' not found.");
        }

        var checksum = await ComputeChecksumAsync(entry.ArchivePath, ct).ConfigureAwait(false);
        return checksum.Equals(entry.Checksum, StringComparison.OrdinalIgnoreCase);
    }

    /// <inheritdoc />
    public Task<Result> DeleteBackupAsync(BackupEntry entry, CancellationToken ct = default)
    {
        if (File.Exists(entry.ArchivePath))
        {
            File.Delete(entry.ArchivePath);
        }

        var meta = entry.ArchivePath + ".meta.json";
        if (File.Exists(meta))
        {
            File.Delete(meta);
        }

        return Task.FromResult(Result.Success());
    }

    // ── IRestoreService ───────────────────────────────────────────────────────

    /// <inheritdoc />
    public async Task<Result> RestoreAsync(BackupEntry entry, string targetPath, CancellationToken ct = default)
    {
        if (!File.Exists(entry.ArchivePath))
        {
            return Error.NotFound("BACKUP.ARCHIVE_MISSING", $"Archive '{entry.ArchivePath}' not found.");
        }

        Directory.CreateDirectory(targetPath);
        await ZipFile.ExtractToDirectoryAsync(entry.ArchivePath, targetPath, overwriteFiles: true, ct).ConfigureAwait(false);
        return Result.Success();
    }

    /// <inheritdoc />
    public async Task<Result> RestorePointInTimeAsync(
        string backupDirectory,
        DateTimeOffset pointInTime,
        string targetPath,
        CancellationToken ct = default)
    {
        var backups = await ListBackupsAsync(backupDirectory, ct).ConfigureAwait(false);
        var target = backups
            .Where(e => e.CreatedAt <= pointInTime)
            .OrderByDescending(e => e.CreatedAt)
            .FirstOrDefault();

        if (target is null)
        {
            return Error.NotFound(
                "BACKUP.NO_BACKUP_FOR_POINT_IN_TIME",
                $"No backup found at or before '{pointInTime:O}'.");
        }

        return await RestoreAsync(target, targetPath, ct).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<Result<RestoreValidationResult>> ValidateRestoreAsync(
        BackupEntry entry,
        CancellationToken ct = default)
    {
        var verify = await VerifyBackupAsync(entry, ct).ConfigureAwait(false);
        if (verify.IsFailure)
        {
            return verify.Error;
        }

        if (!verify.Value)
        {
            return new RestoreValidationResult(false, 0, "Checksum mismatch — archive may be corrupted.");
        }

        await using var archive = await ZipFile.OpenReadAsync(entry.ArchivePath, ct).ConfigureAwait(false);
        return new RestoreValidationResult(true, archive.Entries.Count, null);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static async Task<string> ComputeChecksumAsync(string path, CancellationToken ct)
    {
        await using var stream = File.OpenRead(path);
        var hash = await SHA256.HashDataAsync(stream, ct).ConfigureAwait(false);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private static async Task WriteMetaAsync(BackupEntry entry, CancellationToken ct)
    {
        var metaPath = entry.ArchivePath + ".meta.json";
        var json = JsonSerializer.Serialize(entry, MetaJsonOptions);
        await File.WriteAllTextAsync(metaPath, json, ct).ConfigureAwait(false);
    }
}
