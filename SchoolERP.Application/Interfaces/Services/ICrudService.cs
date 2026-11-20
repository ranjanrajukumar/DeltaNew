using SchoolERP.Application.Common.Helpers;

namespace SchoolERP.Application.Interfaces.Services;

/// <summary>List, create, update and delete for one table with a fixed whitelist of writable fields (the Node app's crudRouter).</summary>
public interface ICrudService<T> where T : class
{
    Task<List<T>> ListAsync(CancellationToken ct = default);

    /// <summary>Every row ordered by one column (e.g. by name), for lists that are not newest-first.</summary>
    Task<List<T>> ListOrderedByAsync<TKey>(System.Linq.Expressions.Expression<Func<T, TKey>> orderBy, CancellationToken ct = default);

    Task<int> CreateAsync(Body body, IReadOnlyList<string> fields, CancellationToken ct = default);

    Task<bool> UpdateAsync(int id, Body body, IReadOnlyList<string> fields, CancellationToken ct = default);

    Task DeleteAsync(int id, CancellationToken ct = default);
}
