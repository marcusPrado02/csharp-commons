namespace MarcusPrado.Platform.BackupRestore;

/// <summary>Metadata describing a single backup archive.</summary>
public sealed record BackupEntry(
    Guid Id,
    string ArchivePath,
    BackupType Type,
    DateTimeOffset CreatedAt,
    long SizeBytes,
    string Checksum,
    string SourcePath);

/// <summary>The type of a backup operation.</summary>
public enum BackupType
{
    /// <summary>Full backup of all files in the source.</summary>
    Full,

    /// <summary>Incremental backup of files changed since a given point.</summary>
    Incremental,
}
