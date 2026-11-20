using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Interfaces.Repositories;
using SchoolERP.Infrastructure.Persistence;

namespace SchoolERP.Infrastructure.Repositories;

/// <summary>EF Core implementation of <see cref="IRepository{T}"/>; module repositories derive from it.</summary>
public class Repository<T>(SchoolErpDbContext db) : IRepository<T> where T : class
{
    protected SchoolErpDbContext Db { get; } = db;

    protected DbSet<T> Set => Db.Set<T>();

    /// <summary>Read-only queries (no change tracking).</summary>
    protected IQueryable<T> Read => Set.AsNoTracking();

    public Task<T?> FindAsync(int id, CancellationToken ct = default) => Set.FirstOrDefaultAsync(e => EF.Property<int>(e, "Id") == id, ct);

    public Task<T?> GetAsync(int id, CancellationToken ct = default) => Read.FirstOrDefaultAsync(e => EF.Property<int>(e, "Id") == id, ct);

    public Task<bool> ExistsAsync(int id, CancellationToken ct = default) => Set.AnyAsync(e => EF.Property<int>(e, "Id") == id, ct);

    public Task<List<T>> ListNewestFirstAsync(CancellationToken ct = default) =>
        Read.OrderByDescending(e => EF.Property<int>(e, "Id")).ToListAsync(ct);

    public Task<List<T>> ListOrderedByAsync<TKey>(System.Linq.Expressions.Expression<Func<T, TKey>> orderBy, CancellationToken ct = default) =>
        Read.OrderBy(orderBy).ToListAsync(ct);

    public void Add(T entity) => Set.Add(entity);

    public void AddRange(IEnumerable<T> entities) => Set.AddRange(entities);

    public void Remove(T entity) => Set.Remove(entity);

    public void RemoveRange(IEnumerable<T> entities) => Set.RemoveRange(entities);

    public Task<int> SaveChangesAsync(CancellationToken ct = default) => Db.SaveChangesAsync(ct);
}
