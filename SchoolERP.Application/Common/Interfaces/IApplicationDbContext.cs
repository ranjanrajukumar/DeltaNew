using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace SchoolERP.Application.Common.Interfaces;

/// <summary>
/// The database as the application services see it. Every table is reachable through <see cref="Set{TEntity}"/>;
/// reads skip soft-deleted rows automatically and <c>Remove</c> soft-deletes (with the Node app's cascade rules).
/// </summary>
public interface IApplicationDbContext
{
    DbSet<TEntity> Set<TEntity>() where TEntity : class;

    DatabaseFacade Database { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>Runs <paramref name="work"/> in one transaction (joins the current one if a transaction is already open).</summary>
    Task<T> InTransactionAsync<T>(Func<Task<T>> work, CancellationToken cancellationToken = default);
}
