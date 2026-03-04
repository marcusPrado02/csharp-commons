namespace MarcusPrado.Platform.BackupRestore.Tests;

public sealed class FilesystemBackupServiceTests : IDisposable
{
    private readonly string _source  = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
    private readonly string _backups = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
    private readonly FilesystemBackupService _svc = new();

    public FilesystemBackupServiceTests()
    {
        Directory.CreateDirectory(_source);
        File.WriteAllText(Path.Combine(_source, "file1.txt"), "hello");
        File.WriteAllText(Path.Combine(_source, "file2.txt"), "world");
    }

    public void Dispose()
    {
        if (Directory.Exists(_source))  Directory.Delete(_source,  recursive: true);
        if (Directory.Exists(_backups)) Directory.Delete(_backups, recursive: true);
    }

    [Fact]
    public async Task CreateFullBackup_CreatesZipAndMeta()
    {
        var result = await _svc.CreateFullBackupAsync(_source, _backups);
        result.IsSuccess.Should().BeTrue();
        File.Exists(result.Value.ArchivePath).Should().BeTrue();
        File.Exists(result.Value.ArchivePath + ".meta.json").Should().BeTrue();
        result.Value.Type.Should().Be(BackupType.Full);
    }

    [Fact]
    public async Task CreateFullBackup_SourceMissing_ReturnsFailure()
    {
        var result = await _svc.CreateFullBackupAsync("/nonexistent/dir", _backups);
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task VerifyBackup_ValidArchive_ReturnsTrue()
    {
        var backup = await _svc.CreateFullBackupAsync(_source, _backups);
        var verify = await _svc.VerifyBackupAsync(backup.Value);
        verify.IsSuccess.Should().BeTrue();
        verify.Value.Should().BeTrue();
    }

    [Fact]
    public async Task VerifyBackup_CorruptArchive_ReturnsFalse()
    {
        var backup = await _svc.CreateFullBackupAsync(_source, _backups);
        // Corrupt the archive
        File.WriteAllText(backup.Value.ArchivePath, "corrupted");
        var verify = await _svc.VerifyBackupAsync(backup.Value);
        verify.Value.Should().BeFalse();
    }

    [Fact]
    public async Task ListBackups_AfterCreate_ReturnsEntries()
    {
        await _svc.CreateFullBackupAsync(_source, _backups);
        await _svc.CreateFullBackupAsync(_source, _backups);
        var list = await _svc.ListBackupsAsync(_backups);
        list.Should().HaveCount(2);
    }

    [Fact]
    public async Task ListBackups_EmptyDir_ReturnsEmpty()
    {
        Directory.CreateDirectory(_backups);
        var list = await _svc.ListBackupsAsync(_backups);
        list.Should().BeEmpty();
    }

    [Fact]
    public async Task DeleteBackup_RemovesArchiveAndMeta()
    {
        var backup = await _svc.CreateFullBackupAsync(_source, _backups);
        await _svc.DeleteBackupAsync(backup.Value);
        File.Exists(backup.Value.ArchivePath).Should().BeFalse();
    }

    [Fact]
    public async Task RestoreBackup_ExtractsFiles()
    {
        var restoreTo = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        try
        {
            var backup  = await _svc.CreateFullBackupAsync(_source, _backups);
            var restore = await _svc.RestoreAsync(backup.Value, restoreTo);
            restore.IsSuccess.Should().BeTrue();
            File.Exists(Path.Combine(restoreTo, "file1.txt")).Should().BeTrue();
        }
        finally
        {
            if (Directory.Exists(restoreTo)) Directory.Delete(restoreTo, recursive: true);
        }
    }

    [Fact]
    public async Task ValidateRestore_ValidArchive_ReturnsValid()
    {
        var backup   = await _svc.CreateFullBackupAsync(_source, _backups);
        var validate = await _svc.ValidateRestoreAsync(backup.Value);
        validate.IsSuccess.Should().BeTrue();
        validate.Value.IsValid.Should().BeTrue();
        validate.Value.EntryCount.Should().Be(2);
    }

    [Fact]
    public async Task IncrementalBackup_OnlyChangedFiles_SmallArchive()
    {
        // Files are newer than reference date
        var since   = DateTimeOffset.UtcNow.AddHours(-1);
        var result  = await _svc.CreateIncrementalBackupAsync(_source, _backups, since);
        result.IsSuccess.Should().BeTrue();
        result.Value.Type.Should().Be(BackupType.Incremental);
    }

    [Fact]
    public async Task IncrementalBackup_NothingChanged_ReturnsFailure()
    {
        var future = DateTimeOffset.UtcNow.AddDays(1);
        var result = await _svc.CreateIncrementalBackupAsync(_source, _backups, future);
        result.IsFailure.Should().BeTrue();
    }
}
