using MarcusPrado.Platform.Abstractions.Storage;
using MarcusPrado.Platform.EfCore.DbContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace MarcusPrado.Platform.EfCore.Uow;

/// <summary>
/// EF Core implementation of <see cref="IUnitOfWork"/> using a database transaction.
/// </summary>
public sealed class EfUnitOfWork : IUnitOfWork, IAsyncDisposable
{
    private readonly AppDbContextBase _context;
    private IDbContextTransaction? _transaction;

    /// <summary>Initialises the unit of work with the given db context.</summary>
    public EfUnitOfWork(AppDbContextBase context)
    {
        ArgumentNullException.ThrowIfNull(context);
        _context = context;
    }

    /// <inheritdoc/>
    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
        if (_transaction is not null)
        {
            await _transaction.CommitAsync(cancellationToken);
        }
    }

    /// <inheritdoc/>
    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction is not null)
        {
            await _transaction.RollbackAsync(cancellationToken);
        }
    }

    /// <inheritdoc/>
    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>Creates a savepoint within the active transaction.</summary>
    public async Task SavepointAsync(string name, CancellationToken cancellationToken = default)
    {
        if (_transaction is not null)
        {
            await _transaction.CreateSavepointAsync(name, cancellationToken);
        }
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        if (_transaction is not null)
        {
            await _transaction.DisposeAsync();
        }
    }
}
