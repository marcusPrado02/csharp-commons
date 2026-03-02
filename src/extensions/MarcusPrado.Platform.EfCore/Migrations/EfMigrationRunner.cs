using MarcusPrado.Platform.EfCore.DbContext;
using Microsoft.EntityFrameworkCore;

namespace MarcusPrado.Platform.EfCore.Migrations;

/// <summary>Runs pending EF Core migrations at application startup.</summary>
public sealed class EfMigrationRunner
{
    private readonly AppDbContextBase _context;

    /// <summary>Initialises the runner with the given db context.</summary>
    public EfMigrationRunner(AppDbContextBase context)
    {
        ArgumentNullException.ThrowIfNull(context);
        _context = context;
    }

    /// <summary>Applies all pending migrations asynchronously.</summary>
    public async Task RunMigrationsAsync(CancellationToken ct = default)
    {
        await _context.Database.MigrateAsync(ct);
    }

    /// <summary>Returns the list of migration names that have not yet been applied.</summary>
    public async Task<IReadOnlyList<string>> GetPendingMigrationsAsync(CancellationToken ct = default)
    {
        var pending = await _context.Database.GetPendingMigrationsAsync(ct);
        return pending.ToList();
    }
}
