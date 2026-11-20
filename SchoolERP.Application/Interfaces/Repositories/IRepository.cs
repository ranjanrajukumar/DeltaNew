namespace SchoolERP.Application.Interfaces.Repositories;

/// <summary>
/// The basic operations every table supports. Reads skip soft-deleted rows; <see cref="Remove"/> soft-deletes with the Node
/// app's cascade rules when saved. Module repositories extend this with their own queries.
/// </summary>
public interface IRepository<T> where T : class
{
    /// <summary>The row by id, tracked so changes to it are saved; null when it does not exist.</summary>
    Task<T?> FindAsync(int id, CancellationToken ct = default);

    /// <summary>The row by id for reading only; null when it does not exist.</summary>
    Task<T?> GetAsync(int id, CancellationToken ct = default);

    Task<bool> ExistsAsync(int id, CancellationToken ct = default);

    /// <summary>Every row, newest (highest id) first.</summary>
    Task<List<T>> ListNewestFirstAsync(CancellationToken ct = default);

    /// <summary>Every row, ordered by one column (ascending).</summary>
    Task<List<T>> ListOrderedByAsync<TKey>(System.Linq.Expressions.Expression<Func<T, TKey>> orderBy, CancellationToken ct = default);

    void Add(T entity);

    void AddRange(IEnumerable<T> entities);

    void Remove(T entity);

    void RemoveRange(IEnumerable<T> entities);

    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
