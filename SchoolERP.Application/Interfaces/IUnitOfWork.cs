namespace SchoolERP.Application.Interfaces;

/// <summary>
/// The save and transaction boundary services use. All repositories of a request share one unit of work, so saving through
/// it (or through any repository) saves every pending change.
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct = default);

    /// <summary>Runs <paramref name="work"/> in one transaction (joins the current one if a transaction is already open).</summary>
    Task<T> InTransactionAsync<T>(Func<Task<T>> work, CancellationToken ct = default);
}
